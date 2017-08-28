using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.Util;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softwrench.sw4.offlineserver.dto;
using softwrench.sw4.offlineserver.dto.association;
using softwrench.sw4.offlineserver.model;
using softwrench.sw4.offlineserver.services.util;
using softwrench.sW4.audit.classes.Model;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using softWrench.sW4.Data.Persistence.Relational.Cache.Core;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.offlineserver.audit {

    public class OfflineAuditManager : ISingletonComponent {


        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public ObjectRedisManager RedisManager { get; set; }

        [Import]
        public IConfigurationFacade ConfigFacade { get; set; }

        [Import]
        public IAuditManager AuditManager { get; set; }

        private readonly ILog _log = LogManager.GetLogger(typeof(OfflineAuditManager));

        private readonly TimeSpan _defaultExpiresIn = TimeSpan.FromMinutes(3);


        public void PopulateSyncOperationWithTopData(string clientOperationId, SynchronizationResultDto synchronizationResultDto) {

            var user = SecurityFacade.CurrentUser();
            var key = SyncOperation.GenerateKey(user, clientOperationId);

            if (!ShouldAudit(key)) {
                return;
            }
            lock (string.Intern(key)) {
                var operation = RedisManager.Lookup<SyncOperation>(key);
                if (operation == null) {
                    _log.WarnFormat("could not locate audit sync operation for {0}", key);
                } else {
                    var topCountData = synchronizationResultDto.TopApplicationData.OrderBy(a => a.ApplicationName)
                        .ToDictionary(applicationData => applicationData.ApplicationName,
                            applicationData => applicationData.NewCount);
                    var compositionCounts = synchronizationResultDto.CompositionData.OrderBy(a => a.ApplicationName)
                        .ToDictionary(applicationData => applicationData.ApplicationName,
                            applicationData => applicationData.NewCount);

                    var topAppTotals = topCountData.Sum(s => s.Value);
                    var compositionTotals = compositionCounts.Sum(s => s.Value);
                    operation.TopAppCounts = topAppTotals;
                    operation.CompositionCounts = compositionTotals;

                    if (operation.AssociationCounts != null) {
                        SaveOperation(operation);
                        return;
                    }
                    RedisManager.Insert(new BaseRedisInsertKey(key, _defaultExpiresIn), operation);
                }
            }
        }



        public void PopulateSyncOperationWithAssociationData(string requestClientOperationId, AssociationSynchronizationResultDto associationResult) {

            var user = SecurityFacade.CurrentUser();
            var key = SyncOperation.GenerateKey(user, requestClientOperationId);

            if (!ShouldAudit(key)) {
                return;
            }

            lock (string.Intern(key)) {
                var operation = RedisManager.Lookup<SyncOperation>(key);
                if (operation == null) {
                    _log.WarnFormat("could not locate audit sync operation for {0}", key);
                } else {
                    operation.AssociationCounts = associationResult.TotalCount;
                    if (operation.TopAppCounts != null) {
                        SaveOperation(operation);
                        return;
                    }
                    RedisManager.Insert(new BaseRedisInsertKey(key, _defaultExpiresIn), operation);
                }
            }
        }

        public enum OfflineAuditMode {
            Metadata, Batch, Data, Association
        }

        public SyncOperation MarkSyncOperationBegin(string requestClientOperationId, DeviceData deviceData, OfflineAuditMode mode) {


            var user = SecurityFacade.CurrentUser();
            var key = SyncOperation.GenerateKey(user, requestClientOperationId);

            if (!ShouldAudit(key, mode == OfflineAuditMode.Batch)) {
                return null;
            }

            SyncOperation operation= null;

            lock (string.Intern(key)) {
                //to ensure a lock, waiting for 10s to acquire it or giving up
                operation = RedisManager.Lookup<SyncOperation>(key);
                if (operation == null) {
                    operation = GenerateOperation(requestClientOperationId, deviceData, user, key, mode == OfflineAuditMode.Metadata);
                }
                if (OfflineAuditMode.Batch.Equals(mode)) {
                    AuditManager.InitThreadTrail(operation.AuditTrail);
                }
            }

            return operation;
        }


        private void SaveOperation(SyncOperation operation) {
            //saving at the database on another thread to avoid performance hit
            Task.Run(() => {
                var user = SecurityFacade.CurrentUser();
                operation.User = user.DBUser;
                operation.AuditTrail.EndTime = DateTime.Now;
                operation.RegisterTime = DateTime.Now;
                operation.MetadataDownload = false;
                var userPropsJson = JsonConvert.SerializeObject(user.GenericSyncProperties, Formatting.None,
                    new JsonSerializerSettings {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                operation.UserProperties = userPropsJson;
                Dao.Save(operation);
            });
        }



        private SyncOperation GenerateOperation(string requestClientOperationId, DeviceData deviceData, InMemoryUser user, string key, bool metadataDownload) {
            var operation = new SyncOperation();
            var trail = new AuditTrail {
                BeginTime = DateTime.Now,
                Operation = "sync",
                Name = "offline",
                ExternalId = requestClientOperationId,
                Session = new AuditSession(user.SessionAuditId),
                ShouldPersist = true
            };
            operation.ServerEnv = ApplicationConfiguration.Profile;
            operation.ServerVersion = ApplicationConfiguration.SystemVersion;
            operation.TimezoneOffset = user.TimezoneOffset;
            operation.DeviceData = deviceData;
            operation.MetadataDownload = metadataDownload;
            operation.AuditTrail = trail;
            RedisManager.Insert(new BaseRedisInsertKey(key) { ExpiresIn = _defaultExpiresIn }, operation);
            return operation;
        }


        private bool ShouldAudit(string key, bool isBatchOperation = false) {
            if (!RedisManager.IsAvailable()) {
                return false;
            }

            if (ConfigFacade.Lookup<bool>(OfflineConstants.EnableAudit)) {
                return true;
            }
            if (ConfigFacade.Lookup<bool>(AuditConstants.AuditEnabled)) {
                //only maximo auditing is enabled, thus we will only audit it if there´s a batch operation present
                return isBatchOperation;
            }

            return false;


        }

        public void MarkBatchCompleted(SyncOperation operation) {
            var before = Stopwatch.StartNew();
            var key = SyncOperation.GenerateKey(SecurityFacade.CurrentUser(),operation.AuditTrail.ExternalId);
            if (!ShouldAudit(key, true)) {
                return;
            }
            var currentTrail = AuditManager.CurrentTrail();
            //updating current thread trail
            operation.AuditTrail = currentTrail;
            RedisManager.Insert(new BaseRedisInsertKey(key) { ExpiresIn = _defaultExpiresIn }, operation);
            LoggingUtil.BaseDurationMessage("updating batch syncoperation", before);
        }
    }
}
