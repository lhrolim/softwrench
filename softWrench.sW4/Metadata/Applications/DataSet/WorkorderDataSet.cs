using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Search;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;
using System;

namespace softWrench.sW4.Metadata.Applications.DataSet {

    class WorkorderDataSet : MaximoApplicationDataSet {


        //WAPPR -> Pode mudar para todos os outros. Sem restrições de edição na info da WO.
        //APPR -> Pode mudar para todos os outros. Sem restrições de edição na info da WO (por enquanto).
        //COMP -> Pode apenas mudar para CLOSE. Sem restrições de edição na info da WO.
        //CLOSE-> Não dá para editar mais nenhuma info da WO.
        //CAN -> Não dá para editar mais nenhuma info da WO.
        public IEnumerable<IAssociationOption> FilterAvailableStatus(DataMap currentWorkorder, IEnumerable<AssociationOption> loadedAssociations) {
            var currentStatus = (string)currentWorkorder.GetAttribute("status");
            var filterAvailableStatus = loadedAssociations as AssociationOption[] ?? loadedAssociations.ToArray();
            if (currentStatus == null) {
                return new List<AssociationOption> { filterAvailableStatus.First(l => l.Value == "OPEN") };
            }
            var currentOption = filterAvailableStatus.FirstOrDefault(l => l.Value == currentStatus);
            if (currentOption == null) {
                return filterAvailableStatus;
            }

            if (currentStatus == "APPR" || currentStatus == "WAPPR") {
                return filterAvailableStatus;
            }
            if (currentStatus == "COMP") {
                return new List<AssociationOption> { currentOption, filterAvailableStatus.First(l => l.Value == "CLOSE") };
            }
            return new List<AssociationOption> { currentOption };
        }
        /* Need to add this prefilter function for the problem codes !! */
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters) {
            return ProblemCodeFilterByFailureClassFunction(parameters);
        }

        private SearchRequestDto ProblemCodeFilterByFailureClassFunction(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var failurecodeid = parameters.OriginalEntity.GetAttribute("failurelist_.failurelist");
            if (failurecodeid == null) {
                return filter;
            }
            filter.AppendSearchEntry("parent", failurecodeid.ToString());
            return filter;
        }
        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            return AssetFilterBySiteFunction(parameters);
        }

        public SearchRequestDto AssetFilterBySiteFunction(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null) {
                return filter;
            }
            filter.AppendSearchEntry("asset.location", location.ToUpper());
            return filter;
        }

        public IEnumerable<IAssociationOption> GetWOPriorityType(OptionFieldProviderParameters parameters)
        {
            var query = @"SELECT [description] AS LABEL,
	                             CAST(value AS INT) AS VALUE 
                          FROM numericdomain
                          WHERE domainid = 'WO PRIORITY'";

            var result = MaxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            if (result != null && result.Any())
            {
                foreach (var record in result)
                    list.Add(new AssociationOption(record["VALUE"].ToString(), string.Format("{0} - {1}", record["VALUE"], record["LABEL"])));
            }
            else
            {
                // If no values are found, then default to numeric selection 1-5
                list.Add(new AssociationOption("1", "1"));
                list.Add(new AssociationOption("2", "2"));
                list.Add(new AssociationOption("3", "3"));
                list.Add(new AssociationOption("4", "4"));
                list.Add(new AssociationOption("5", "5"));
            }

            return list;
        }

        public IEnumerable<IAssociationOption> GetWOClassStructureType(OptionFieldProviderParameters parameters)
        {

            // TODO: Change the design to use a tree view component
            var query = string.Format(@"SELECT c.classstructureid AS ID,
                                               p3.classificationid AS CLASS_5,
                                               p2.classificationid AS CLASS_4,
                                               p1.classificationid AS CLASS_3,
                                               p.classificationid AS CLASS_2,
                                               c.classificationid AS CLASS_1
                                        from classstructure as c
                                        left join classstructure as p on p.classstructureid = c.parent
                                        left join classstructure as p1 on p1.classstructureid = p.parent
                                        left join classstructure as p2 on p2.classstructureid = p1.parent
                                        left join classstructure as p3 on p3.classificationid = p2.parent
                                        where
                                        c.haschildren = 0
                                        and (c.orgid is null or (c.orgid is not null and c.orgid = '{0}' ))
                                        and (c.siteid is null or (c.siteid is not null and c.siteid = '{1}' ))
                                        and c.classstructureid in (select classusewith.classstructureid
                                        from classusewith
                                        where classusewith.classstructureid=c.classstructureid
                                        and objectname= '{2}')",
                                        parameters.OriginalEntity.Attributes["orgid"],
                                        parameters.OriginalEntity.Attributes["siteid"],
                                        parameters.OriginalEntity.Attributes["woclass"] == "" ? parameters.OriginalEntity.Attributes["woclass"] : "WORKORDER");

            var result = MaxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            foreach (var record in result)
            {
                list.Add(new AssociationOption(record["ID"],
                String.Format("{0}{1}{2}{3}{4}",
                            record["CLASS_5"] == null ? "" : record["CLASS_5"] + "/",
                            record["CLASS_4"] == null ? "" : record["CLASS_4"] + "/",
                            record["CLASS_3"] == null ? "" : record["CLASS_3"] + "/",
                            record["CLASS_2"] == null ? "" : record["CLASS_2"] + "/",
                            record["CLASS_1"] == null ? "" : record["CLASS_1"]
                            )));
            }

            return list;
        }

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
