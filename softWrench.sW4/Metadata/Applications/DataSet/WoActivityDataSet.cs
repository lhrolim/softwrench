using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Context;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Search;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;
using System;

namespace softWrench.sW4.Metadata.Applications.DataSet {

    class WorActivityDataSet : MaximoApplicationDataSet {

        private IContextLookuper _contextLookuper;

        public WorActivityDataSet(IContextLookuper contextLookuper) {
            _contextLookuper = contextLookuper;
        }


        //WAPPR -> Pode mudar para todos os outros. Sem restrições de edição na info da WO.
        //APPR -> Pode mudar para todos os outros. Sem restrições de edição na info da WO (por enquanto).
        //COMP -> Pode apenas mudar para CLOSE. Sem restrições de edição na info da WO.
        //CLOSE-> Não dá para editar mais nenhuma info da WO.
        //CAN -> Não dá para editar mais nenhuma info da WO.
        public IEnumerable<IAssociationOption> FilterAvailableStatus(AssociationPostFilterFunctionParameters postFilter) {
            var metadataParameters = _contextLookuper.LookupContext().MetadataParameters;
            string currentStatus = null;

            if (postFilter.OriginalEntity.ContainsAttribute("status")) {
                currentStatus = postFilter.OriginalEntity.GetAttribute("status").ToString();
            } else if (metadataParameters.ContainsKey("currentstatus")) {
                currentStatus = metadataParameters["currentstatus"].ToString();
            }
            var filterAvailableStatus = postFilter.Options;
            if (currentStatus == null) {
                return new List<IAssociationOption> { filterAvailableStatus.First(l => l.Value == "OPEN") };
            }
            var currentOption = filterAvailableStatus.FirstOrDefault(l => l.Value == currentStatus);
            if (currentOption == null) {
                return filterAvailableStatus;
            }

            if (currentStatus == "APPR" || currentStatus == "WAPPR") {
                return filterAvailableStatus;
            }
            if (currentStatus == "COMP") {
                return new List<IAssociationOption> { currentOption, filterAvailableStatus.First(l => l.Value == "CLOSE") };
            }
            return new List<IAssociationOption> { currentOption };
        }





        public override string ApplicationName() {
            return "woactivity";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
