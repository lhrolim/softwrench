using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Dynamic.Model;
using softWrench.sW4.Util;

namespace softWrench.sW4.Dynamic.Services {

    public class JavascriptDynamicService : BaseScriptService {

        public JavascriptDynamicService(ISWDBHibernateDAO dao) : base(dao) {
        }


        public IDictionary<string, long> ConvertToRowstampDict(JObject rowstampMap) {
            if (rowstampMap == null) {
                return new Dictionary<string, long>();
            }
            var result = new Dictionary<string, long>();
            dynamic obj = rowstampMap;
            //Loop over the array
            foreach (dynamic row in obj.items) {
                var name = row.Name;
                var rowstamp = row.Value.Value;
                //TODO: implement other fields
                result.Add(name, rowstamp);
            }
            return result;
        }



        public async Task<ISet<ScriptSyncResultDTO>> SyncResult(IDictionary<string, long> clientState, [NotNull]ScriptDeviceInfo deviceInfo) {
            deviceInfo.Validate(ScriptDeviceInfo.DeviceInfoValMode.Request);

            IDictionary<string, ScriptSyncResultDTO> result = new Dictionary<string, ScriptSyncResultDTO>();
            var deployedScripts = await DAO.FindByQueryAsync<JavascriptEntry>(JavascriptEntry.DeployedScripts);

            if (clientState == null) {
                //to avoid null pointers later on
                clientState = new Dictionary<string, long>();
            }


            foreach (var deployedScript in deployedScripts) {
                if (!ConditionsMatch(deployedScript, deviceInfo)) {
                    continue;
                }


                if (clientState.ContainsKey(deployedScript.Target)) {
                    if (deployedScript.Lastupdate > clientState[deployedScript.Target]) {
                        //we have an updated version at the server side
                        result.Add(deployedScript.Target, ScriptSyncResultDTO.FromEntity(deployedScript));
                    } else {
                        result.Add(deployedScript.Target, null);
                    }
                } else {
                    //this is a new script for the customer
                    //if the conditions are met should be simply add to the result list
                    result.Add(deployedScript.Target, ScriptSyncResultDTO.FromEntity(deployedScript));
                }
            }

            //second pass --> marking non deployed scripts for deletion at the client side
            foreach (var clientItem in clientState) {
                if (!result.ContainsKey(clientItem.Key)) {
                    result.Add(clientItem.Key, new ScriptSyncResultDTO { Target = clientItem.Key, ToDelete = true });
                }
            }

            return result.Values.Where(v => v != null).ToHashSet();
        }

        private bool ConditionsMatch(JavascriptEntry deployedScript, [NotNull]ScriptDeviceInfo deviceInfo) {
            var deployedCriteria = deployedScript.ScriptDeviceCriteria;

            var deployedPlatformMatch = deployedCriteria.Platform.Match(deviceInfo.Platform);

            var deviceMatch = deployedCriteria.OfflineDevice.Match(deviceInfo.OfflineDevice);

            var versionMatch = true;

            if (deviceInfo.Platform.Equals(ClientPlatform.Web)) {
                //if it is an offline request, there´s no point to check if the server version is the same as the one 
                versionMatch = deployedScript.Appliestoversion.Equals(GetSystemVersion());
            } else if (deployedCriteria.OfflineVersions != null) {
                var versions = new HashSet<string>(deployedCriteria.OfflineVersions.Split(','));
                if (!"ripple".EqualsIc(deviceInfo.OfflineVersions) && !versions.Contains(deviceInfo.OfflineVersions)) {
                    versionMatch = false;
                }
            }

            return deployedPlatformMatch && deviceMatch && versionMatch;
        }

        public override void ReloadContainer(AScriptEntry entry){
            //NOOP
        }
    }
}
