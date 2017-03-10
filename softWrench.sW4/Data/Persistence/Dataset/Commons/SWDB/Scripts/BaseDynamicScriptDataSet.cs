using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Dynamic.Model;
using softWrench.sW4.Dynamic.Services;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB.Scripts {
    public abstract class BaseDynamicScriptDataSet : SWDBApplicationDataset{

        private readonly IDynComponentEmailer _dynComponentEmailer;
        protected readonly BaseScriptService _scriptService;

        protected BaseDynamicScriptDataSet(IDynComponentEmailer dynComponentEmailer, BaseScriptService scriptService)
        {
            _dynComponentEmailer = dynComponentEmailer;
            _scriptService = scriptService;
        }

        protected virtual void SendEmail(AScriptEntry entry, AScriptEntry origEntry, bool saveAndReload) {
            var reloadSufix = saveAndReload ? (entry.Deploy ? " and Deployed" : " and Undeployed") : "";
            if (origEntry != null) {
                SendUpdateEmail(entry, origEntry, reloadSufix);
                return;
            }

            var email = new DynComponentCreatedEmail {
                Entry = entry,
                ReloadSufix = reloadSufix
            };
            _dynComponentEmailer.FillBaseEmailDTO(email, "", entry.Comment, entry.Username, "Dynamic Component Created" + reloadSufix);
            _dynComponentEmailer.SendDynComponentCreatedEmail(email);
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);
            result.ResultObject["systemversion"] = _scriptService.GetSystemVersion();

            if (application.Schema.Stereotype == SchemaStereotype.DetailNew) {
                result.ResultObject["deploy"] = true;
                result.ResultObject["appliestoversion"] = _scriptService.GetSystemVersion();
                return result;
            }

            return result;
        }

        protected virtual void SendUpdateEmail(AScriptEntry entry, AScriptEntry origEntry, string reloadSufix) {
            var sendEmail = !entry.Target.Equals(origEntry.Target);
            sendEmail = sendEmail || !entry.Appliestoversion.Equals(origEntry.Appliestoversion);
            sendEmail = sendEmail || !entry.Deploy.Equals(origEntry.Deploy);
            sendEmail = sendEmail || !_scriptService.SameScript(origEntry.Script, entry.Script);
            if (!sendEmail) {
                return;
            }

            var email = new DynComponentUpdatedEmail {
                OldEntry = origEntry,
                NewEntry = entry,
                ReloadSufix = reloadSufix
            };
            _dynComponentEmailer.FillBaseEmailDTO(email, "", entry.Comment, entry.Username, "Dynamic Component Updated" + reloadSufix);
            _dynComponentEmailer.SendDynComponentUpdatedEmail(email);
        }

        protected virtual void DeleteEntry(AScriptEntry entry, JObject json) {
            var emailEntry = entry.ShallowCopy();
            Task.Run(() => {
                SendDeleteEmail(emailEntry, json);
            });

            // force container undeploy
            if (entry.Isoncontainer) {
                entry.Deploy = false;
                _scriptService.ReloadContainer(entry);
            }

            SWDAO.Delete(entry);
        }

        protected virtual void SendDeleteEmail(AScriptEntry origEntry, JObject json) {
            origEntry.Comment = json["comment"].ToObject<string>();
            origEntry.Username = json["username"].ToObject<string>();
            var email = new DynComponentDeleteEmail {
                Entry = origEntry,
                ReloadSufix = origEntry.Isoncontainer ? " and Undeployed" : ""
            };

            var title = "Dynamic Component Deleted" + email.ReloadSufix;
            _dynComponentEmailer.FillBaseEmailDTO(email, "", origEntry.Comment, origEntry.Username, title);
            _dynComponentEmailer.SendDynComponentDeletedEmail(email);
        }

    }
}
