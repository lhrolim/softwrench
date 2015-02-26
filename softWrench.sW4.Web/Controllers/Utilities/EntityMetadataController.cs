using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using System.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.SPF;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Entities;
using System.Xml;
using Newtonsoft.Json.Linq;
using softWrench.sW4.SimpleInjector;
using Newtonsoft.Json;
using System.IO;

namespace softWrench.sW4.Web.Controllers.Utilities {

    [Authorize]
    public class EntityMetadataController : ApiController {
        private static SWDBHibernateDAO _swdbDao;
       
        private SWDBHibernateDAO GetSWDBDAO()
        {
            if (_swdbDao == null)
            {
                _swdbDao = SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
            }
            return _swdbDao;
        }

        [HttpGet]
        [SPFRedirect("Metadata Builder", "_headermenu.metadatabuilder", "EntityMetadataBuilder")]
        public RedirectResponseResult Builder() {
            return new RedirectResponseResult();
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
        public IGenericResponseResult RestoreMetadata()
        {
            var resultData = GetSWDBDAO().FindByQuery<Metadataeditor>(Metadataeditor.ByDefaultId);



            string Metadata = (from c in resultData

                            select c.SystemStringValue).FirstOrDefault();
           return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(Metadata, "metadata"));
            
        }
        [HttpGet]
        [SPFRedirect("StatusColour Editor", "_headermenu.statuscoloreditor", "EntityMetadataEditor")]
        public IGenericResponseResult StatuscolorEditor()
        {
            using (var reader = new MetadataProvider().GetStream("statuscolors.json"))
            {
                var result = reader.ReadToEnd();
                return new GenericResponseResult<EntityMetadataEditorResult>(new EntityMetadataEditorResult(result, "statuscolors"));
            }
        }
        [HttpGet]
        [SPFRedirect("Menu Editor", "_headermenu.menueditor", "EntityMetadataEditor")]
        public IGenericResponseResult MenuEditor() {
            using (var reader = new MetadataProvider().GetStream("menu.web.xml"))
            {
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
        public EntityMetadataController(SWDBHibernateDAO dao)
        {
            _swdbDao = dao;
        }

        [HttpPost]
        public void SaveMetadataEditor(HttpRequestMessage request)
        {
            var content = request.Content;
            JObject json = JObject.Parse(content.ReadAsStringAsync().Result);
            var comments = json.First.Last.ToString();
            var Metadata = json.Last.Last.ToString();
           
           
            DateTime now = DateTime.Now;
            var newMetadataEntry = new Metadataeditor()
            {
                SystemStringValue = Metadata,
                Comments = comments,
                CreatedDate = now,
                DefaultId = 0,
                
            };
            _swdbDao.Save(newMetadataEntry);
        }
       
        [HttpPut]
        public void SaveStatuscolor(HttpRequestMessage request)
        {
            var task = request
            .Content
            .ReadAsStreamAsync();

            task.Wait();
            new MetadataProvider().SaveColor(task.Result);
            //new StatusColorResolver().ClearCache();
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