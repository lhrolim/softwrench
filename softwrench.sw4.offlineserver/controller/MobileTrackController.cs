using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using cts.commons.web.Controller;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using softwrench.sw4.offlineserver.model.dto;
using softwrench.sw4.offlineserver.model.dto.association;
using softwrench.sw4.offlineserver.services;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.offlineserver.controller {


    [Authorize]
    [SWControllerConfiguration]
    public class MobileTrackController : ApiController {

        private readonly SynchronizationTracker _synchTracker;
        private readonly IContextLookuper _contextLookuper;
        private readonly SynchronizationManager _syncManager;

        public MobileTrackController(SynchronizationTracker synchTracker, IContextLookuper contextLookuper, SynchronizationManager syncManager) {
            _synchTracker = synchTracker;
            _contextLookuper = contextLookuper;
            _syncManager = syncManager;
        }


        [HttpGet]
        public async Task<string> TopAppData(string clientOperationId = null, int? userId = null) {

            InMemoryUser user;

            SynchronizationRequestDto req;

            if (userId == null) {
                user = SecurityFacade.CurrentUser();
            } else {
                user = SecurityFacade.GetInMemoryUser(userId.Value);
                if (user == null) {
                    throw new InvalidOperationException("user not found");
                }
            }
            if (clientOperationId != null) {
                req = await _synchTracker.ReConstructOperation(clientOperationId);
            } else {
                req = new SynchronizationRequestDto {
                    ReturnNewApps = true,
                    UserData = new UserSyncData(user)
                };
            }
            var context = _contextLookuper.LookupContext();
            context.OfflineMode = true;
            _contextLookuper.AddContext(context);
            var appData = await _syncManager.GetData(req, user);

            return JsonConvert.SerializeObject(appData);




        }

        [HttpGet]
        public async Task<string> AssociationData(string clientOperationId = null, int? userId = null) {

            InMemoryUser user;

            AssociationSynchronizationRequestDto assReq;

            if (userId == null) {
                user = SecurityFacade.CurrentUser();
            } else {
                user = SecurityFacade.GetInMemoryUser(userId.Value);
                if (user == null) {
                    throw new InvalidOperationException("user not found");
                }
            }

            if (clientOperationId != null) {
                assReq = await _synchTracker.ReConstructAssociationOperation(clientOperationId);

            } else {
                assReq = new AssociationSynchronizationRequestDto {
                    UserData = new UserSyncData(user),
                    InitialLoad = true
                };
            }

            var context = _contextLookuper.LookupContext();
            context.OfflineMode = true;
            _contextLookuper.AddContext(context);

            var watch = Stopwatch.StartNew();

            watch.Restart();
            var associationResult = await _syncManager.GetAssociationData(user, assReq);
            return JsonConvert.SerializeObject(associationResult);

        }

        [HttpGet]
        public async Task<string> Report(string clientOperationId = null, int? userId = null) {

            InMemoryUser user = null;
            JObject rowstampMap = null;

            SynchronizationRequestDto req = null;
            AssociationSynchronizationRequestDto assReq = null;

            if (userId == null) {
                user = SecurityFacade.CurrentUser();
            } else {
                user = SecurityFacade.GetInMemoryUser(userId.Value);
                if (user == null) {
                    throw new InvalidOperationException("user not found");
                }
            }


            if (clientOperationId != null) {
                req = await _synchTracker.ReConstructOperation(clientOperationId);
                assReq = await _synchTracker.ReConstructAssociationOperation(clientOperationId);

            } else {
                req = new SynchronizationRequestDto {
                    ReturnNewApps = true,
                    UserData = new UserSyncData(user)
                };
                assReq = new AssociationSynchronizationRequestDto {
                    UserData = new UserSyncData(user),
                    InitialLoad = true
                };
            }


            var context = _contextLookuper.LookupContext();
            context.OfflineMode = true;
            _contextLookuper.AddContext(context);

            var watch = Stopwatch.StartNew();

            var appData = await _syncManager.GetData(req, user);


            watch.Stop();
            var appEllapsed = watch.ElapsedMilliseconds;

            watch.Restart();
            var associationResult = await _syncManager.GetAssociationData(user, assReq);
            watch.Stop();
            var associationEllapsed = watch.ElapsedMilliseconds;

            var topCountData = appData.TopApplicationData.OrderBy(a => a.ApplicationName).ToDictionary(applicationData => applicationData.ApplicationName, applicationData => applicationData.NewCount);
            var associationCounts = associationResult.AssociationData.OrderBy(a => a.Key).ToDictionary(applicationData => applicationData.Key, applicationData => applicationData.Value.Count);
            var compositionCounts = appData.CompositionData.OrderBy(a => a.ApplicationName).ToDictionary(applicationData => applicationData.ApplicationName, applicationData => applicationData.NewCount);

            var associationTotals = associationCounts.Sum(s => s.Value);
            var topAppTotals = topCountData.Sum(s => s.Value);

            var report = new MobileController.MobileCountReport() {
                TopAppCounts = topCountData,
                AssociationCounts = associationCounts,
                AssociationTotals = associationTotals,
                TopAppTotals = topAppTotals,
                CompositionCounts = compositionCounts,
                AppTimeEllapsed = appEllapsed,
                AssociationTimeEllapsed = associationEllapsed,
                UserData = new MobileController.MobileUserDtoReport(user)
            };

            return JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented,
            new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

    }
}
