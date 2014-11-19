using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;


namespace softWrench.sW4.Metadata.Applications.DataSet {

    class BaseInvuseDataSet : MaximoApplicationDataSet {
        
        public SearchRequestDto FilterToStoreLoc(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            const string type = "STOREROOM";
            filter.AppendSearchEntry("location.type", type.ToUpper());

            var siteid = parameters.OriginalEntity.GetAttribute("siteid");

            if (siteid != null) {
                filter.AppendSearchEntry("location.siteid", siteid.ToString().ToUpper());
            }

            return filter;
        }

        public SearchRequestDto FilterFromStoreLoc(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            
            if (siteid != null) {
                filter.AppendSearchEntry("inventory.siteid", siteid.ToString().ToUpper());
                filter.SearchSort = "inventory.location asc";
            }
            return filter;
        }

        public SearchRequestDto FilterItem(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            
            if (siteid != null) {
                filter.AppendSearchEntry("inventory_.siteid", siteid.ToString().ToUpper());
                filter.SearchSort = "inventory_.itemnum";
            }
            return filter;
        }

        public SearchRequestDto FilterFromBin(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var location = parameters.OriginalEntity.GetAttribute("fromstoreloc");
            return FilterBin(filter, siteid, location);
        }

        public SearchRequestDto FilterToBin(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var location = parameters.OriginalEntity.GetAttribute("invuseline_.tostoreloc");
            return FilterBin(filter, siteid, location);
        }

        public SearchRequestDto FilterBin(SearchRequestDto filter, object siteid, object location) {
            if (siteid == null || location == null) {
                return filter;
            }
            filter.AppendSearchEntry("inventory_.siteid", siteid.ToString().ToUpper());
            filter.AppendSearchEntry("invbalances.location", location.ToString().ToUpper());
            filter.SearchAscending = true;
            return filter;
        }

        public override string ApplicationName() {
            return "invuse";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}