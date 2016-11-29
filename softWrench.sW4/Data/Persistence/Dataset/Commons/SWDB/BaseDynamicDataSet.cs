using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using NHibernate.Linq;
using NHibernate.Util;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Dynamic;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB {
    public class BaseDynamicDataSet : SWDBApplicationDataset {

        private readonly ScriptsService _scriptsService;
        private readonly IDynComponentEmailer _dynComponentEmailer;

        public BaseDynamicDataSet(ScriptsService scriptsService, IDynComponentEmailer dynComponentEmailer) {
            _scriptsService = scriptsService;
            _dynComponentEmailer = dynComponentEmailer;
        }

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData) {
            if (operationData.Operation.EqualsIc(OperationConstants.CRUD_DELETE)) {
                json["comment"] = operationData.Comment;
                json["username"] = operationData.Username;
            }
            return await base.Execute(application, json, operationData);
        }

        public override TargetResult DoExecute(OperationWrapper operationWrapper) {
            var id = string.IsNullOrEmpty(operationWrapper.Id) ? (int?)null : int.Parse(operationWrapper.Id);
            var entry = (ScriptEntry)null;
            var origEntry = id != null ? SWDAO.FindByPK<ScriptEntry>(typeof(ScriptEntry), id) : null;
            var json = operationWrapper.JSON;
            if (operationWrapper.OperationName.EqualsIc(OperationConstants.CRUD_DELETE)) {
                DeleteEntry(origEntry, json);
            } else if (operationWrapper.OperationName.EqualsIc(OperationConstants.CRUD_CREATE) || operationWrapper.OperationName.EqualsIc(OperationConstants.CRUD_UPDATE)) {
                entry = SaveEntry(ref id, json, origEntry);
            }
            return new TargetResult(id.ToString(), entry == null ? "" : entry.Name, entry);
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);
            result.ResultObject["systemversion"] = _scriptsService.GetSystemVersion();

            if (application.Schema.Stereotype == SchemaStereotype.DetailNew) {
                result.ResultObject["deploy"] = true;
                result.ResultObject["appliestoversion"] = _scriptsService.GetSystemVersion();
                return result;
            }

            SetDatesAndFlags(result.ResultObject);
            return result;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result =await base.GetList(application, searchDto);
            var datamaps = result.ResultObject;
            var datamapList = datamaps as IList<AttributeHolder> ?? datamaps.ToList();
            if (datamapList.Any()) {
                datamapList.ForEach(SetDatesAndFlags);
            }
            return result;
        }

        protected virtual ScriptEntry SaveEntry(ref int? id, JObject daramap, ScriptEntry origEntry) {
            var script = daramap["script"].ToObject<string>();
            var entry = new ScriptEntry {
                Id = id,
                Name = daramap["name"].ToObject<string>(),
                Target = daramap["target"].ToObject<string>(),
                Description = daramap["description"] == null ? null : daramap["description"].ToObject<string>(),
                Script = script,
                Deploy = daramap["deploy"].ToObject<bool>(),
                Lastupdate = DateTime.Now,
                Appliestoversion = (string)daramap["appliestoversion"],
                Isuptodate = origEntry != null && _scriptsService.SameScript(script, origEntry.Script),
                Isoncontainer = origEntry != null && origEntry.Isoncontainer
            };

            var shouldbeoncontainer = _scriptsService.ShouldBeOnContainer(entry);
            _scriptsService.ValidateScriptEntry(entry, shouldbeoncontainer);

            entry = SWDAO.Save(entry);

            var saveAndReloadToken = daramap["saveandreload"];
            var saveAndReload = saveAndReloadToken != null && saveAndReloadToken.ToObject<bool>();
            if (saveAndReload) {
                _scriptsService.ReloadContainer(entry);
            }

            // those transient need to be set after save
            entry.Status = _scriptsService.GetStatus(shouldbeoncontainer, entry.Isoncontainer, entry.Isuptodate);
            entry.Comment = daramap["comment"] == null ? "" : daramap["comment"].ToObject<string>();
            entry.Username = daramap["username"] == null ? "" : daramap["username"].ToObject<string>();

            var emailEntry = entry.ShallowCopy();
            Task.Run(() => {
                SendEmail(emailEntry, origEntry, saveAndReload);
            });

            return entry;
        }

        protected virtual void DeleteEntry(ScriptEntry entry, JObject json) {
            var emailEntry = entry.ShallowCopy();
            Task.Run(() => {
                SendDeleteEmail(emailEntry, json);
            });

            // force container undeploy
            if (entry.Isoncontainer) {
                entry.Deploy = false;
                _scriptsService.ReloadContainer(entry);
            }

            SWDAO.Delete(entry);
        }

        protected virtual void SetDatesAndFlags(AttributeHolder datamap) {
            var deploy = (bool)datamap["deploy"];
            var version = (string)datamap["appliestoversion"];
            var isOnContainer = (bool)datamap["isoncontainer"];
            var isUpToDate = (bool)datamap["isuptodate"];
            var shouldBeOnContainer = _scriptsService.ShouldBeOnContainer(deploy, version);
            datamap["status"] = _scriptsService.GetStatus(shouldBeOnContainer, isOnContainer, isUpToDate);
        }

        protected virtual void SendEmail(ScriptEntry entry, ScriptEntry origEntry, bool saveAndReload) {
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

        protected virtual void SendUpdateEmail(ScriptEntry entry, ScriptEntry origEntry, string reloadSufix) {
            var sendEmail = !entry.Target.Equals(origEntry.Target);
            sendEmail = sendEmail || !entry.Appliestoversion.Equals(origEntry.Appliestoversion);
            sendEmail = sendEmail || !entry.Deploy.Equals(origEntry.Deploy);
            sendEmail = sendEmail || !_scriptsService.SameScript(origEntry.Script, entry.Script);
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

        protected virtual void SendDeleteEmail(ScriptEntry origEntry, JObject json) {
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

        public override string ApplicationName() {
            return "_dynamic";
        }
    }
}
