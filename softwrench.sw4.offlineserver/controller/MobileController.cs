using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.Util;
using cts.commons.web.Attributes;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NHibernate.Util;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.offlineserver.audit;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softwrench.sw4.offlineserver.model;
using softwrench.sw4.offlineserver.model.dto;
using softwrench.sw4.offlineserver.model.dto.association;
using softwrench.sw4.offlineserver.services;
using softwrench.sw4.offlineserver.services.util;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softwrench.sW4.Shared2.Metadata.Offline;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Dynamic.Model;
using softWrench.sW4.Dynamic.Services;
using softWrench.sW4.Metadata.Menu;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SPF;

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
    [SWControllerConfiguration]
    public class MobileController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MobileController));

        private const string MenuBuilderKey = "offlinetititlebuilder";

        private readonly IContextLookuper _contextLookuper;

        private readonly SynchronizationManager _syncManager;

        private readonly AppConfigurationProvider _appConfigurationProvider;

        private readonly OffLineBatchService _offLineBatchService;

        private readonly MenuSecurityManager _menuManager;

        private readonly JavascriptDynamicService _jsService;


        private readonly OfflineAuditManager _offlineAuditManager;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public MobileController(SynchronizationManager syncManager, AppConfigurationProvider appConfigurationProvider,
            OffLineBatchService offLineBatchService, MenuSecurityManager menuManager, IContextLookuper contextLookuper, JavascriptDynamicService jsService,
            OfflineAuditManager offlineAuditManager) {
            _syncManager = syncManager;
            _appConfigurationProvider = appConfigurationProvider;
            _offLineBatchService = offLineBatchService;
            _menuManager = menuManager;
            _contextLookuper = contextLookuper;
            _jsService = jsService;
            _offlineAuditManager = offlineAuditManager;
        }


        private static string BuildOfflineMenuTitle(IDictionary<string, object> parameters) {
            if (parameters == null || !parameters.ContainsKey(MenuBuilderKey)) {
                return null;
            }
            var builderString = parameters[MenuBuilderKey] as string;
            return string.IsNullOrEmpty(builderString) ? null : GenericSwMethodInvoker.Invoke<string>(null, builderString, null);
        }

        private static void BuildOfflineMenuTitle(MenuBaseDefinition leaf) {
            var container = leaf as MenuContainerDefinition;
            if (container != null) {
                container.Title = BuildOfflineMenuTitle(container.Parameters) ?? container.Title;
                container.Leafs.ToList().ForEach(BuildOfflineMenuTitle);
            }
            var application = leaf as ApplicationMenuItemDefinition;
            if (application != null) {
                application.Title = BuildOfflineMenuTitle(application.Parameters) ?? application.Title;
            }
        }

        private static MenuDefinition BuildOfflineMenuTitles(MenuDefinition baseMenu) {
            var builtLeafs = new List<MenuBaseDefinition>();
            if (baseMenu.Leafs == null) {
                return new MenuDefinition(builtLeafs, baseMenu.MainMenuDisplacement.ToString(), baseMenu.ItemindexId);
            }
            builtLeafs.AddRange(baseMenu.Leafs.Select(leaf => {
                BuildOfflineMenuTitle(leaf);
                return leaf;
            }));
            return new MenuDefinition(builtLeafs, baseMenu.MainMenuDisplacement.ToString(), baseMenu.ItemindexId);
        }



        /// <summary>
        /// The main purpose here is to retrieve all the metadata information 
        /// needed for the mobile application to this current user in a single step.
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MobileMetadataDownloadResponseDefinition> DownloadMetadatas(MetadataDownloadDto metadataDto) {
            var watch = Stopwatch.StartNew();
            
            await _offlineAuditManager.MarkSyncOperationBegin(metadataDto.ClientOperationId, metadataDto.DeviceData, OfflineAuditManager.OfflineAuditMode.Metadata);
            var user = SecurityFacade.CurrentUser();
            var topLevel = MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile, user);
            //apply any user role constraints that would avoid bringing unwanted fields for that specific user.
            var securedMetadatas = topLevel.Select(metadata => metadata.CloneSecuring(user)).ToList();

            var associationApps = OffLineMetadataProvider.FetchAssociationApps(user, false);
            var compositonApps = OffLineMetadataProvider.FetchCompositionApps(user);
            var commandBars = user.SecuredBars(ClientPlatform.Mobile, MetadataProvider.CommandBars(platform: ClientPlatform.Mobile, includeNulls: false));

            bool fromCache;
            var securedMenu = BuildOfflineMenuTitles(_menuManager.Menu(user, ClientPlatform.Mobile, out fromCache));

            var response = new MobileMetadataDownloadResponseDefinition {
                TopLevelMetadatasJson = JsonConvert.SerializeObject(securedMetadatas, Formatting.None, _jsonSerializerSettings),
                AssociationMetadatasJson = JsonConvert.SerializeObject(associationApps, Formatting.None, _jsonSerializerSettings),
                CompositionMetadatasJson = JsonConvert.SerializeObject(compositonApps, Formatting.None, _jsonSerializerSettings),
                MenuJson = JsonConvert.SerializeObject(securedMenu, Formatting.None, _jsonSerializerSettings),
                CommandBarsJson = JsonConvert.SerializeObject(commandBars, Formatting.None, _jsonSerializerSettings),
                AppConfiguration = _appConfigurationProvider.AppConfig(),
            };

            Log.InfoFormat("Download Metadata executed in {0}", LoggingUtil.MsDelta(watch));
            return response;
        }

        [HttpGet]
        public async Task<MobileMetadataDownloadResponseDefinition> DownloadMetadatas() {
            return await DownloadMetadatas(new MetadataDownloadDto { DeviceData = new DeviceData() });
        }

        [HttpPost]
        public async Task<SynchronizationResultDto> PullNewData([FromBody] SynchronizationRequestDto synchronizationRequest) {
            await _offlineAuditManager.MarkSyncOperationBegin(synchronizationRequest.ClientOperationId, synchronizationRequest.DeviceData, OfflineAuditManager.OfflineAuditMode.Data);
            var synchronizationResultDto = await _syncManager.GetData(synchronizationRequest, SecurityFacade.CurrentUser());
            _offlineAuditManager.PopulateSyncOperationWithTopData(synchronizationRequest.ClientOperationId, synchronizationResultDto);
            return synchronizationResultDto;
        }

        [HttpPost]
        public async Task<AssociationSynchronizationResultDto> PullAssociationData([FromBody] AssociationSynchronizationRequestDto request) {
            await _offlineAuditManager.MarkSyncOperationBegin(request.ClientOperationId, request.DeviceData, OfflineAuditManager.OfflineAuditMode.Association);
            var associationResult = await _syncManager.GetAssociationData(SecurityFacade.CurrentUser(), request);
            _offlineAuditManager.PopulateSyncOperationWithAssociationData(request.ClientOperationId, associationResult);
            return associationResult;
        }

        [HttpPost]
        public async Task<ISet<ScriptSyncResultDTO>> BuildSyncMap([FromBody]OfflineScriptSyncDtoRequest clientRequest) {
            return await _jsService.SyncResult(clientRequest.ClientState, new ScriptDeviceInfo {
                Platform = ClientPlatform.Mobile,
                OfflineDevice = clientRequest.offlineDevice,
                OfflineVersions = clientRequest.offlineVersion
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="application">Name of the application of this batch</param>
        /// <param name="remoteId">Id of the batch on the remote end</param>
        /// <param name="clientOperationId">Id of the whole syncoperation on the remote end</param>
        /// <param name="deviceData"></param>
        /// <param name="batchContent">The </param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Batch> SubmitBatch([FromUri]string application, [FromUri]string remoteId, JObject batchContent, [FromUri]string clientOperationId = null, [FromUri]DeviceData deviceData = null) {
            var operation = await _offlineAuditManager.MarkSyncOperationBegin(clientOperationId, deviceData, OfflineAuditManager.OfflineAuditMode.Batch);
            Log.InfoFormat("Creating batch for application {0}", application);
            // return the generated Batch to be serialized
            var batch = _offLineBatchService.SubmitBatch(application, remoteId, operation, batchContent);


            return batch;
        }

        [HttpGet]
        public async Task<IList<Batch>> BatchStatus([FromUri]IList<string> ids) {
            return await _offLineBatchService.GetBatchesByRemoteIds(ids);
        }

        #region Reporting
        [HttpGet]
        public async Task<string> Counts() {
            var user = SecurityFacade.CurrentUser();

            var req = new SynchronizationRequestDto() {
                ReturnNewApps = true,
                UserData = new UserSyncData(user)
            };

            var context = _contextLookuper.LookupContext();
            context.OfflineMode = true;
            _contextLookuper.AddContext(context);

            var watch = Stopwatch.StartNew();

            var appData = await _syncManager.GetData(req, user);
            watch.Stop();
            var appEllapsed = watch.ElapsedMilliseconds;

            watch.Restart();
            var associationResult = await _syncManager.GetAssociationData(user, null);
            watch.Stop();
            var associationEllapsed = watch.ElapsedMilliseconds;

            var topCountData = appData.TopApplicationData.OrderBy(a => a.ApplicationName).ToDictionary(applicationData => applicationData.ApplicationName, applicationData => applicationData.NewCount);
            var associationCounts = associationResult.AssociationData.OrderBy(a => a.Key).ToDictionary(applicationData => applicationData.Key, applicationData => applicationData.Value.Count);
            var compositionCounts = appData.CompositionData.OrderBy(a => a.ApplicationName).ToDictionary(applicationData => applicationData.ApplicationName, applicationData => applicationData.NewCount);

            var associationTotals = associationCounts.Sum(s => s.Value);
            var topAppTotals = topCountData.Sum(s => s.Value);

            var report = new MobileCountReport() {
                TopAppCounts = topCountData,
                AssociationCounts = associationCounts,
                AssociationTotals = associationTotals,
                TopAppTotals = topAppTotals,
                CompositionCounts = compositionCounts,
                AppTimeEllapsed = appEllapsed,
                AssociationTimeEllapsed = associationEllapsed,
                UserData = new MobileUserDtoReport(user)
            };

            return JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented,
            new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public class MobileCountReport {
            public IDictionary<string, int> TopAppCounts = new Dictionary<string, int>();
            public IDictionary<string, int> AssociationCounts = new Dictionary<string, int>();
            public IDictionary<string, int> CompositionCounts = new Dictionary<string, int>();
            public long AssociationTimeEllapsed;
            public long AppTimeEllapsed;
            public int AssociationTotals;
            public int TopAppTotals;

            public MobileUserDtoReport UserData { get; set; }
        }

        public class MobileUserDtoReport {

            public MobileUserDtoReport(InMemoryUser user) {
                PersonId = user.MaximoPersonId;
                Properties = user.Genericproperties;
                Username = user.Login;
                OrgId = user.OrgId;
                SiteId = user.SiteId;
            }

            public string SiteId { get; set; }
            public string OrgId { get; set; }
            public string PersonId { get; set; }
            public string Username { get; set; }
            public IDictionary<string, object> Properties { get; set; }
        }
        #endregion
    }
}