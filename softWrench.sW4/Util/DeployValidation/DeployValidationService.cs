using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
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
using NHibernate.Util;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.API.Composition;
using cts.commons.Util;


namespace softWrench.sW4.Util.DeployValidation {
    public class DeployValidationService : ISingletonComponent {
        private const string DeploymentTestDataPath = "\\App_Data\\DeploymentValidation\\Data\\{0}.json";
        private IEnumerable<CompleteApplicationMetadataDefinition> applicationsMetadataList;
        private readonly DataSetProvider dataSetProvider;
        private static Dictionary<string, string> testFileNames;
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
            "loweremail",
            "allkeywords"
        };
        

        private static Dictionary<string, string> GetTestFileNames {
            get {
                if (testFileNames == null) {
                    testFileNames = new Dictionary<string, string>();
                    var topLevelApps = MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, null);
                    foreach (var app in topLevelApps) {
                        var filePath = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, string.Format(DeploymentTestDataPath, app.ApplicationName));
                        testFileNames.Add(app.ApplicationName, filePath);                       
                    }

                    //ToDo : remove hardcoded applications. 
                    // Fetch these along with the top level apps. 
                    testFileNames.Add("person", string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, string.Format(DeploymentTestDataPath, "person")));
                }

                return testFileNames;
            }
        }

        public DeployValidationService(DataSetProvider dataSetProvider) {
            this.dataSetProvider = dataSetProvider;           
        }

        public List<DirectoryAndFileValidationModel> ValidateFilesDirectories(InMemoryUser user) {

            var models = new List<DirectoryAndFileValidationModel>();

            using (var stream = new StreamReader(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "\\App_Data\\DeploymentValidation\\FilesAndFolders.json"))) {
                if (stream != null) {
                    var filesDataAray = JArray.Parse(stream.ReadToEnd());

                    for (int a = 0; a < filesDataAray.Count; a++) {
                        var jsonData = filesDataAray[a];

                        if(jsonData["type"].ToString().Equals("FOLDER", StringComparison.OrdinalIgnoreCase)) {
                            var directoryValidation = new DirectoryAndFileValidationModel() { Validations = new List<DirectoryAndFileValidation>() };

                            directoryValidation.Name = jsonData["folderName"].ToString();
                            directoryValidation.Path = jsonData["folderPath"].ToString();

                            if(directoryValidation.Path.Equals("NOT_DEFINED", StringComparison.OrdinalIgnoreCase)) {
                                switch (directoryValidation.Name) {
                                    case "App_Data":
                                        directoryValidation.Path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, directoryValidation.Name);
                                        break;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(jsonData["shouldExist"].ToString()) && Convert.ToBoolean(jsonData["shouldExist"])){
                                var validation = Directory.Exists(directoryValidation.Path) ? new DirectoryAndFileValidation() {
                                    ValidationSuccess = true,
                                    ValidationMessage = "The directory exists"
                                } : new DirectoryAndFileValidation() {
                                    ValidationSuccess = false,
                                    ValidationMessage = "The directory does not exist"
                                };

                                directoryValidation.Validations.Add(validation);
                            }

                            if (!string.IsNullOrWhiteSpace(jsonData["shouldHaveWriteAccess"].ToString()) && Convert.ToBoolean(jsonData["shouldHaveWriteAccess"])) {
                                var validation = DirectoryUtil.HasWriteAccessToFolder(directoryValidation.Path) ? new DirectoryAndFileValidation() {
                                    ValidationSuccess = true,
                                    ValidationMessage = "The directory has write access"
                                } : new DirectoryAndFileValidation() {
                                    ValidationSuccess = false,
                                    ValidationMessage = "The directory has no write access"
                                };

                                directoryValidation.Validations.Add(validation);
                            }

                            models.Add(directoryValidation);
                        }
                    }
                }
            }

            return models;
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

                if (json == null || appValidationModel.IsMissingTestData) {
                    model.MissingTestData = true;
                } else if (appValidationModel.ActionSupported) {
                    model.MissingTestData = false;
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
                if (File.Exists(filePath.Value)) {
                    using (var stream = new StreamReader(filePath.Value)) {// assembly.GetManifestResourceStream(filePath)) {
                        if (stream != null) {
                            var app = GetApplicationValidationModels(stream.ReadToEnd());

                            if (app != null) {
                                applicationValidationModels.AddRange(app);
                            }
                        }
                    }
                } else {
                    applicationValidationModels.Add(new ApplicationValidationModel() {
                        Action = "Unknown",
                        ActionDescription = "Test data is missing",
                        ActionSupported = false,
                        Application = filePath.Key,
                        IsMissingTestData = true,
                        TestJson = null,
                        CompositionOperationDTO = null,
                        Metadata = null,
                        SchemaKey = null
                    });
                }
            }

            return applicationValidationModels;
        }

        private List<ApplicationValidationModel> GetApplicationValidationModels(string json) {            
            var applicationDataAray = JArray.Parse(json);

            if (applicationDataAray == null) {
                return null;
            }

            var models = new List<ApplicationValidationModel>();

            for (int a = 0; a < applicationDataAray.Count; a++) {
                var model = applicationDataAray[a] != null ? CreateApplicationValidationModel(applicationDataAray[a]) : null;

                if (model != null) {
                    models.Add(model);
                }
            }
            
            return models;
        }

        private ApplicationValidationModel CreateApplicationValidationModel(JToken jsonData) {    
            var application = jsonData["application"].ToString();
            var action = jsonData["action"].ToString();
            var applicationMetadata = MetadataProvider.Application(application, false);

            if (applicationMetadata == null || (action.Equals(OperationConstants.CRUD_CREATE) && !applicationMetadata.HasCreationSchema)) {
                return new ApplicationValidationModel() {
                    Application = application,
                    Action = action,
                    ActionDescription = jsonData["actiondescription"].ToString(),
                    ActionSupported = false,
                    TestJson = null,
                    Metadata = null,
                    IsMissingTestData = true
                };
            } else {
                var mainData = JObject.Parse(jsonData["data"].ToString());
                var applicationValidationModel = new ApplicationValidationModel();
                applicationValidationModel.Application = application;
                applicationValidationModel.Action = action;
                applicationValidationModel.ActionSupported = true;
                applicationValidationModel.ActionDescription = jsonData["actiondescription"].ToString();
                applicationValidationModel.Metadata = applicationMetadata;
                applicationValidationModel.SchemaKey = jsonData["schemakey"] != null ? jsonData["schemakey"].ToString() : string.Empty; ;
                applicationValidationModel.IsMissingTestData = false;

                var compData = jsonData["compositiondata"] != null ? jsonData["compositiondata"] : null;
                if (compData != null) {
                    var compositionData = new CompositionOperationDTO();
                    compositionData.DispatcherComposition = compData["dispatchercomposition"].ToString();
                    compositionData.Id = compData["id"].ToString();
                    compositionData.Operation = compData["operation"].ToString();
                    compositionData.CompositionItem = JObject.Parse(string.Format("{0}", compData["data"].ToString()));

                    applicationValidationModel.CompositionOperationDTO = compositionData;
                }

                applicationValidationModel.TestJson = mainData;
                return applicationValidationModel;
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
            if (!GetTestFileNames.Values.Contains(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, string.Format(DeploymentTestDataPath, application.ApplicationName)))) {
                return false;
            }

            var entity = MetadataProvider.Entity(application.Entity);
            return entity.ConnectorParameters.Parameters.ContainsKey("integration_interface") 
                || entity.ConnectorParameters.Parameters.ContainsKey("integration_interface_operations");
        }

        private static Dictionary<string, object> GetAppInfo(CompleteApplicationMetadataDefinition application) {
            MetadataProvider.FetchNonInternalSchemas(ClientPlatform.Web, application.ApplicationName);
            var hasCreationSchema = MetadataProvider.Application(application.ApplicationName).HasCreationSchema;

            return new Dictionary<string, object>() {
                { "applicationName", application.ApplicationName},
                { "title", application.Title},
                { "hasCreationSchema", hasCreationSchema}
            };
        }
    }
}
