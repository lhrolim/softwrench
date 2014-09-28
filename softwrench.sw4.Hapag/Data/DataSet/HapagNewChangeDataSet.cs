using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagNewChangeDataSet : HapagBaseApplicationDataSet {

        #region PrefilterFunctions

        public IEnumerable<IAssociationOption> GetInfrastructureAssetToChange(OptionFieldProviderParameters parameters) {
            var entityMetadata = MetadataProvider.Entity("ci");
            var result = new EntityRepository().Get(entityMetadata, new SearchRequestDto());
            var attributeHolders = result as AttributeHolder[] ?? result.ToArray();
            var options = new HashSet<IAssociationOption>();
            foreach (var attributeHolder in attributeHolders) {
                var ciname = attributeHolder.GetAttribute("ciname");
                if (!DataSetUtil.IsValid(ciname, typeof(String))) {
                    continue;
                }
                options.Add(new AssociationOption((string)attributeHolder.GetAttribute("cinum"), (string)ciname));
            }

            return options;
        }

        #endregion

        public override string ApplicationName() {
            return "newchange";
        }
    }
}
