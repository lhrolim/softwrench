using System;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Dynamic.Model;
using softWrench.sW4.Dynamic.Services;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB.Scripts {

    public class JavascriptDynamicDataSet : BaseDynamicScriptDataSet {

        public JavascriptDynamicDataSet(IDynComponentEmailer dynComponentEmailer, JavascriptDynamicService service) : base(dynComponentEmailer, service) {
        }


        [Transactional(DBType.Swdb)]
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            var id = string.IsNullOrEmpty(operationWrapper.Id) ? (int?)null : int.Parse(operationWrapper.Id);
            var entry = (AScriptEntry)null;
            var origEntry = id != null ? SWDAO.FindByPK<JavascriptEntry>(typeof(JavascriptEntry), id) : null;
            var json = operationWrapper.JSON;
            if (operationWrapper.OperationName.EqualsIc(OperationConstants.CRUD_DELETE)) {
                DeleteEntry(origEntry, json);
            } else if (operationWrapper.OperationName.EqualsIc(OperationConstants.CRUD_CREATE) || operationWrapper.OperationName.EqualsIc(OperationConstants.CRUD_UPDATE)) {
                entry = SaveEntry(ref id, json, origEntry);
            }
            return new TargetResult(id.ToString(), entry == null ? "" : entry.Name, entry);
        }



        protected virtual JavascriptEntry SaveEntry(ref int? id, JObject datamap, JavascriptEntry origEntry) {
            var script = datamap["script"].ToObject<string>();

            var entry = new JavascriptEntry {
                Id = id,
                Name = datamap.StringValue("name"),
                Target = datamap.StringValue("target"),
                Description = datamap.StringValue("description"),
                Script = script,
                Deploy = datamap["deploy"].ToObject<bool>(),
                Lastupdate = DateTime.Now.ToUnixTimeStamp(),
                Appliestoversion = datamap.StringValue("appliestoversion"),
            };

            ClientPlatform plat;
            OfflineDevice dev;


            Enum.TryParse(datamap.StringValue("platform"), true, out plat);
            Enum.TryParse(datamap.StringValue("offlineDevice"), true, out dev);
            var offlineVersions = datamap.StringValue("offlineversions");

            var devInfo = new ScriptDeviceInfo {
                Platform = plat,
                OfflineDevice = dev,
                OfflineVersions = offlineVersions
            }.Validate(ScriptDeviceInfo.DeviceInfoValMode.Database);

            entry.ScriptDeviceCriteria = devInfo;

            entry = SWDAO.Save(entry);

            // those transient need to be set after save

            entry.Comment = datamap.StringValue("comment") ?? "";
            entry.Username = datamap.StringValue("username") ?? "";

            var emailEntry = entry.ShallowCopy();
            Task.Run(() => {
                SendEmail(emailEntry, origEntry, entry.Deploy);
            });

            return entry;
        }


        public override string ApplicationName() {
            return "_jsdynamic";
        }


    }
}
