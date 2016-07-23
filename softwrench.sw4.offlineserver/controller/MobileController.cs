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
using softwrench.sw4.batch.api.entities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softwrench.sw4.offlineserver.dto;
using softwrench.sw4.offlineserver.dto.association;
using softwrench.sw4.offlineserver.services;
using softwrench.sw4.offlineserver.services.util;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Offline;
using softWrench.sW4.Metadata.Menu;
using softWrench.sW4.Security.Context;

namespace softwrench.sw4.offlineserver.controller {

    /// <summary>
    /// <para>This controller is a front facade for handling all operations that comes from a mobile device.</para>
    /// <para>The mobile devices nature is to stay disconnected most of the time, lots of operations should be performed in batch, in an eager fashion, 
    /// in opposition to the lazy-loading style of a web platform, in which the data is only fetched when needed.</para>
    /// <para>This is the main reason why this controller is being dettached from the others, since it will carry some specific logic upon metadata,
    ///  delegating to the inner tiers of the application</para>
    /// 
    /// </summary>
    [Authorize]
    public class MobileController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MobileController));

        private IContextLookuper _contextLookuper;

        private readonly SynchronizationManager _syncManager;

        private readonly AppConfigurationProvider _appConfigurationProvider;

        private readonly OffLineBatchService _offLineBatchService;

        private readonly MenuSecurityManager _menuManager;

        readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public MobileController(SynchronizationManager syncManager, AppConfigurationProvider appConfigurationProvider,
            OffLineBatchService offLineBatchService, MenuSecurityManager menuManager, IContextLookuper contextLookuper) {
            _syncManager = syncManager;
            _appConfigurationProvider = appConfigurationProvider;
            _offLineBatchService = offLineBatchService;
            _menuManager = menuManager;
            _contextLookuper = contextLookuper;
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

            var associationApps = OffLineMetadataProvider.FetchAssociationApps(user, false);
            var compositonApps = OffLineMetadataProvider.FetchCompositionApps(user);
            var commandBars = user.SecuredBars(ClientPlatform.Mobile, MetadataProvider.CommandBars(platform: ClientPlatform.Mobile, includeNulls: false));

            bool fromCache;
            var securedMenu = _menuManager.Menu(user, ClientPlatform.Mobile, out fromCache);

            var response = new MobileMetadataDownloadResponseDefinition {
                TopLevelMetadatasJson = JsonConvert.SerializeObject(securedMetadatas, Formatting.None, _jsonSerializerSettings),
                AssociationMetadatasJson = JsonConvert.SerializeObject(associationApps, Formatting.None, _jsonSerializerSettings),
                CompositionMetadatasJson = JsonConvert.SerializeObject(compositonApps, Formatting.None, _jsonSerializerSettings),
                MenuJson = JsonConvert.SerializeObject(securedMenu, Formatting.None, _jsonSerializerSettings),
                CommandBarsJson = JsonConvert.SerializeObject(commandBars, Formatting.None, _jsonSerializerSettings),
                AppConfiguration = _appConfigurationProvider.AppConfig(),
                UserProperties = user.GenericSyncProperties
            };

            Log.InfoFormat("Download Metadata executed in {0}", LoggingUtil.MsDelta(watch));
            return response;
        }

        [HttpGet]
        public string Counts() {

            var req = new SynchronizationRequestDto() {
                ReturnNewApps = true
            };

            _contextLookuper.LookupContext().OfflineMode = true;

            var watch = Stopwatch.StartNew();
            var appData = _syncManager.GetData(req, SecurityFacade.CurrentUser(), null);
            watch.Stop();
            var appEllapsed = watch.ElapsedMilliseconds;

            watch.Restart();
            var associationResult = _syncManager.GetAssociationData(SecurityFacade.CurrentUser(), null);
            watch.Stop();
            var associationEllapsed = watch.ElapsedMilliseconds;

            var topCountData = appData.TopApplicationData.ToDictionary(applicationData => applicationData.ApplicationName, applicationData => applicationData.NewCount);
            var associationCounts = associationResult.AssociationData.ToDictionary(applicationData => applicationData.Key, applicationData => applicationData.Value.Count);
            var compositionCounts = appData.CompositionData.ToDictionary(applicationData => applicationData.ApplicationName, applicationData => applicationData.NewCount);

            var report = new MobileCountReport() {
                TopAppCounts = topCountData,
                AssociationCounts = associationCounts,
                CompositionCounts = compositionCounts,
                AppTimeEllapsed = appEllapsed,
                AssociationTimeEllapsed = associationEllapsed
            };

            return JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented,
            new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
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

        public class MobileCountReport {
            public IDictionary<string, int> TopAppCounts = new Dictionary<string, int>();
            public IDictionary<string, int> AssociationCounts = new Dictionary<string, int>();
            public IDictionary<string, int> CompositionCounts = new Dictionary<string, int>();
            public long AssociationTimeEllapsed;
            public long AppTimeEllapsed;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="application">Name of the application of this batch</param>
        /// <param name="remoteId">Id of the batch on the remote end</param>
        /// <param name="batchContent">The </param>
        /// <returns></returns>
        [HttpPost]
        public Batch SubmitBatch([FromUri]string application, [FromUri]string remoteId, JObject batchContent) {
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