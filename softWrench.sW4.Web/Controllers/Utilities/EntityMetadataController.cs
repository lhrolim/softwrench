using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using System.Linq;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;
using softWrench.sW4.SPF;
using Newtonsoft.Json.Linq;
using System.Data;
using softWrench.sW4.Metadata.Validator;
using System.Collections.Generic;
using System.IO;
using softwrench.sw4.api.classes.email;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Web.Email;
using cts.commons.simpleinjector.app;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Web.Controllers.Utilities {

    [Authorize]
    public class EntityMetadataController : ApiController {
        private readonly SWDBHibernateDAO swdbDao;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly MetadataEmailer metadataEmailer;
        private readonly IConfigurationFacade configurationFacade;
        private readonly IApplicationConfiguration appConfig;

        public EntityMetadataController(SWDBHibernateDAO dao, IEventDispatcher eventDispatcher, MetadataEmailer metadataEmailer, IConfigurationFacade configurationFacade, IApplicationConfiguration appConfig) {
            swdbDao = dao;
            _eventDispatcher = eventDispatcher;
            this.metadataEmailer = metadataEmailer;
            this.configurationFacade = configurationFacade;
            this.appConfig = appConfig;
        }
        
        [HttpGet]
        [SPFRedirect("Metadata Builder", "_headermenu.metadatabuilder", "EntityMetadataBuilder")]
        public RedirectResponseResult Builder() {
            return new RedirectResponseResult();
        }
        
        [HttpGet]
        public IGenericResponseResult Refresh() {
            var user = SecurityFacade.CurrentUser();
            if (ApplicationConfiguration.IsDev() || user.IsSwAdmin()) {
                MetadataProvider.StubReset();
                _eventDispatcher.Dispatch(new ClearCacheEvent());
            }
            return new BlankApplicationResponse();
        }


        [HttpGet]
        [SPFRedirect("Metadata Editor", "_headermenu.metadataeditor", "EntityMetadataEditor")]
        public IGenericResponseResult Editor() {
            using (var reader = new MetadataProvider().GetStream(MetadataProvider.METADATA_FILE)) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, MetadataProvider.METADATA_FILE));
            }
        }

        [HttpGet]
        public IGenericResponseResult ReadOriginalBackup(string file) { 
            using (var reader = new MetadataProvider().GetStream(string.Format("{0}.orig",file))) {
                var result = reader != null ? reader.ReadToEnd() : string.Empty;
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, file));
            }
        }        

        [HttpGet]
        [SPFRedirect("Metadata Editor", "_headermenu.metadataeditor", "EntityMetadataEditor")]
        public IGenericResponseResult PropertiesFileEditor() {
            using (var reader = new MetadataProvider().GetStream(MetadataProvider.PROPERTIES_FILE)) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, MetadataProvider.PROPERTIES_FILE));
            }
        }

        [HttpGet]
        public IGenericResponseResult GetFileContent(string path) {
            using (var reader = new MetadataProvider().GetTemplateStream(path)) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, Path.GetFileName(path)));
            }
        }

        [HttpGet]
        public List<dynamic> GetTemplateFiles(string type) {            
            var templates = new List<dynamic>();

            switch (type) {
                case MetadataProvider.METADATA_FILE:
                    //add the metadata
                    var metadataPath = MetadataParsingUtils.GetPath(MetadataProvider.METADATA_FILE);
                    templates.Add(new { path = metadataPath, name = MetadataProvider.METADATA_FILE });

                    //add the templates
                    var files = MetadataParsingUtils.GetTemplateFileNames();
                    foreach (var file in files) {
                        templates.Add(new { path = file, name = Path.GetFileName(file) });
                    }
                    break;

                case MetadataProvider.STATUS_COLOR_FILE:
                    templates.Add(new { path = MetadataParsingUtils.GetPath(MetadataProvider.STATUS_COLOR_FILE), name = "status colors" });

                    //Add fallback files if the user has the dynamic admin role.
                    if (SecurityFacade.CurrentUser().IsInRolInternal(Role.DynamicAdmin, false)) {
                        var fallbackPathPattern = "{0}App_Data\\Client\\@internal\\fallback\\{1}";
                        templates.Add(new { path = String.Format(fallbackPathPattern, AppDomain.CurrentDomain.BaseDirectory, "statuscolors.json"), name = "status colors fallback file" });
                        templates.Add(new { path = String.Format(fallbackPathPattern, AppDomain.CurrentDomain.BaseDirectory, "statuscolorvalues.json"), name = "status color values" });
                    }
                    break;
            }
            
            return templates;
        }

        [HttpGet]
        public IGenericResponseResult RestoreDefaultMetadata() {
            var resultData = swdbDao.FindByQuery<Metadataeditor>(Metadataeditor.ByDefaultId);
            var metadata = (from c in resultData select c.SystemStringValue).FirstOrDefault();
            return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(metadata, MetadataProvider.METADATA_FILE));

        }

        [HttpGet]
        public DataTable RestoreSavedMetadata(string metadataFileName) {
            var query = string.Format(Metadataeditor.ByFileName, metadataFileName);

            var resultData = swdbDao.FindByQuery<Metadataeditor>(query);

            var result = new DataTable();
            result.Columns.Add("Id", typeof(Int32));
            result.Columns.Add("CreatedDate", typeof(DateTime));
            result.Columns.Add("Description", typeof(string));
            result.Columns.Add("Metadata", typeof(string));

            foreach (Metadataeditor i in resultData) {
                var id = i.Id;
                var metadata = i.SystemStringValue;
                var comments = i.Comments;
                var createdDate = i.CreatedDate;
                result.Rows.Add(id, createdDate, comments, metadata);
            }
            return result;
        }

        [HttpGet]
        [SPFRedirect("Classification Color Editor", "_headermenu.classificationcoloreditor", "EntityMetadataEditor")]
        public IGenericResponseResult ClassificationColorEditor()
        {
            using (var reader = new MetadataProvider().GetStream(MetadataProvider.CLASSIFICATION_COLOR_FILE))
            {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, MetadataProvider.CLASSIFICATION_COLOR_FILE));
            }
        }
        
        [HttpGet]
        [SPFRedirect("Status Color Editor", "_headermenu.statuscoloreditor", "EntityMetadataEditor")]
        public IGenericResponseResult StatusColorEditor() {
            using (var reader = new MetadataProvider().GetStream(MetadataProvider.STATUS_COLOR_FILE)) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, MetadataProvider.STATUS_COLOR_FILE));
            }
        }

        [HttpGet]
        [SPFRedirect("Menu Editor", "_headermenu.menueditor", "EntityMetadataEditor")]
        public IGenericResponseResult MenuEditor() {
            using (var reader = new MetadataProvider().GetStream("menu.web.xml")) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, MetadataProvider.MENU_WEB_FILE));
            }
        }

        [HttpPut]
        public void SaveMetadata(HttpRequestMessage request) {
            var task = request
            .Content
            .ReadAsStreamAsync();

            task.Wait();
            new MetadataProvider().Save(task.Result);
        }
        
        [HttpPut]
        public void SaveMetadataEditor(HttpRequestMessage request) {
            var content = request.Content;
            var json = JObject.Parse(content.ReadAsStringAsync().Result);
            var comments = json.StringValue("Comments");
            var metadata = json.StringValue("Metadata");
            var userFullName = json.StringValue("UserFullName");
            var filePath = json.StringValue("Path");
            var fileName = json.StringValue("Name");
            var ipAddress = request.GetIPAddress();
            var newFileContent = metadata;
            var oldFileContent = File.ReadAllText(filePath);

            var now = DateTime.Now;
            var newMetadataEntry = new Metadataeditor() {
                SystemStringValue = metadata,
                Comments = comments,
                CreatedDate = now,
                DefaultId = 0,
                ChangedBy = SecurityFacade.CurrentUser().MaximoPersonId,
                ChangedByFullName = userFullName,
                Name = fileName,
                Path = filePath,
                IPAddress = ipAddress
            };
            
            swdbDao.Save(newMetadataEntry);
            
            new MetadataProvider().Save(metadata, false, filePath);
            
            this.SendMetadataChangeEmail(
                MetadataProvider.STATUS_COLOR_FILE,
                newFileContent,
                oldFileContent,
                string.Format("[softWrench {0} - {1}] Metadata file updated", appConfig.GetClientKey(), ApplicationConfiguration.Profile),
                comments,
                ipAddress,
                userFullName
            );
        }


        [HttpPut]
        public void SavePropertiesFile(HttpRequestMessage request) {
            var content = request.Content;
            var json = JObject.Parse(content.ReadAsStringAsync().Result);
            var comments = json.StringValue("Comments");
            var metadata = json.StringValue("Metadata");
            var userFullName = json.StringValue("UserFullName");
            var filePath = MetadataParsingUtils.GetPath(MetadataProvider.PROPERTIES_FILE);
            var fileName = MetadataProvider.PROPERTIES_FILE;
            var ipAddress = request.GetIPAddress();
            var newFileContent = metadata;
            var oldFileContent = File.ReadAllText(filePath);

            var now = DateTime.Now;
            var newMetadataEntry = new Metadataeditor() {
                SystemStringValue = metadata,
                Comments = comments,
                CreatedDate = now,
                DefaultId = 0,
                ChangedBy = SecurityFacade.CurrentUser().MaximoPersonId,
                ChangedByFullName = userFullName,
                Name = fileName,
                Path = filePath,
                IPAddress = ipAddress
            };

            swdbDao.Save(newMetadataEntry);

            new MetadataProvider().SavePropertiesFile(metadata, false);

            this.SendMetadataChangeEmail(
                MetadataProvider.PROPERTIES_FILE,
                newFileContent,
                oldFileContent,
                string.Format("[softWrench {0} - {1}] Properties file updated", appConfig.GetClientKey(), ApplicationConfiguration.Profile),
                comments,
                ipAddress,
                userFullName
            );
        }

        [HttpPut]
        public void SaveStatuscolor(HttpRequestMessage request) {
            var content = request.Content;
            var json = JObject.Parse(content.ReadAsStringAsync().Result);
            var comments = json.StringValue("Comments");
            var metadata = json.StringValue("Metadata");
            var userFullName = json.StringValue("UserFullName");
            var ipAddress = request.GetIPAddress();
            var filePath = json.StringValue("Path");
            var fileName = json.StringValue("Name");
            var newFileContent = metadata;
            var oldFileContent = File.ReadAllText(filePath);

            var newMetadataEntry = new Metadataeditor() {
                SystemStringValue = metadata,
                Comments = comments,
                CreatedDate = DateTime.UtcNow,
                DefaultId = 0,
                ChangedBy = SecurityFacade.CurrentUser().MaximoPersonId,
                ChangedByFullName = userFullName,
                Name = fileName,
                Path = filePath,
                IPAddress = ipAddress
            };            

            swdbDao.Save(newMetadataEntry);

            new MetadataProvider().SaveColor(metadata, filePath);
            
            this.SendMetadataChangeEmail(
                MetadataProvider.STATUS_COLOR_FILE,
                newFileContent,
                oldFileContent,
                string.Format("[softWrench {0} - {1}] Status Color file updated", appConfig.GetClientKey(), ApplicationConfiguration.Profile),
                comments,
                ipAddress,
                userFullName
            );

            Refresh();
        }

        [HttpPut]
        public void SaveClassificationcolor(HttpRequestMessage request) {
            var content = request.Content;
            var json = JObject.Parse(content.ReadAsStringAsync().Result);
            var comments = json.StringValue("Comments");
            var metadata = json.StringValue("Metadata");
            var userFullName = json.StringValue("UserFullName");
            var ipAddress = request.GetIPAddress();
            var filePath = MetadataParsingUtils.GetPath(MetadataProvider.CLASSIFICATION_COLOR_FILE);
            var newFileContent = metadata;
            var oldFileContent = File.ReadAllText(filePath);

            var newMetadataEntry = new Metadataeditor() {
                SystemStringValue = metadata,
                Comments = comments,
                CreatedDate = DateTime.UtcNow,
                DefaultId = 0,
                ChangedBy = SecurityFacade.CurrentUser().MaximoPersonId,
                ChangedByFullName = userFullName,
                Name = MetadataProvider.CLASSIFICATION_COLOR_FILE,
                Path = filePath,
                IPAddress = ipAddress
            };

            swdbDao.Save(newMetadataEntry);

            new MetadataProvider().SaveColor(metadata, filePath);
            
            this.SendMetadataChangeEmail(
                MetadataProvider.CLASSIFICATION_COLOR_FILE,
                newFileContent,
                oldFileContent,
                string.Format("[softWrench {0} - {1}] Classification Color file updated", appConfig.GetClientKey(), ApplicationConfiguration.Profile),
                comments,
                ipAddress,
                userFullName
            );

            Refresh();
        }

        [HttpPut]
        public void SaveMenu(HttpRequestMessage request) {
            var content = request.Content;
            var json = JObject.Parse(content.ReadAsStringAsync().Result);
            var comments = json.StringValue("Comments");
            var metadata = json.StringValue("Metadata");
            var userFullName = json.StringValue("UserFullName");
            var fileName = MetadataProvider.MENU_WEB_FILE;
            var filePath = MetadataParsingUtils.GetPath(fileName);
            var ipAddress = request.GetIPAddress();
            var newFileContent = metadata;
            var oldFileContent = File.ReadAllText(filePath);
            
            var newMetadataEntry = new Metadataeditor() {
                SystemStringValue = metadata,
                Comments = comments,
                CreatedDate = DateTime.UtcNow,
                DefaultId = 0,
                ChangedBy = SecurityFacade.CurrentUser().MaximoPersonId,
                ChangedByFullName = userFullName,
                Name = fileName,
                Path = filePath,
                IPAddress = ipAddress
            };

            swdbDao.Save(newMetadataEntry);
            new MetadataProvider().SaveMenu(metadata);

            this.SendMetadataChangeEmail(
                fileName,
                newFileContent,
                oldFileContent,
                string.Format("[softWrench {0} - {1}] Menu file updated", appConfig.GetClientKey(), ApplicationConfiguration.Profile),
                comments,
                ipAddress,
                userFullName
            );
        }

        [HttpGet]
        public MetadataResult Build(string tablename) {
            if (tablename == null) {
                throw new InvalidOperationException("table Name should be informed");
            }
            var xml = new MetadataBuilderUtil().GenerateEntityMetadata(tablename);
            if (xml == null) {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            }
            return new MetadataResult(FormatXml(xml), null);
        }


        private void SendMetadataChangeEmail(string fileName, string newFileContent, string oldFileContent, string subject, string comment, string ipAddress, string userName) {
            var sendTo = configurationFacade.Lookup<string>(ConfigurationConstants.MetadataChangeReportEmailId);

            if (!string.IsNullOrWhiteSpace(sendTo)) {

                var emailData = new MetadataChangeEmail() {
                    Customer = appConfig.GetClientKey(),
                    IPAddress = ipAddress,
                    Comment = comment,
                    ChangedByFullName = userName,
                    CurrentUser = SecurityFacade.CurrentUser().DBUser,
                    ChangedOnUTC = DateTime.UtcNow,
                    MetadataName = fileName,
                    NewFileContent = newFileContent,
                    OldFileContent = oldFileContent,
                    SendTo = sendTo,
                    Subject = subject
                };

                //metadataEmailer.SendMetadataChangeEmail(emailData);
            }
        }

        private string FormatXml(String xml) {
            try {
                var doc = XDocument.Parse(xml);
                return doc.ToString();
            } catch (Exception) {
                return xml;
            }
        }

        public class MetadataResult {
            private readonly string _metadata;

            public MetadataResult(string metadata, string error) {
                _metadata = metadata;
            }

            public string Metadata {
                get { return _metadata; }
            }
        }
    }
}