using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using NHibernate.Linq;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using System.IO;
using System.Reflection;
using softWrench.sW4.Data.Persistence.Operation;

namespace softWrench.sW4.Util.DeployValidation {
    public class DeployValidationService : ISingletonComponent {

        private readonly DataSetProvider _dataSetProvider;
        private List<ApplicationValidationModel> applicationList = null;
        private static readonly List<string> GlobalMissingPropertyBlackList = new List<string>(){
            "rowstamp" ,
            "class",
            "actlabhrs",
            "actlabcost",
            "reportdate",
            "actualstart",
            "estapprlabhrs",
            "estapprmatcost",
            "estapprlabcost",
            "outservcost"
    };

        public DeployValidationService(DataSetProvider dataSetProvider) {
            _dataSetProvider = dataSetProvider;           
        }
        
        public Dictionary<string, DeployValidationResult> ValidateServices(InMemoryUser user) {
            Setup();

            DoMockProxyInvocation();
            
            var resultmap = new Dictionary<string, DeployValidationResult>();

            foreach (var app in applicationList) //.FindAll(x => x.CreateJson != null))
            {
                ValidateCreate(user, app, resultmap);
            }

            foreach (var app in applicationList) //.FindAll(x => x.UpdateJson != null)) 
            {
                ValidateUpdate(user, app, resultmap);
            }

            return resultmap;
        }

        public Dictionary<string, Dictionary<string, object>> GetAllApplicationInfo() {
            var dict = new Dictionary<string, Dictionary<string, object>>();
            GetApplications().ForEach(a => dict.Add(a.ApplicationName, GetAppInfo(a)));

            return dict;
        }
        
        public IEnumerable<CompleteApplicationMetadataDefinition> GetApplications() { 
            return MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, null)
               .Where(FilterApplication)
               .OrderBy(a => a.Title);
        }

        public static bool MockProxyInvocation() {
            var lds = System.Threading.Thread.GetNamedDataSlot("sw.deployvalidation.wsexecute.mock.proxy");
            var data = System.Threading.Thread.GetData(lds);
            return data as bool? ?? false;
        }

        public static void AddMissingProperty(string property) {
            if (string.IsNullOrEmpty(property) || GlobalMissingPropertyBlackList.Contains(property.ToLower()) || !MockProxyInvocation()) {
                return;
            }
            var model = GetModel();
            model.AddMissingProperty(property.ToLower());
        }

        private void ValidateCreate(InMemoryUser user, ApplicationValidationModel appValidationModel, IDictionary<string, DeployValidationResult> resultmap) {
            InsertModel();

            var application = appValidationModel.Application;      
            var json = appValidationModel.CreateJson;
            var model = GetModel();

            try
            {
                if(json == null)
                {
                    model.MissingTestData = true;
                }
                else
                {
                    model.MissingTestData = false;
                    var platform = ClientPlatform.Web;
                    var schemaRepresentation = MetadataProvider.LocateNewSchema(appValidationModel.Application);
                    var schemaKey = (schemaRepresentation != null && !string.IsNullOrWhiteSpace(schemaRepresentation.SchemaId)) ? schemaRepresentation.SchemaId : "newdetail.input.web";
                    var operationDataRequest = new OperationDataRequest()
                    {
                        ApplicationName = application,
                        Batch = false,
                        CurrentSchemaKey = schemaKey,
                        Id = null,
                        MockMaximo = false,
                        Operation = OperationConstants.CRUD_CREATE,
                        Platform = ClientPlatform.Web,
                        UserId = null
                    };

                    var currentschemaKey = SchemaUtil.GetSchemaKeyFromString(operationDataRequest.CurrentSchemaKey, platform);
                    var applicationMetadata = MetadataProvider.Application(application).ApplyPolicies(currentschemaKey, user, platform);

                    _dataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId)
                    .Execute(applicationMetadata, json, operationDataRequest);
                }
            }
            catch (Exception ex)
            {
                var wsdlEx = ex as MaximoWebServiceNotResolvedException;
                if (wsdlEx != null)
                {
                    model.MissingWsdl = true;
                }
                model.ReportException(ex);
            }

            var result = GetOrCreateResult(application, resultmap);
            model.CalcHasProblems();
            result.CreateValidation = model;
        }

        private void ValidateUpdate(InMemoryUser user, ApplicationValidationModel appValidationModel, IDictionary<string, DeployValidationResult> resultmap) {      
            InsertModel();

            var application = appValidationModel.Application;
            var json = appValidationModel.UpdateJson;
            var model = GetModel();

            try
            {
                if (json == null)
                {
                    model.MissingTestData = true;
                }
                else
                {
                    model.MissingTestData = false;

                    var platform = ClientPlatform.Web;
                    var schemaRepresentation = MetadataProvider.LocateRelatedDetailSchema(appValidationModel.Metadata.GetListSchema());
                    var schemaKey = (schemaRepresentation != null && !string.IsNullOrWhiteSpace(schemaRepresentation.SchemaId)) ? schemaRepresentation.SchemaId : "editdetail.input.web";
                    var operationDataRequest = new OperationDataRequest()
                    {
                        ApplicationName = application,
                        Batch = false,
                        CurrentSchemaKey = schemaKey,
                        Id = "1",
                        MockMaximo = false,
                        Operation = OperationConstants.CRUD_UPDATE,
                        Platform = ClientPlatform.Web,
                        UserId = "1"
                    };

                    var currentschemaKey = SchemaUtil.GetSchemaKeyFromString(operationDataRequest.CurrentSchemaKey, platform);
                    var applicationMetadata = MetadataProvider.Application(application).ApplyPolicies(currentschemaKey, user, platform);

                    _dataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId)
                    .Execute(applicationMetadata, json, operationDataRequest);
                }
            }
            catch (Exception ex)
            {
                var wsdlEx = ex as MaximoWebServiceNotResolvedException;
                if (wsdlEx != null)
                {
                    model.MissingWsdl = true;
                }
                model.ReportException(ex);
            }

            var result = GetOrCreateResult(application, resultmap);
            model.CalcHasProblems();
            result.UpdateValidation = model;
        }
        
        private void Setup() {

            if(applicationList == null)
            {
                applicationList = new List<ApplicationValidationModel>();
            }
            else
            {
                applicationList.Clear();
            }
            
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var applicationMetadata in GetApplications()) {
                var filePath = string.Empty;
                var app = new ApplicationValidationModel();
                app.Application = applicationMetadata.ApplicationName;
                app.Metadata = applicationMetadata;

                //Create json 
                if (applicationMetadata.HasCreationSchema) {
                    filePath = string.Format("softWrench.sW4.Util.DeployValidation.Data.Create.{0}.json", applicationMetadata.ApplicationName);
                    using (Stream stream = assembly.GetManifestResourceStream(filePath)) {
                        if (stream != null) {
                            using (StreamReader reader = new StreamReader(stream)) {
                                app.CreateJson = JObject.Parse(reader.ReadToEnd());
                            }
                        }
                    }
                }

                //Update json
                filePath = string.Format("softWrench.sW4.Util.DeployValidation.Data.Update.{0}.json", applicationMetadata.ApplicationName);
                using (Stream stream = assembly.GetManifestResourceStream(filePath)) {
                    if (stream != null) {
                        using (StreamReader reader = new StreamReader(stream)) {
                            app.UpdateJson = JObject.Parse(reader.ReadToEnd());
                        }
                    }
                }
                
                applicationList.Add(app);
            }            
        }

        private static void DoMockProxyInvocation() {
            var lds = System.Threading.Thread.GetNamedDataSlot("sw.deployvalidation.wsexecute.mock.proxy");
            System.Threading.Thread.SetData(lds, true);
        }

        private static void InsertModel() {
            var lds = System.Threading.Thread.GetNamedDataSlot("sw.deployvalidation.validationmodel");
            System.Threading.Thread.SetData(lds, new DeployValidationModel());
        }

        private static DeployValidationModel GetModel() {
            var lds = System.Threading.Thread.GetNamedDataSlot("sw.deployvalidation.validationmodel");
            return (DeployValidationModel)System.Threading.Thread.GetData(lds);
        }

        private static DeployValidationResult GetOrCreateResult(string application, IDictionary<string, DeployValidationResult> resultmap) {
            if (resultmap.ContainsKey(application)) {
                return resultmap[application];
            }
            var result = new DeployValidationResult();
            resultmap[application] = result;
            return result;
        }

        private static bool FilterApplication(CompleteApplicationMetadataDefinition application) {
            var entity = MetadataProvider.Entity(application.Entity);
            return entity.ConnectorParameters.Parameters.ContainsKey("integration_interface");
        }

        private static Dictionary<string, object> GetAppInfo(CompleteApplicationMetadataDefinition application) {
            MetadataProvider.FetchNonInternalSchemas(ClientPlatform.Web, application.ApplicationName);
            var hasCreationSchema = MetadataProvider.Application(application.ApplicationName).HasCreationSchema;
            return new Dictionary<string, object>()
                {
                    { "applicationName", application.ApplicationName},
                    { "title", application.Title},
                    { "hasCreationSchema", hasCreationSchema}
                };
        }
    }
}
