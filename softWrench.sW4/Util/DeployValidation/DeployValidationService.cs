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
using softWrench.sW4.Data.API.Composition;

namespace softWrench.sW4.Util.DeployValidation {
    public class DeployValidationService : ISingletonComponent {
        private const string DeploymentTestDataPath = "\\App_Data\\DeploymentValidation\\Data\\{0}";
        private IEnumerable<CompleteApplicationMetadataDefinition> applicationsMetadataList;
        private readonly DataSetProvider dataSetProvider;
        private static string[] testFileNames;
        private static readonly List<string> GlobalMissingPropertyBlackList = new List<string>() {
            "rowstamp" ,
            "class",
            "actlabhrs",
            "actlabcost",
            "reportdate",
            "actualstart",
            "estapprlabhrs",
            "estapprmatcost",
            "estapprlabcost",
            "outservcost",
            "langcode",
            "orgid",
            "siteid",
            "loweremail"
        };
        
        private static string[] GetTestFileNames {
            get {
                if (testFileNames == null) {
                    testFileNames = Directory.GetFiles(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, string.Format(DeploymentTestDataPath, string.Empty)));
                }

                return testFileNames;
            }
        }

        public DeployValidationService(DataSetProvider dataSetProvider) {
            this.dataSetProvider = dataSetProvider;           
        }
        
        public Dictionary<string, DeployValidationResult> ValidateServices(InMemoryUser user) {
            var applicationValidationModels = SetupApplicationValidationModels();

            DoMockProxyInvocation();
            
            var resultmap = new Dictionary<string, DeployValidationResult>();

            var applications = GetApplications.Select(x => x.ApplicationName);

            foreach(var application in applications) {
                var appValidationModels = applicationValidationModels.FindAll(x => string.Compare(x.Application, application, true) == 0);

                foreach (var app in appValidationModels) {
                    ValidateOperation(user, app, resultmap);
                }
            }

            return resultmap;
        }
               
        public Dictionary<string, Dictionary<string, object>> GetAllApplicationInfo() {
            var dict = new Dictionary<string, Dictionary<string, object>>();
            GetApplications.ForEach(a => dict.Add(a.ApplicationName, GetAppInfo(a)));

            return dict;
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
        
        private IEnumerable<CompleteApplicationMetadataDefinition> GetApplications {
            get {
                if (applicationsMetadataList == null) {
                    applicationsMetadataList = MetadataProvider.Applications(false)
                   .Where(FilterApplication)
                   .OrderBy(a => a.Title);
                }

                return applicationsMetadataList;
            }
        }

        private void ValidateOperation(InMemoryUser user, ApplicationValidationModel appValidationModel, IDictionary<string, DeployValidationResult> resultmap) {
            InsertModel();

            var application = appValidationModel.Application;      
            var json = appValidationModel.TestJson;
            var model = GetModel();

            try {
                model.ActionName = appValidationModel.Action;
                model.ActionDescription = appValidationModel.ActionDescription;
                model.ActionSupported = appValidationModel.ActionSupported;

                if (appValidationModel.ActionSupported) {
                    if (json == null) {
                        
                    } else {
                        model.ActionSupported = appValidationModel.ActionSupported;
                        var platform = ClientPlatform.Web;

                        OperationDataRequest operationDataRequest = null;

                        switch (appValidationModel.Action) {
                            case OperationConstants.CRUD_CREATE:
                                operationDataRequest = GetCreateOperationDataRequest(appValidationModel);
                                break;

                            case OperationConstants.CRUD_UPDATE:
                                operationDataRequest = GetUpdateOperationDataRequest(appValidationModel);
                                break;

                                /// ToDo: add more operations here.
                        }

                        var currentschemaKey = SchemaUtil.GetSchemaKeyFromString(operationDataRequest.CurrentSchemaKey, platform);
                        var applicationMetadata = MetadataProvider.Application(application).ApplyPolicies(currentschemaKey, user, platform);

                        dataSetProvider.LookupDataSet(application, applicationMetadata.Schema.SchemaId)
                        .Execute(applicationMetadata, json, operationDataRequest);
                    }
                }
            } catch (Exception ex) {
                var wsdlEx = ex as MaximoWebServiceNotResolvedException;
                if (wsdlEx != null) {
                    model.MissingWsdl = true;
                }
                model.ReportException(ex);
            }

            var result = GetOrCreateResult(application, resultmap);
            model.CalcHasProblems();
            result.ValidationResultList.Add(model);
        }

        private OperationDataRequest GetCreateOperationDataRequest(ApplicationValidationModel appValidationModel) {
            var schemaRepresentation = MetadataProvider.LocateNewSchema(appValidationModel.Application);

            var schemaKey = !string.IsNullOrWhiteSpace(appValidationModel.SchemaKey) ? 
                appValidationModel.SchemaKey : ((schemaRepresentation != null && !string.IsNullOrWhiteSpace(schemaRepresentation.SchemaId)) ? schemaRepresentation.SchemaId : "newdetail.input.web");

            var operationDataRequest = new OperationDataRequest() {
                ApplicationName = appValidationModel.Application,
                Batch = false,
                CurrentSchemaKey = schemaKey,
                Id = null,
                MockMaximo = false,
                Operation = OperationConstants.CRUD_CREATE,
                Platform = ClientPlatform.Web,
                UserId = null,
                CompositionData = appValidationModel.CompositionOperationDTO
            };

            return operationDataRequest;
        }

        private OperationDataRequest GetUpdateOperationDataRequest(ApplicationValidationModel appValidationModel) {
            var schemaRepresentation = MetadataProvider.LocateRelatedDetailSchema(appValidationModel.Metadata.GetListSchema());

            var schemaKey = !string.IsNullOrWhiteSpace(appValidationModel.SchemaKey) ?
                appValidationModel.SchemaKey : ((schemaRepresentation != null && !string.IsNullOrWhiteSpace(schemaRepresentation.SchemaId)) ? schemaRepresentation.SchemaId : "editdetail.input.web");

            var operationDataRequest = new OperationDataRequest() {
                ApplicationName = appValidationModel.Application,
                Batch = false,
                CurrentSchemaKey = schemaKey,
                Id = "1",
                MockMaximo = false,
                Operation = OperationConstants.CRUD_UPDATE,
                Platform = ClientPlatform.Web,
                UserId = "1",
                CompositionData = appValidationModel.CompositionOperationDTO
            };

            return operationDataRequest;
        }
                
        private List<ApplicationValidationModel> SetupApplicationValidationModels() {
            var applicationValidationModels = new List<ApplicationValidationModel>();     
            var assembly = Assembly.GetExecutingAssembly();
            var applications = GetApplications.Select(x => x.ApplicationName.ToLower());

           // var testApplicationDataList = GetTestFileNames assembly.GetManifestResourceNames();
            foreach (var filePath in GetTestFileNames) {
                if (File.Exists(filePath)) {
                    using (var stream = new StreamReader(filePath)) {// assembly.GetManifestResourceStream(filePath)) {
                        if (stream != null) {
                            var app = GetApplicationValidationModels(stream.ReadToEnd());

                            if (app != null) {
                                applicationValidationModels.AddRange(app);
                            }
                        }
                    }
                }
            }

            return applicationValidationModels;
        }

        private List<ApplicationValidationModel> GetApplicationValidationModels(string json) {
            var models = new List<ApplicationValidationModel>();
            var applicationDataAray = JArray.Parse(json);

            if (applicationDataAray != null) {
                for (int a = 0; a < applicationDataAray.Count; a++) {
                    var jsonData = applicationDataAray[a];
                    var mainData = JObject.Parse(jsonData["data"].ToString());
                    var subDataArray = jsonData["subdata"] != null ? JArray.Parse(jsonData["subdata"].ToString()) : null;
                    var compData = jsonData["compositiondata"] != null ? jsonData["compositiondata"] : null;
                    var application = jsonData["application"].ToString();
                    var action = jsonData["action"].ToString();
                    var schemaKey = jsonData["schemakey"] != null ? jsonData["schemakey"].ToString() : string.Empty;   
                    var applicationMetadata = MetadataProvider.Application(application, false);

                    if (applicationMetadata == null || (action.Equals(OperationConstants.CRUD_CREATE) && !applicationMetadata.HasCreationSchema)) {
                        models.Add(new ApplicationValidationModel() {
                            Application = application,
                            Action = action,
                            ActionDescription = jsonData["actiondescription"].ToString(),
                            ActionSupported = false,
                            TestJson = null,
                            Metadata = null,
                            IsMissingTestData = true
                        });
                    } else {
                        var applicationValidationModel = new ApplicationValidationModel();
                        applicationValidationModel.Application = application;
                        applicationValidationModel.Action = action;
                        applicationValidationModel.ActionSupported = true;
                        applicationValidationModel.ActionDescription = jsonData["actiondescription"].ToString();
                        applicationValidationModel.Metadata = applicationMetadata;
                        applicationValidationModel.SchemaKey = schemaKey;
                        applicationValidationModel.IsMissingTestData = false;

                        if (subDataArray != null) {
                            for (int i = 1; i < subDataArray.Count; i++) {
                                if (subDataArray[0]["addto"].ToString().Equals("main")) {
                                    if (subDataArray[i]["type"].ToString().Equals("array")) {
                                        mainData.Add(subDataArray[i]["property"].ToString(), JArray.Parse(string.Format("{0}", subDataArray[i]["data"].ToString())));
                                    } else {
                                        mainData.Add(subDataArray[i]["property"].ToString(), subDataArray[i]["data"].ToString());
                                    }
                                } else {
                                    var propertyData = mainData.Property(subDataArray[i]["addto"].ToString());
                                    if (subDataArray[i]["type"].ToString().Equals("array")) {
                                        propertyData.Add(JArray.Parse(string.Format("{0}", subDataArray[i]["data"].ToString())));
                                    } else {
                                        propertyData.Add(subDataArray[i]["data"].ToString());
                                    }
                                }
                            }
                        }

                        if (compData != null) {
                            var compositionData = new CompositionOperationDTO();
                            compositionData.DispatcherComposition = compData["dispatchercomposition"].ToString();
                            compositionData.Id = compData["id"].ToString();
                            compositionData.Operation = compData["operation"].ToString();
                            compositionData.CompositionItem = JObject.Parse(string.Format("{0}", compData["data"].ToString()));

                            applicationValidationModel.CompositionOperationDTO = compositionData;
                        }
                        
                        applicationValidationModel.TestJson = mainData;
                        models.Add(applicationValidationModel);
                    }
                }
            }
            
            return models;
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
            if (!GetTestFileNames.Contains(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, string.Format(DeploymentTestDataPath, string.Format("{0}.json", application.ApplicationName))))) {
                return false;
            }

            var entity = MetadataProvider.Entity(application.Entity);
            return entity.ConnectorParameters.Parameters.ContainsKey("integration_interface") 
                || entity.ConnectorParameters.Parameters.ContainsKey("integration_interface_operations");
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
