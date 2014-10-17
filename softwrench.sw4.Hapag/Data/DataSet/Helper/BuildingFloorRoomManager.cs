using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using System.Collections.Generic;
using softWrench.sW4.SimpleInjector;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    class BuildingFloorRoomManager {

        public delegate IAssociationOption LocationFieldDelegate(IDictionary<string, object> values);

        private static EntityRepository _repository;

        private static EntityRepository EntityRepository {
            get {
                if (_repository == null) {
                    _repository =
                        SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
                }
                return _repository;
            }
        }

        public static IEnumerable<IAssociationOption> DoGetLocation(string location, SearchRequestDto dto, LocationFieldDelegate delegateToUse) {
            var locationApp = MetadataProvider.Application("location");
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(locationApp.ApplyPolicies(new ApplicationMetadataSchemaKey("list"), SecurityFacade.CurrentUser(), ClientPlatform.Web));
            
//            dto.PageSize = 10000;
            dto.AppendSearchEntry("pluspcustomer", "%" + location);
            dto.AppendSearchEntry("status","!=DECOMMISSIONED");
            dto.AppendSearchEntry("TYPE","OPERATING");
            dto.IgnoreWhereClause = true;
            var result = EntityRepository.GetAsRawDictionary(entityMetadata, dto);
            var options = new HashSet<IAssociationOption>();
            foreach (var attributeHolder in result) {
                options.Add(delegateToUse.Invoke(attributeHolder));
            }
            return options;
        }


       

    }
}
