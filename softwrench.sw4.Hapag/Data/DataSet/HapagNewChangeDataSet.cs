using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Security;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagNewChangeDataSet : HapagBaseApplicationDataSet {

        #region PrefilterFunctions

        public HapagNewChangeDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, IMaximoHibernateDAO maxDao)
            : base(locationManager, entityRepository, maxDao) {
        }

        public async Task<IEnumerable<IAssociationOption>> GetInfrastructureAssetToChange(OptionFieldProviderParameters parameters) {
            var entityMetadata = MetadataProvider.Entity("ci");
            var attributeHolders = await EntityRepository.Get(entityMetadata, new SearchRequestDto());
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
