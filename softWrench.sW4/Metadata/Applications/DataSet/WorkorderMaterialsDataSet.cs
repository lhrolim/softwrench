using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sw4.Shared2.Data.Association;

using softWrench.sW4.Data.Search;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;
using System;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Metadata.Applications.DataSet {

    class WorkorderMaterialsDataSet : MaximoApplicationDataSet {

        private IEnumerable<IAssociationOption> filterMaterials(AssociationPostFilterFunctionParameters postParams) {
            List<IAssociationOption> Collections = new List<IAssociationOption>();
            foreach (var item in postParams.Options) {
                if (item.Label != null && item.Value.Equals(postParams.OriginalEntity.Attributes["itemnum"])) {
                    Collections.Add(new AssociationOption(item.Label, item.Label));
                }
            }

            return Collections;
        }

        public SearchRequestDto filterItems(AssociationPreFilterFunctionParameters preParams) {
            var filter = preParams.BASEDto;
            filter.AppendWhereClause(String.Format("(ITEMNUM IN (SELECT ITEMNUM FROM invbalances WHERE siteid =  '{0}') )", preParams.OriginalEntity.GetAttribute("siteid")));

            return filter;
        }

        public IEnumerable<IAssociationOption> filterStoreLoc(AssociationPostFilterFunctionParameters postParams)
        {
            return filterMaterials(postParams);
        }

        public IEnumerable<IAssociationOption> filterLotnum(AssociationPostFilterFunctionParameters postParams) {
            return filterMaterials(postParams);
        }

        public IEnumerable<IAssociationOption> filterBinnum(AssociationPostFilterFunctionParameters postParams) {
            return filterMaterials(postParams);
        }

        public override string ApplicationName() {
            return "matusetrans";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
