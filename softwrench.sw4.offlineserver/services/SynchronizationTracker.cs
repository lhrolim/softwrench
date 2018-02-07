using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using softwrench.sw4.offlineserver.model;
using softwrench.sw4.offlineserver.model.dto;
using softwrench.sw4.offlineserver.model.dto.association;
using softwrench.sw4.offlineserver.services.util;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.offlineserver.services {

    public class SynchronizationTracker : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO SwdbDAO { get; set; }

        [Import]
        public IConfigurationFacade ConfigurationFacade { get; set; }

        public async Task<SynchronizationRequestDto> ReConstructOperation([NotNull]string clientOperationId) {
            var op = await SwdbDAO.FindSingleByQueryAsync<SyncOperation>(SyncOperation.ByExternalId, clientOperationId);
            if (op == null) {
                return null;
            }
            var userId = op.User.Id.Value;
            JObject ob = null;
            List<string> itemsToDownload = null;

            if (op.Inputs.Any()) {
                var rowstampChunks = op.Inputs.Where(i => i.Key.StartsWith("rowstampmap_")).ToList();
                rowstampChunks.Sort();
                var sb = new StringBuilder();
                foreach (var chunk in rowstampChunks) {
                    sb.Append(chunk.Value);
                }
                ob = JObject.Parse(sb.ToString());

                var itemsToDownloadSt = op.Inputs.FirstOrDefault(f => f.Key.Equals("itemstodownload"))?.Value;
                if (itemsToDownloadSt != null) {
                    itemsToDownload = new List<string>(itemsToDownloadSt.Split(','));
                }
            }

            return new SynchronizationRequestDto {
                ReturnNewApps = true,
                UserData = new UserSyncData(SecurityFacade.GetInMemoryUser(userId)),
                RowstampMap = ob,
                InitialLoad = op.InitialLoad,
                ItemsToDownload = itemsToDownload
            };

        }


        public async Task<AssociationSynchronizationRequestDto> ReConstructAssociationOperation([NotNull]string clientOperationId) {
            var op = await SwdbDAO.FindSingleByQueryAsync<SyncOperation>(SyncOperation.ByExternalId, clientOperationId);
            if (op == null) {
                return null;
            }
            var userId = op.User.Id.Value;
            JObject ob = null;

            if (op.Inputs.Any()) {
                var rowstampChunks = op.Inputs.Where(i => i.Key.StartsWith("assrowstampmap_")).ToList();
                rowstampChunks.Sort();
                var sb = new StringBuilder();
                foreach (var chunk in rowstampChunks) {
                    sb.Append(chunk.Value);
                }
                ob = JObject.Parse(sb.ToString());
            }

            return new AssociationSynchronizationRequestDto {
                UserData = new UserSyncData(SecurityFacade.GetInMemoryUser(userId)),
                RowstampMap = ob,
                InitialLoad = op.InitialLoad,
            };

        }

        public void PopulateTopAppInputs(SyncOperation operation, SynchronizationRequestDto syncRequest) {

            if (!AuditingEnabled()) {
                return;
            }

            if (syncRequest.RowstampMap != null) {


                var rm = JsonConvert.SerializeObject(syncRequest.RowstampMap, Newtonsoft.Json.Formatting.None,
                    new JsonSerializerSettings {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });


                var chunks = Math.Ceiling((double)rm.Length / 4000);
                for (var i = 0;i < chunks;i++) {
                    var size = Math.Min(4000, (rm.Length - (4000 * i)));
                    operation.Inputs.Add(new SyncOperationInput {
                        Key = "rowstampmap_" + i,
                        Value = rm.Substring(4000 * i, size)
                    });
                }



            }

            if (syncRequest.ItemsToDownload != null) {
                operation.Inputs.Add(new SyncOperationInput {
                    Key = "itemstodownload",
                    Value = string.Join(",", syncRequest.ItemsToDownload)
                });
            }
        }

        private bool AuditingEnabled() {
            return ConfigurationFacade.Lookup<bool>(OfflineConstants.EnableParameterAuditing);
        }

        public void PopulateAssociationInputs(SyncOperation operation, AssociationSynchronizationRequestDto syncRequest) {

            if (!AuditingEnabled()) {
                return;
            }

            if (syncRequest.RowstampMap != null) {
                var rm = JsonConvert.SerializeObject(syncRequest.RowstampMap, Newtonsoft.Json.Formatting.None,
                    new JsonSerializerSettings {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });


                var chunks = Math.Ceiling((double)rm.Length / 4000);
                for (var i = 0;i < chunks;i++) {
                    var size = Math.Min(4000, (rm.Length - (4000 * i)));
                    operation.Inputs.Add(new SyncOperationInput {
                        Key = "assrowstampmap_" + i,
                        Value = rm.Substring(4000 * i, size)
                    });
                }



            }


        }
    }
}
