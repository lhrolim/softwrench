﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using Castle.Core.Internal;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softwrench.sw4.offlineserver.model;
using softwrench.sw4.offlineserver.model.dto;
using softwrench.sw4.offlineserver.model.dto.association;
using softwrench.sw4.offlineserver.model.exception;
using softwrench.sw4.offlineserver.services;
using softwrench.sw4.offlineserver.services.util;
using softwrench.sW4.audit.classes.Model;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Configuration.Services.Api;
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

        [Import]
        public SynchronizationTracker SynchTracker { get; set; }


        private readonly ILog _log = LogManager.GetLogger(typeof(OfflineAuditManager));

        private readonly TimeSpan _defaultExpiresIn = TimeSpan.FromMinutes(3);


        /// <summary>
        /// Marks the end of the PullData Operation
        /// </summary>
        /// <param name="syncRequest"></param>
        /// <param name="synchronizationResultDto"></param>
        /// <param name="forceSave">if true persists the operation by the end of this method (useful for reports)</param>
        public void PopulateSyncOperationWithTopData(SynchronizationRequestDto syncRequest, SynchronizationResultDto synchronizationResultDto, bool forceSave = false) {

            var user = SecurityFacade.CurrentUser();


            if (!ShouldAudit()) {
                return;
            }

            var clientOperationId = syncRequest.ClientOperationId;
            var key = SyncOperation.GenerateKey(user, clientOperationId);
            lock (string.Intern(key)) {
                var operation = RedisManager.Lookup<SyncOperation>(key);
                var trail = AuditManager.CurrentTrail();
                if (operation == null) {
                    _log.WarnFormat("could not locate audit sync operation for {0}", key);
                } else {
                    var topCountData = new Dictionary<string, int>();
                    synchronizationResultDto.TopApplicationData.OrderBy(a => a.ApplicationName).ForEach(applicationData => {
                        if (!topCountData.ContainsKey(applicationData.ApplicationName)) {
                            topCountData.Add(applicationData.ApplicationName, applicationData.NewCount);
                        } else {
                            topCountData[applicationData.ApplicationName] = topCountData[applicationData.ApplicationName] + applicationData.NewCount;
                        }
                    });

                    var compositionCounts = new Dictionary<string, int>();
                    synchronizationResultDto.CompositionData.OrderBy(a => a.ApplicationName).ForEach(applicationData => {
                        if (!compositionCounts.ContainsKey(applicationData.ApplicationName)) {
                            compositionCounts.Add(applicationData.ApplicationName, applicationData.NewCount);
                        } else {
                            compositionCounts[applicationData.ApplicationName] = compositionCounts[applicationData.ApplicationName] + applicationData.NewCount;
                        }
                    });

                    var topAppTotals = topCountData.Sum(s => s.Value);
                    var compositionTotals = compositionCounts.Sum(s => s.Value);
                    operation.TopAppCounts = topAppTotals;
                    operation.CompositionCounts = compositionTotals;
                    operation.AuditTrail.Queries.AddAll(trail.Queries);

                    SynchTracker.PopulateTopAppInputs(operation, syncRequest);




                    if (operation.AssociationCounts != null || forceSave) {
                        if (forceSave) {
                            operation.AssociationCounts = -1;
                        }
                        SaveOperation(operation);
                        return;
                    }
                    RedisManager.Insert(new BaseRedisInsertKey(key, _defaultExpiresIn), operation);
                }
            }
        }



        public void PopulateSyncOperationWithAssociationData(AssociationSynchronizationRequestDto req, AssociationSynchronizationResultDto associationResult, bool forceSave = false) {

            var user = SecurityFacade.CurrentUser();


            if (!ShouldAudit()) {
                return;
            }

            var key = SyncOperation.GenerateKey(user, req.ClientOperationId);

            lock (string.Intern(key)) {
                var operation = RedisManager.Lookup<SyncOperation>(key);
                if (operation == null) {
                    _log.WarnFormat("could not locate audit sync operation for {0}", key);
                } else {
                    operation.InitialLoad = req.InitialLoad;
                    operation.AssociationCounts = associationResult.TotalCount;
                    SynchTracker.PopulateAssociationInputs(operation, req);

                    if (operation.TopAppCounts != null || forceSave) {
                        if (forceSave) {
                            //a non null is required
                            operation.TopAppCounts = -1;
                            operation.CompositionCounts = -1;
                        }

                        SaveOperation(operation);
                        return;
                    }
                    RedisManager.Insert(new BaseRedisInsertKey(key, _defaultExpiresIn), operation);
                }
            }
        }

        public enum OfflineAuditMode {
            Metadata, Batch, Data, Association, Exception
        }

        //        public async Task<SyncOperation> MarkSyncOperationException(string requestClientOperationId, DeviceData deviceData, OfflineAuditMode mode, string message, string stacktrace = null) {
        //            var syncOperation = MarkSyncOperationBegin(requestClientOperationId, deviceData, OfflineAuditMode.Exception);
        //            if (syncOperation != null && syncOperation.Id == null) {
        //                syncOperation.ErrorMessage = message;
        //                syncOperation.StackTrace = stacktrace;
        //                syncOperation = await Dao.SaveAsync(syncOperation);
        //
        //            }
        //
        //            return syncOperation;
        //
        //        }

        public async Task<SyncOperation> MarkSyncOperationBegin(string requestClientOperationId, DeviceData deviceData, OfflineAuditMode mode) {


            var user = SecurityFacade.CurrentUser();


            if (deviceData == null) {
                //playing safe, shouldn´t happen
                deviceData = new DeviceData{ Model = "FKR", ClientVersion = "FKR",Platform = "FKR", Version = ApplicationConfiguration.SystemVersion};
            }

            var validversion = await ValidateOffLineVersion(deviceData.ClientVersion);

            if (!ShouldAudit(mode == OfflineAuditMode.Batch)) {
                if (!validversion) {
                    throw new InvalidOffLineVersionException(deviceData.ClientVersion);
                }

                return null;
            }

            var key = SyncOperation.GenerateKey(user, requestClientOperationId);

            SyncOperation operation = null;

            lock (string.Intern(key)) {
                //to ensure a lock, waiting for 10s to acquire it or giving up
                operation = RedisManager.Lookup<SyncOperation>(key);
                if (operation == null) {
                    operation = GenerateOperation(requestClientOperationId, deviceData, user, key, mode == OfflineAuditMode.Metadata);
                    if (!validversion) {
                        operation.ErrorMessage = InvalidOffLineVersionException.Msg.Fmt(deviceData.ClientVersion);
                        operation.SetDefaultCounts();
                        SaveOperation(operation, false);
                        RedisManager.Insert(new BaseRedisInsertKey(key) { ExpiresIn = _defaultExpiresIn }, operation);
                        throw new InvalidOffLineVersionException(deviceData.ClientVersion);
                    }
                } else if (!validversion) {
                    throw new InvalidOffLineVersionException(deviceData.ClientVersion);
                }
                if (OfflineAuditMode.Batch.Equals(mode)) {

                }
            }

            return operation;
        }


        private void SaveOperation(SyncOperation operation, bool ignoreErrorOperation = true) {
            if (ignoreErrorOperation && operation.IsErrorOperation()) {
                return;
            }
            //saving at the database on another thread to avoid performance hit
            Task.Run(() => {
                var user = SecurityFacade.CurrentUser();
                operation.User = user.DBUser;
                operation.TimezoneOffset = user.TimezoneOffset;
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
                ShouldPersist = true,
            };
            operation.ServerEnv = ApplicationConfiguration.Profile;
            operation.ServerVersion = ApplicationConfiguration.SystemVersion;
            operation.TimezoneOffset = user.TimezoneOffset;
            operation.DeviceData = deviceData;
            operation.MetadataDownload = metadataDownload;
            operation.AuditTrail = trail;
            operation.HasUploadOperation = false;
            RedisManager.Insert(new BaseRedisInsertKey(key) { ExpiresIn = _defaultExpiresIn }, operation);
            return operation;
        }

        private async Task<bool> ValidateOffLineVersion(string currentVersion) {
            var allowedVersions = await ConfigFacade.LookupAsync<string>(OfflineConstants.AllowedClientVersions);
            return VersionUtil.IsGreaterThan(currentVersion, allowedVersions);

        }


        private bool ShouldAudit(bool isBatchOperation = false) {
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

            if (!ShouldAudit(true)) {
                return;
            }
            var key = SyncOperation.GenerateKey(SecurityFacade.CurrentUser(), operation.AuditTrail.ExternalId);
            var currentTrail = AuditManager.CurrentTrail();
            if (currentTrail != null) {
                //updating current thread trail
                operation.AuditTrail = currentTrail;
            }

            operation.HasUploadOperation = true;
            RedisManager.Insert(new BaseRedisInsertKey(key) { ExpiresIn = _defaultExpiresIn }, operation);
            LoggingUtil.BaseDurationMessage("updating batch syncoperation", before);
        }

        //DO not make this asyncable
        public void InitThreadTrail(AuditTrail operationAuditTrail) {
            AuditManager.InitThreadTrail(operationAuditTrail);
        }
    }
}
