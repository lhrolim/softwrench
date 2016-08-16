using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Workorder {

    class WorkorderMaterialsDataSet : MaximoApplicationDataSet {

        private IEnumerable<IAssociationOption> filterMaterials(AssociationPostFilterFunctionParameters postParams) {
            return (from item in postParams.Options where item.Label != null && item.Value.Equals(postParams.OriginalEntity["itemnum"]) select new AssociationOption(item.Label, item.Label)).Cast<IAssociationOption>().ToList();
        }

        public SearchRequestDto filterItems(AssociationPreFilterFunctionParameters preParams) {
            var filter = preParams.BASEDto;
            filter.AppendWhereClause(String.Format("(ITEMNUM IN (SELECT ITEMNUM FROM invbalances WHERE siteid =  '{0}') )", preParams.OriginalEntity.GetAttribute("siteid")));

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
            return null;
        }
    }

    //class ValueComparer : IEqualityComparer<IAssociationOption> {

    //    public bool Equals(IAssociationOption x, IAssociationOption y) {

    //        //Check whether the compared objects reference the same data. 
    //        if (Object.ReferenceEquals(x, y)) return true;

    //        //Check whether any of the compared objects is null. 
    //        if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
    //            return false;

    //        return x.Value == y.Value;
    //    }

    //    public int GetHashCode(IAssociationOption item) {
    //        //Check whether the object is null 
    //        if (Object.ReferenceEquals(item, null)) return 0;

    //        //Get hash code for the Name field if it is not null. 
    //        int hashProductName = item.Value == null ? 0 : item.Value.GetHashCode();

    //        //Get hash code for the Code field. 
    //        int hashProductCode = item.Value.GetHashCode();

    //        //Calculate the hash code for the product. 
    //        return hashProductName ^ hashProductCode;
    //    }
    //}
}
