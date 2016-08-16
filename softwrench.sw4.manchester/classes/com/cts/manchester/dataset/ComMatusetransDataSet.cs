using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softwrench.sw4.manchester.classes.com.cts.manchester.dataset {
    class ComMatusetransDataSet : MaximoApplicationDataSet {
        private IEnumerable<IAssociationOption> filterMaterials(AssociationPostFilterFunctionParameters postParams) {
            return (from item in postParams.Options where item.Label != null && item.Value.Equals(postParams.OriginalEntity["itemnum"]) select new AssociationOption(item.Label, item.Label)).Cast<IAssociationOption>().ToList();
        }

        public SearchRequestDto filterPlannedMaterials(AssociationPreFilterFunctionParameters parameters) {
            var orgid = parameters.OriginalEntity.GetAttribute("orgid");
            var wonum = parameters.OriginalEntity.GetAttribute("refwo");
            var filter = parameters.BASEDto;
            var query = string.Format(@"select itemnum
                                        from wpmaterial
                                        where wonum = '{0}' and orgid = '{1}'",
            wonum, orgid);
            var result = MaxDAO.FindByNativeQuery(query, null);
            if (result.Any()) {
                filter.AppendWhereClauseFormat("( itemnum in ({0}) )", query);
            }

            filter.AppendWhereClause(String.Format("(ITEMNUM IN (SELECT ITEMNUM FROM invbalances WHERE siteid =  '{0}') )", parameters.OriginalEntity.GetAttribute("siteid")));

            return filter;
        }

        public SearchRequestDto filterSpareParts(AssociationPreFilterFunctionParameters preParams) {
            var filter = preParams.BASEDto;
            filter.AppendWhereClause(String.Format("(sparepart.ITEMNUM IN (SELECT ITEMNUM FROM invbalances WHERE siteid =  '{0}') )", preParams.OriginalEntity.GetAttribute("siteid")));

            return filter;
        }

        public IEnumerable<IAssociationOption> filterStoreLoc(AssociationPostFilterFunctionParameters postParams) {
            return filterMaterials(postParams).Distinct(new ValueComparer());
        }

        public IEnumerable<IAssociationOption> filterLotnum(AssociationPostFilterFunctionParameters postParams) {
            return filterMaterials(postParams).Distinct(new ValueComparer());
        }

        public IEnumerable<IAssociationOption> filterBinnum(AssociationPostFilterFunctionParameters postParams) {
            return filterMaterials(postParams).Distinct(new ValueComparer());
        }

        public IEnumerable<IAssociationOption> filterCond(AssociationPostFilterFunctionParameters postParams) {
            return filterMaterials(postParams).Distinct(new ValueComparer());
        }

        public override string ApplicationName() {
            return "matusetrans";
        }

        public override string ClientFilter() {
            return "manchester";
        }
    }
}
