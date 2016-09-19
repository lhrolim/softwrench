using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Quartz.Util;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class BaseLocationDataSet : MaximoApplicationDataSet {
        private const string CteTemplate = @"classificationhierarchy{1} ( classstructureid, classificationid, parent, level )
                                             AS
                                             (
                                             select	cs.classstructureid, c.classificationid, cs.parent, 0 AS Level
                                             from	classstructure cs
                                             inner join classification c on cs.classificationid = c.classificationid
                                             where cs.classificationid = '{0}'
                                             UNION ALL
                                             select	cs.classstructureid, c.classificationid, cs.parent, Level + 1
                                             from	classstructure cs
                                             inner join classification c on cs.classificationid = c.classificationid
                                             inner join classificationhierarchy{1} ch on cs.classstructureid = ch.parent
                                             )
                                             ";
        private const string SelectTemplate = @"select distinct '{0}' as id, 
	                                            SUBSTRING(
		                                          (
			                                        select ' / '+ch1.classificationid as [text()]
			                                        from classificationhierarchy{1} ch1
			                                        order by ch1.level desc
			                                        For XML PATH ('')
		                                          ), 3, 1000) [hierarchypath]
                                                from classificationhierarchy{1} ch2";

        /// <summary>
        /// Populate the result object for the detail page with the systemid from the list grid so that we know which related records to display.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="user"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);

            if (request.CustomParameters != null) {
                result.ResultObject.SetAttribute("parentlocation_.systemid", request.CustomParameters["parentlocation_.systemid"]);
            }

            return result;
        }

        /// <summary>
        /// There is no way of using an OR for a relation ship on the entity metadata so we add a custom where caluse.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public SearchRequestDto BuildAssettransWhereClause(CompositionPreFilterFunctionParameters parameter) {
            var originalEntity = parameter.OriginalEntity;
            var location = originalEntity.GetAttribute("location");
            var siteid = originalEntity.GetAttribute("siteid");
            var orgid = originalEntity.GetAttribute("orgid");

            var sb = new StringBuilder();
            //base section
            sb.AppendFormat(@"(assettrans.fromloc = '{0}' AND assettrans.siteid = '{1}' AND assettrans.orgid = '{2}')
                            OR (assettrans.toloc = '{0}' AND assettrans.tositeid = '{1}' AND assettrans.toorgid = '{2}')",
                            location, siteid, orgid);
            parameter.BASEDto.SearchValues = null;
            parameter.BASEDto.SearchParams = null;
            parameter.BASEDto.AppendWhereClause(sb.ToString());

            return parameter.BASEDto;
        }

        /// <summary>
        /// The locationspec composition list requires a field that lists the hierarchy of the classifications in a single string delimited with /'s
        /// </summary>
        /// <param name="application"></param>
        /// <param name="request"></param>
        /// <param name="currentData"></param>
        /// <returns></returns>
        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request,
            JObject currentData) {

            var result = await base.GetCompositionData(application, request, currentData);
            if (result.ResultObject.ContainsKey("locationspec_")) {
                var locationspecList = result.ResultObject["locationspec_"].ResultList;
                if (locationspecList.Any()) {
                    var classificationids =
                        locationspecList.Select(value => value["classstructure_.classificationid"].ToString()).ToList();
                    var classhierarchys = GetClassstructureHierarchyPath(classificationids);
                    foreach (var locationspec in locationspecList) {
                        var classificationid = locationspec["classstructure_.classificationid"].ToString();
                        var path = classhierarchys.Single(p => p["id"] == classificationid)["hierarchypath"];
                        locationspec.Add("classstructure_.hierarchypath", path);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Additional where clause so that we only see the related records with the systemid that was selected on the list grid.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public SearchRequestDto FilterLocationHierarchy(CompositionPreFilterFunctionParameters parameter) {
            var systemid = parameter.OriginalEntity.GetAttribute("parentlocation_.systemid");
            var where = "systemid = '{0}'".FormatInvariant(systemid);
            parameter.BASEDto.AppendWhereClause(where);
            return parameter.BASEDto;
        }

        private List<Dictionary<string, string>> GetClassstructureHierarchyPath(List<string> classificationids) {
            var queryString = GetCteQuery(classificationids);
            var result = MaximoHibernateDAO.GetInstance().FindByNativeQuery(queryString);
            return result;
        }
        private string GetCteQuery(List<string> classificationids) {
            var cteQuery = "WITH ";
            var selectQuery = "";
            var queryGroup = 0;
            foreach (var classificationid in classificationids) {
                if (cteQuery != "WITH ") {
                    cteQuery += ",\r\n";
                }
                cteQuery += CteTemplate.FormatInvariant(classificationid, queryGroup);
                if (selectQuery != "") {
                    selectQuery += "\r\nUNION\r\n";
                }
                selectQuery += SelectTemplate.FormatInvariant(classificationid, queryGroup);
                queryGroup++;
            }
            var completeQuery = cteQuery + "\r\n" + selectQuery;
            return completeQuery;
        }

        public override string ApplicationName() {
            return "location";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
