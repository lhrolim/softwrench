using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using cts.commons.persistence;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {

    public class BaseTicketDataSet : MaximoApplicationDataSet {

        private readonly EntityMetadata _classificationMetadata;

        [Import]
        public TicketStatusHandler StatusHandler {
            get; set;
        }

        [Import]
        public WhereBuilderManager WhereBuilderManager {
            get; set;
        }

        public BaseTicketDataSet() {
            _classificationMetadata = MetadataProvider.Entity("classstructure");
        }


        /*private const string Base = "(( DOCLINKS.ownerid = '{0}' ) AND ( UPPER(COALESCE(DOCLINKS.ownertable,'')) = '{0}' )  ";*/



        public IEnumerable<IAssociationOption> FilterAvailableStatus(AssociationPostFilterFunctionParameters postFilter) {
            return StatusHandler.FilterAvailableStatus(postFilter);
        }

        public virtual SearchRequestDto BuildWorkLogsWhereClause(CompositionPreFilterFunctionParameters parameter) {
            //To allow customization
            return parameter.BASEDto;
        }


        public virtual SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null) {
                return filter;
            }
            filter.AppendSearchEntry("asset.location", location.ToUpper());
            return filter;
        }

        protected virtual IEnumerable<IAssociationOption> GetClassStructureType(
            OptionFieldProviderParameters parameters, string ticketclass, string searchString = null) {

            // TODO: Change the design to use a tree view component
            var query = BuildClassificationQuery(parameters, ticketclass, searchString);

            var result = MaxDAO.FindByNativeQuery(query, null);

            return result.Select(record => {
                var label = string.Format("{0}{1}{2}{3}{4}",
                        record["CLASS_5"] == null ? "" : record["CLASS_5"] + "/",
                        record["CLASS_4"] == null ? "" : record["CLASS_4"] + "/",
                        record["CLASS_3"] == null ? "" : record["CLASS_3"] + "/",
                        record["CLASS_2"] == null ? "" : record["CLASS_2"] + "/",
                        record["CLASS_1"] == null ? "" : record["CLASS_1"] + " (" + record["DESCRIPTION"] + ")");
                var extra = new Dictionary<string, object>();
                extra["classificationid"] = record["CLASS_1"];
                return new MultiValueAssociationOption(record["ID"], label, extra);
            });
        }

        protected virtual IEnumerable<IAssociationOption> GetClassStructureTypeDescription(OptionFieldProviderParameters parameters, string ticketclass, string searchString = null) {

            // TODO: Change the design to use a tree view component
            var query = BuildClassificationQuery(parameters, ticketclass, searchString);

            var result = MaxDAO.FindByNativeQuery(query, null);

            return result.Select(record => {
                var label = record["DESCRIPTION"] ?? string.Format("{0}{1}{2}{3}{4}",
                    record["CLASS_5"] == null ? "" : record["CLASS_5"] + "/",
                    record["CLASS_4"] == null ? "" : record["CLASS_4"] + "/",
                    record["CLASS_3"] == null ? "" : record["CLASS_3"] + "/",
                    record["CLASS_2"] == null ? "" : record["CLASS_2"] + "/",
                    record["CLASS_1"] ?? "");

                return new AssociationOption(record["ID"], label);
            });
        }

        /// <summary>
        /// Using classstructureid by default, unless it requires to sync between more than one Maximo (TGCS and Chicago)
        /// </summary>
        /// <returns></returns>
        protected virtual string ClassificationIdToUse() {
            return "classstructureid";
        }

        protected virtual string BuildClassificationQuery(OptionFieldProviderParameters parameters, string ticketclass, string searchString = null) {

            var classStructureQuery = string.Format(@"SELECT  c.{4} AS ID, c.classificationid as classificationid,
                                                               p3.classificationid AS CLASS_5, 
                                                               p2.classificationid AS CLASS_4,  
                                                               p1.classificationid AS CLASS_3, 
                                                               p.classificationid  AS CLASS_2, 
                                                               c.classificationid  AS CLASS_1,
                                                               c.description AS DESCRIPTION
                                                       FROM classstructure {3} c
                                                       left join classstructure {3} p on p.classstructureid = c.parent
                                                       left join classstructure {3} p1 on p1.classstructureid = p.parent
                                                       left join classstructure {3} p2 on p2.classstructureid = p1.parent
                                                       left join classstructure {3} p3 on p3.classificationid = p2.parent
                                                       WHERE 
                                                               (c.orgid is null or (c.orgid is not null and c.orgid  =  '{0}' )) and
                                                               (c.siteid is null or (c.siteid is not null and c.siteid  =  '{1}' )) and
                                                               c.classstructureid in (select classusewith.classstructureid 
                                                                                        from classusewith  
                                                                                        where classusewith.classstructureid=c.classstructureid
                                                                                        and objectname= '{2}') and {5}",
                                    parameters.OriginalEntity.GetAttribute("orgid"),
                                    parameters.OriginalEntity.GetAttribute("siteid"),
                                    ReferenceEquals(parameters.OriginalEntity.GetAttribute("class"), "") ? "" : ticketclass,
                                    ApplicationConfiguration.IsOracle(DBType.Maximo) ? "" : "as",
                                    ClassificationIdToUse(),
                                    WhereBuilderManager.BuildWhereClause(_classificationMetadata, new InternalQueryRequest() {},true)
                                    );
            if (searchString != null) {
                classStructureQuery += string.Format(@" and ( UPPER(COALESCE(p3.classificationid,'')) like '%{0}%' or UPPER(COALESCE(p3.description,'')) like '%{0}%' or
                                                              UPPER(COALESCE(p2.classificationid,'')) like '%{0}%' or UPPER(COALESCE(p2.description,'')) like '%{0}%' or
                                                              UPPER(COALESCE(p1.classificationid,'')) like '%{0}%' or UPPER(COALESCE(p1.description,'')) like '%{0}%' or
                                                              UPPER(COALESCE(p.classificationid,''))  like '%{0}%' or UPPER(COALESCE(p.description,''))  like '%{0}%' or
                                                              UPPER(COALESCE(c.classificationid,''))  like '%{0}%' or UPPER(COALESCE(c.description,''))  like '%{0}%' )",
                                                            searchString.ToUpper());
            }
            return classStructureQuery;
        }


    }
}
