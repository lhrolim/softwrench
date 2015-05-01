using System.Linq;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Offline;

namespace softWrench.sW4.Web.Controllers.Mobile {

    /// <summary>
    /// <para>This controller is a front facade for handling all operations that comes from a mobile device.</para>
    /// <para>The mobile devices nature is to stay disconnected most of the time, lots of operations should be performed in batch, in an eager fashion, 
    /// in opposition to the lazy-loading style of a web platform, in which the data is only fetched when needed.</para>
    /// <para>This is the main reason why this controller is being dettached from the others, since it will carry some specific logic upon metadata,
    ///  delegating to the inner tiers of the application</para>
    /// 
    /// </summary>
    //    [Authorize]
    public class MobileController : ApiController {

        private readonly DataSetProvider _dataSetProvider = DataSetProvider.GetInstance();

        private readonly SynchronizationManager _syncManager;

        readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public MobileController(SynchronizationManager syncManager) {
            this._syncManager = syncManager;
        }

        /// <summary>
        /// The main purpose here is to retrieve all the metadata information 
        /// needed for the mobile application to this current user in a single step.
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MobileMetadataDownloadResponseDefinition DownloadMetadatas() {
            var user = SecurityFacade.CurrentUser();
            var metadatas = MetadataProvider.Applications(ClientPlatform.Mobile);
            var securedMetadatas = metadatas.Select(metadata => metadata.CloneSecuring(user)).ToList();
            bool fromCache;
            var securedMenu = user.Menu(ClientPlatform.Mobile, out fromCache);

            var response = new MobileMetadataDownloadResponseDefinition {
                MetadatasJSON = JsonConvert.SerializeObject(securedMetadatas, Newtonsoft.Json.Formatting.None, _jsonSerializerSettings),
                MenuJson = JsonConvert.SerializeObject(securedMenu, Newtonsoft.Json.Formatting.None, _jsonSerializerSettings)
            };
            return response;
        }

        [HttpPost]
        public SynchronizationResultDto PullNewData([FromUri]SynchronizationRequestDto synchronizationRequest, JObject rowstampMap) {
            return _syncManager.GetData(synchronizationRequest, SecurityFacade.CurrentUser(), rowstampMap);
        }


    }
}