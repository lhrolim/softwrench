using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using cts.commons.Util;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softwrench.sw4.offlineserver.dto;
using softwrench.sw4.offlineserver.dto.association;
using softwrench.sw4.offlineserver.services;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Offline;
using softWrench.sW4.Util;

namespace softwrench.sw4.offlineserver.controller {

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

        private static readonly ILog Log = LogManager.GetLogger(typeof(MobileController));

        private readonly DataSetProvider _dataSetProvider = DataSetProvider.GetInstance();

        private readonly SynchronizationManager _syncManager;

        private readonly StatusColorResolver _statusColorResolver;

        private readonly OffLineBatchService _offLineBatchService;

        readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public MobileController(SynchronizationManager syncManager, StatusColorResolver statusColorResolver, OffLineBatchService offLineBatchService) {
            _syncManager = syncManager;
            _statusColorResolver = statusColorResolver;
            _offLineBatchService = offLineBatchService;
        }

        /// <summary>
        /// The main purpose here is to retrieve all the metadata information 
        /// needed for the mobile application to this current user in a single step.
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MobileMetadataDownloadResponseDefinition DownloadMetadatas() {
            var watch = Stopwatch.StartNew();
            var user = SecurityFacade.CurrentUser();
            var topLevel = MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile, user);
            //apply any user role constraints that would avoid bringing unwanted fields for that specific user.
            var securedMetadatas = topLevel.Select(metadata => metadata.CloneSecuring(user)).ToList();

            var associationApps = OffLineMetadataProvider.FetchAssociationApps(user);
            var compositonApps = OffLineMetadataProvider.FetchCompositionApps(user);


            bool fromCache;
            var securedMenu = user.Menu(ClientPlatform.Mobile, out fromCache);

            var statusColorJSONString = "";

            var statusColorJson = _statusColorResolver.FetchCatalogs();
            if (statusColorJson != null) {
                statusColorJSONString = statusColorJson.ToString(Newtonsoft.Json.Formatting.Indented);
            }

            var response = new MobileMetadataDownloadResponseDefinition {
                TopLevelMetadatasJson = JsonConvert.SerializeObject(securedMetadatas, Newtonsoft.Json.Formatting.None, _jsonSerializerSettings),
                AssociationMetadatasJson = JsonConvert.SerializeObject(associationApps, Newtonsoft.Json.Formatting.None, _jsonSerializerSettings),
                CompositionMetadatasJson = JsonConvert.SerializeObject(compositonApps, Newtonsoft.Json.Formatting.None, _jsonSerializerSettings),
                MenuJson = JsonConvert.SerializeObject(securedMenu, Newtonsoft.Json.Formatting.None, _jsonSerializerSettings),
                StatusColorsJSON = statusColorJSONString
            };

            Log.InfoFormat("Download Metadata executed in {0}", LoggingUtil.MsDelta(watch));
            return response;
        }

        [HttpPost]
        public SynchronizationResultDto PullNewData([FromUri]SynchronizationRequestDto synchronizationRequest, JObject rowstampMap) {
            return _syncManager.GetData(synchronizationRequest, SecurityFacade.CurrentUser(), rowstampMap);
        }

        [HttpPost]
        public AssociationSynchronizationResultDto PullAssociationData(JObject rowstampMap) {
            return _syncManager.GetAssociationData(SecurityFacade.CurrentUser(), rowstampMap);
        }


        [HttpPost]
        public AssociationSynchronizationResultDto PullSingleAssociationData([FromUri]String applicationToFetch, JObject rowstampMap) {
            return _syncManager.GetAssociationData(SecurityFacade.CurrentUser(), rowstampMap, applicationToFetch);
        }

        [HttpPost]
        public Batch SubmitBatch([FromUri]String application, [FromUri]String remoteId, JObject batchContent) {
            Log.InfoFormat("Creating batch for application {0}", application);
            // return the generated Batch to be serialized
            var batch = _offLineBatchService.SubmitBatch(application, remoteId, batchContent);
            return batch;
        }

        [HttpGet]
        public IList<Batch> BatchStatus([FromUri]IList<String> ids) {
            return _offLineBatchService.GetBatchesByRemoteIds(ids);
        } 
     
    }
}