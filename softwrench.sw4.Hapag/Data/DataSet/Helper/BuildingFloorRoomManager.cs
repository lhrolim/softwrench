using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.simpleinjector;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    class BuildingFloorRoomManager {

        public delegate IAssociationOption LocationFieldDelegate(IDictionary<string, object> values);

        public static async Task<HashSet<IAssociationOption>> DoGetLocation(string location, SearchRequestDto dto, LocationFieldDelegate delegateToUse) {
            var locationApp = MetadataProvider.Application("location");
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(locationApp.ApplyPolicies(new ApplicationMetadataSchemaKey("list"), SecurityFacade.CurrentUser(), ClientPlatform.Web));
            
//            dto.PageSize = 10000;
            dto.AppendSearchEntry("pluspcustomer", "%" + location);
            dto.AppendSearchEntry("status","!=DECOMMISSIONED");
            dto.AppendSearchEntry("TYPE","OPERATING");
            dto.IgnoreWhereClause = true;
            var entityRepository = SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
            var result = await entityRepository.GetAsRawDictionary(entityMetadata, dto);
            var options = new HashSet<IAssociationOption>();
            foreach (var attributeHolder in result.ResultList) {
                options.Add(delegateToUse.Invoke(attributeHolder));
            }
            return options;
        }


       

    }
}
