using System.Threading.Tasks;
using cts.commons.portable.Util;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.dataset {
    public class ToshibaWorklogDataSet : MaximoApplicationDataSet {

        private readonly RestEntityRepository _restRepository;

        public ToshibaWorklogDataSet(RestEntityRepository restRepository) {
            _restRepository = restRepository;
        }

        protected override async Task<DataMap> FetchDetailDataMap(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            if (request.CustomParameters == null || !request.CustomParameters.ContainsKey(MaximoRestUtils.RestMarkerFieldName)) {
                return await base.FetchDetailDataMap(application, user, request);
            }
            var isRestWorklog = "true".EqualsIc(request.CustomParameters[MaximoRestUtils.RestMarkerFieldName] as string);
            if (!isRestWorklog) {
                return await base.FetchDetailDataMap(application, user, request);
            }

            var entity = MetadataProvider.Entity(application.Entity);
            var dataMap = await _restRepository.Get(entity, request.Id);
            if (dataMap == null) {
                return null;
            }

            // TODO: find out why serializer is acting weird on this field creating:
            // 'description_longdescription' containing only line-breaks
            // 'descriptioN_LONGDESCRIPTION' containing the actual content
            var longdescription = dataMap["DESCRIPTION_LONGDESCRIPTION"];
            dataMap.Remove("DESCRIPTION_LONGDESCRIPTION");
            dataMap["description_longdescription"] = longdescription;

            return dataMap;
        }

        public override string ApplicationName() {
            return "worklog";
        }

        public override string ClientFilter() {
            return "tgcs";
        }
    }
}