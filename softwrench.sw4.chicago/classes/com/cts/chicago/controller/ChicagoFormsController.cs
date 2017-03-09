using System;
using System.Web.Http;
using System.Web.Mvc;
using cts.commons.web.Attributes;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.chicago.classes.com.cts.chicago.dataset;
using softwrench.sw4.webcommons.classes.api;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.controller {

    [System.Web.Http.Authorize]
    [SWControllerConfiguration]
    [NoMenuController]
    public class ChicagoFormsController : Controller {

        private readonly ChicagoFormsEngine _engine;
        private readonly IMemoryContextLookuper _lookuper;

        public ChicagoFormsController(ChicagoFormsEngine engine, IMemoryContextLookuper lookuper) {
            _engine = engine;
            _lookuper = lookuper;
        }

        [System.Web.Http.HttpGet]
        public void GetPdfForm([FromUri] string formName, [FromUri] bool isIbm) {
            var path = GetFormPath(formName, isIbm);
            var content = System.IO.File.ReadAllBytes(path);
            var response = System.Web.HttpContext.Current.Response;
            response.ContentType = "application/pdf";
            response.AddHeader("content-disposition", "attachment; filename=" + formName + ".pdf");
            response.BufferOutput = true; ;
            response.OutputStream.Write(content, 0, content.Length);
            response.End();
        }

        public ActionResult Index([FromUri] string formName, [FromUri] bool isIbm) {
            GetFormPath(formName, isIbm); // validation
            return View(new ChicagoFormsModel() {
                Title = formName,
                ClientName = "chicago",
                PdfUrl = GetContextPath() + "/ChicagoForms/GetPdfForm?formName=" + formName + "&isIbm=" + isIbm,
                FormName = formName
            });
        }

        private string GetFormPath(string formName, bool isIbm) {
            if (string.IsNullOrEmpty(formName)) {
                throw new Exception("Form name empty.");
            }

            // lack of permission
            if (isIbm && !_engine.IsIbmUser()) {
                throw new Exception("Form " + formName + " not found.");
            }

            var formsDir = isIbm ? _engine.GetIbmFormsDir() : _engine.GetFormsDir();
            var fileName = formName + ".pdf";
            var path = formsDir + (formsDir.EndsWith("/") || formsDir.EndsWith("\\") ? "" : "/") + fileName;

            if (!System.IO.File.Exists(path)) {
                throw new Exception("Form " + formName + " not found.");
            }

            return path;
        }

        private string GetContextPath() {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            if (fullContext == null) {
                return null;
            }
            var contextPath = fullContext.Context;
            if (contextPath == null) {
                return null;
            }
            if (contextPath.EndsWith("/")) {
                contextPath = contextPath.Substring(0, contextPath.Length - 1);
            }
            return contextPath;
        }

        public class ChicagoFormsModel {
            public string Title { get; set; }
            public string ClientName { get; set; }
            public string PdfUrl { get; set; }
            public string FormName { get; set; }
        }
    }
}
