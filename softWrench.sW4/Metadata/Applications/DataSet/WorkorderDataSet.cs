using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Search;

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
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters)
        {
            return ProblemCodeFilterByFailureClassFunction(parameters);
        }

        private SearchRequestDto ProblemCodeFilterByFailureClassFunction(AssociationPreFilterFunctionParameters parameters)
        {
            var filter = parameters.BASEDto;
            var failurecodeid = parameters.OriginalEntity.GetAttribute("failurelist_.failurelist");
            if (failurecodeid == null)
            {
                return filter;
            }
            filter.AppendSearchEntry("parent", failurecodeid.ToString());
            return filter;
        }
        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters)
        {
            return AssetFilterBySiteFunction(parameters);
        }


        public SearchRequestDto AssetFilterBySiteFunction(AssociationPreFilterFunctionParameters parameters)
        {
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null)
            {
                return filter;
            }
            filter.AppendSearchEntry("asset.location", location.ToUpper());
            return filter;
        }

        public string ApplicationName() {
            return "workorder";
        }

        public string ClientFilter() {
            return null;
        }
    }
}
