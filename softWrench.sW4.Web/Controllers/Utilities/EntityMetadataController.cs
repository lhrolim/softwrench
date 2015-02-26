using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using System.Xml.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using cts.commons.web.Attributes;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.Controllers.Utilities;
using softWrench.sW4.Web.SPF;

namespace softWrench.sW4.Web.Controllers.Utilities {

    [Authorize]
    public class EntityMetadataController : ApiController {
        private readonly IEventDispatcher _eventDispatcher;

        public EntityMetadataController(IEventDispatcher eventDispatcher) {
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
        [SPFRedirect("StatusColour Editor", "_headermenu.statuscoloreditor", "EntityMetadataEditor")]
        public IGenericResponseResult StatuscolorEditor() {
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
        [HttpPut]
        public void SaveStatuscolor(HttpRequestMessage request) {
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