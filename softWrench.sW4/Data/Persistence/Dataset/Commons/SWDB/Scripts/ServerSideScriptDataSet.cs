using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Dynamic.Model;
using softWrench.sW4.Dynamic.Services;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB.Scripts {
    public class ServerSideScriptDataSet : BaseDynamicScriptDataSet {

        private readonly ScriptsService _scriptsService;

        public ServerSideScriptDataSet(ScriptsService scriptsService, IDynComponentEmailer dynComponentEmailer) : base(dynComponentEmailer, scriptsService) {
            _scriptsService = scriptsService;
        }

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData) {
            if (operationData.Operation.EqualsIc(OperationConstants.CRUD_DELETE)) {
                json["comment"] = operationData.Comment;
                json["username"] = operationData.Username;
            }
            return await base.Execute(application, json, operationData);
        }

        [Transactional(DBType.Swdb)]
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
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

            if (application.Schema.Stereotype != SchemaStereotype.DetailNew) {
                SetDatesAndFlags(result.ResultObject);
            }

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

        protected virtual ScriptEntry SaveEntry(ref int? id, JObject datamap, ScriptEntry origEntry) {
            var script = datamap["script"].ToObject<string>();
            var entry = new ScriptEntry {
                Id = id,
                Name = datamap["name"].ToObject<string>(),
                Target = datamap["target"].ToObject<string>(),
                Description = datamap["description"] == null ? null : datamap["description"].ToObject<string>(),
                Script = script,
                Deploy = datamap["deploy"].ToObject<bool>(),
                Lastupdate = DateTime.Now.ToUnixTimeStamp(),
                Appliestoversion = (string)datamap["appliestoversion"],
                Isuptodate = origEntry != null && _scriptsService.SameScript(script, origEntry.Script),
                Isoncontainer = origEntry != null && origEntry.Isoncontainer
            };

            var shouldbeoncontainer = _scriptsService.ShouldBeOnContainer(entry);
            _scriptsService.ValidateScriptEntry(entry, shouldbeoncontainer);

            entry = SWDAO.Save(entry);

            var saveAndReloadToken = datamap["saveandreload"];
            var saveAndReload = saveAndReloadToken != null && saveAndReloadToken.ToObject<bool>();
            if (saveAndReload) {
                _scriptsService.ReloadContainer(entry);
            }

            // those transient need to be set after save
            entry.Status = _scriptsService.GetStatus(shouldbeoncontainer, entry.Isoncontainer, entry.Isuptodate);
            entry.Comment = datamap["comment"] == null ? "" : datamap["comment"].ToObject<string>();
            entry.Username = datamap["username"] == null ? "" : datamap["username"].ToObject<string>();

            var emailEntry = entry.ShallowCopy();
            Task.Run(() => {
                SendEmail(emailEntry, origEntry, saveAndReload);
            });

            return entry;
        }

       

        protected virtual void SetDatesAndFlags(AttributeHolder datamap) {
            var deploy = (bool)datamap["deploy"];
            var version = (string)datamap["appliestoversion"];
            var isOnContainer = (bool)datamap["isoncontainer"];
            var isUpToDate = (bool)datamap["isuptodate"];
            var shouldBeOnContainer = _scriptsService.ShouldBeOnContainer(deploy, version);
            datamap["status"] = _scriptsService.GetStatus(shouldBeOnContainer, isOnContainer, isUpToDate);
        }

    

        public override string ApplicationName() {
            return "_dynamic";
        }
    }
}
