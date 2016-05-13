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

namespace softWrench.sW4.Web.Controllers.Utilities {

    [Authorize]
    public class EntityMetadataController : ApiController {
        private readonly SWDBHibernateDAO _swdbDao;
        private readonly IEventDispatcher _eventDispatcher;

        public EntityMetadataController(SWDBHibernateDAO dao, IEventDispatcher eventDispatcher) {
            _swdbDao = dao;
            _eventDispatcher = eventDispatcher;
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
        public IGenericResponseResult RestoreDefaultMetadata() {
            var resultData = _swdbDao.FindByQuery<Metadataeditor>(Metadataeditor.ByDefaultId);
            var metadata = (from c in resultData select c.SystemStringValue).FirstOrDefault();
            return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(metadata, "metadata"));

        }
        [HttpGet]
        public DataTable RestoreSavedMetadata() {
            var resultData = _swdbDao.FindByQuery<Metadataeditor>(Metadataeditor.BySavedId);

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

            //return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(Metadata, "metadata"));

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
            var comments = json.First.Last.ToString();
            var metadata = json.Last.Last.ToString();

            
            var now = DateTime.Now;
            var newMetadataEntry = new Metadataeditor() {
                SystemStringValue = metadata,
                Comments = comments,
                CreatedDate = now,
                DefaultId = 0,

            };
            _swdbDao.Save(newMetadataEntry);
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