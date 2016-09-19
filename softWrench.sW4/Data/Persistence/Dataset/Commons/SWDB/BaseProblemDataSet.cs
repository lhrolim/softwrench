using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using softwrench.sw4.problem.classes;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB {
    class BaseProblemDataSet : SWDBApplicationDataset {

        private readonly ProblemHandlerLookuper _handlerLookuper;
        private readonly DataSetProvider _dataSetProvider;

        public BaseProblemDataSet(ProblemHandlerLookuper handlerLookuper, DataSetProvider dataSetProvider) {
            _handlerLookuper = handlerLookuper;
            _dataSetProvider = dataSetProvider;
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);

            var resultObject = result.ResultObject;
            var applicationName = resultObject.GetStringAttribute("recordtype");
            var handlerType = resultObject.GetStringAttribute("problemtype");
            var recordId = resultObject.GetStringAttribute("recordId");
            var schemaId = resultObject.GetStringAttribute("recordschema");

            var handler = _handlerLookuper.FindHandler(handlerType, applicationName);
            if (handler == null || handler.DelegateToMainApplication()) {
                var ds = _dataSetProvider.LookupDataSet(applicationName, null);
                var schema = schemaId ?? "editdetail";
                var schemaKey = new ApplicationMetadataSchemaKey(schema);
                var app = MetadataProvider.Application(applicationName).ApplyPoliciesWeb(schemaKey);
                //To avoid caching on RedirectUrlFilter

                var applicationDetailResult = await ds.GetApplicationDetail(app, user, new DetailRequest(recordId, schemaKey));
                ModifySchemaInsertingProblemData(applicationDetailResult, app);
                applicationDetailResult.ResultObject.SetAttribute("#problemmessage",resultObject.GetAttribute("message"));
                return applicationDetailResult;
            }

            var dataAttribute = resultObject.GetAttribute("data");
            if (dataAttribute == null) {
                return result;
            }
            var dataAsString = StringExtensions.GetString(CompressionUtil.Decompress((byte[])dataAttribute));
            var deserialized = JsonConvert.DeserializeObject(dataAsString);
            var formatted = JsonConvert.SerializeObject(deserialized, Formatting.Indented);
            resultObject.SetAttribute("#dataasstring", formatted);
            return result;
        }

        private static void ModifySchemaInsertingProblemData(ApplicationDetailResult applicationDetailResult,
            ApplicationMetadata app) {
            var originalDisplayables = new LinkedList<IApplicationDisplayable>(applicationDetailResult.Schema.Displayables);
            originalDisplayables.AddFirst(new ApplicationSection() {
                Resourcepath = "Content\\Shared\\problem\\templates\\problemsection.html",
                ShowExpression = "true",
                Attribute = "#problemmessage"
            });
            applicationDetailResult.Schema.Displayables = new List<IApplicationDisplayable>(originalDisplayables);
            applicationDetailResult.Schema.IgnoreCache = true;
        }

        public override string ApplicationName() {
            return "_SoftwrenchError";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}