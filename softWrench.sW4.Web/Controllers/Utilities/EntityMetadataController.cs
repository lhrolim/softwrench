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

namespace softWrench.sW4.Web.Controllers.Utilities {

    [Authorize]
    public class EntityMetadataController : ApiController {
        private readonly SWDBHibernateDAO _swdbDao;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly MetadataEmailer metadataEmailer;
        private readonly IConfigurationFacade configurationFacade;

        public EntityMetadataController(SWDBHibernateDAO dao, IEventDispatcher eventDispatcher, MetadataEmailer metadataEmailer, IConfigurationFacade configurationFacade) {
            _swdbDao = dao;
            _eventDispatcher = eventDispatcher;
            this.metadataEmailer = metadataEmailer;
            this.configurationFacade = configurationFacade;
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
            using (var reader = new MetadataProvider().GetStream("metadata.xml")) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, "metadata"));
            }
        }

        [HttpGet]
        public IGenericResponseResult GetMetadataContent(string templatePath) {
            using (var reader = new MetadataProvider().GetTemplateStream(templatePath)) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, "metadata"));
            }
        }

        [HttpGet]
        public List<dynamic> GetTemplateFiles() {            
            var templates = new List<dynamic>();

            //add the metadata
            var metadataPath = MetadataParsingUtils.GetPath("metadata.xml");
            templates.Add(new { path = metadataPath, name = "metadata.xml" });

            //add the templates
            var files = MetadataParsingUtils.GetTemplateFileNames();
            foreach (var file in files) {
                templates.Add(new { path = file, name = Path.GetFileName(file) });
            }

            return templates;
        }

        [HttpGet]
        public IGenericResponseResult RestoreDefaultMetadata() {
            var resultData = _swdbDao.FindByQuery<Metadataeditor>(Metadataeditor.ByDefaultId);
            var metadata = (from c in resultData select c.SystemStringValue).FirstOrDefault();
            return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(metadata, "metadata"));

        }

        [HttpGet]
        public DataTable RestoreSavedMetadata(string metadataFileName) {
            var query = string.Format(Metadataeditor.ByFileName, metadataFileName);

            var resultData = _swdbDao.FindByQuery<Metadataeditor>(query);

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
            using (var reader = new MetadataProvider().GetStream("classificationcolors.json"))
            {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, "classificationcolors"));
            }
        }

        [HttpGet]
        [SPFRedirect("Status Color Editor", "_headermenu.statuscoloreditor", "EntityMetadataEditor")]
        public IGenericResponseResult StatusColorEditor() {
            using (var reader = new MetadataProvider().GetStream("statuscolors.json")) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, "statuscolors"));
            }
        }

        [HttpGet]
        [SPFRedirect("Menu Editor", "_headermenu.menueditor", "EntityMetadataEditor")]
        public IGenericResponseResult MenuEditor() {
            using (var reader = new MetadataProvider().GetStream("menu.web.xml")) {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, "menu"));
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
        
        [HttpPost]
        public void SaveMetadataEditor(HttpRequestMessage request) {
            var content = request.Content;
            var json = JObject.Parse(content.ReadAsStringAsync().Result);
            var comments = json.StringValue("Comments");
            var metadata = json.StringValue("Metadata");
            var path = json.StringValue("Path");
            var name = json.StringValue("Name");

            var now = DateTime.Now;
            var newMetadataEntry = new Metadataeditor() {
                SystemStringValue = metadata,
                Comments = comments,
                CreatedDate = now,
                DefaultId = 0,
                ChangedBy = SecurityFacade.CurrentUser().MaximoPersonId,
                Name = name,
                Path = path,
                BaselineVersion = ""
            };

            var newFileContent = newMetadataEntry.SystemStringValue;
            var oldFileContent = File.ReadAllText(newMetadataEntry.Path);

            _swdbDao.Save(newMetadataEntry);
            new MetadataProvider().Save(metadata, false, path);

            var sendTo = configurationFacade.Lookup<string>(ConfigurationConstants.MetadataChangeReportEmailId);

            if (!string.IsNullOrWhiteSpace(sendTo)) {
                metadataEmailer.SendMetadataChangeEmail(newMetadataEntry.Name,
                newFileContent,
                oldFileContent,
                newMetadataEntry.Comments,
                SecurityFacade.CurrentUser().DBUser,
                DateTime.Now,
                sendTo);
            }
        }

        [HttpPut]
        public void SaveStatuscolor(HttpRequestMessage request) {
            var task = request
            .Content
            .ReadAsStreamAsync();

            task.Wait();
            new MetadataProvider().SaveColor(task.Result);
            Refresh();
        }

        [HttpPut]
        public void SaveMenu(HttpRequestMessage request) {
            var task = request
            .Content
            .ReadAsStreamAsync();
            task.Wait();
            new MetadataProvider().SaveMenu(task.Result);
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

        string FormatXml(String xml) {
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