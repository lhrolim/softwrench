using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using cts.commons.persistence;
using cts.commons.web.Attributes;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.chicago.classes.com.cts.chicago.controller;
using softwrench.sw4.webcommons.classes.api;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Help;
using softWrench.sW4.Util;
using CompressionUtil = cts.commons.Util.CompressionUtil;

namespace softWrench.sW4.Web.Help {


    [System.Web.Http.Authorize]
    [SWControllerConfiguration]
    [NoMenuController]
    public class HelpController : Controller {

        private readonly ISWDBHibernateDAO _dao;
        private readonly IMemoryContextLookuper _lookuper;

        public HelpController(ISWDBHibernateDAO dao, IMemoryContextLookuper lookuper) {
            _dao = dao;
            _lookuper = lookuper;
        }



        [System.Web.Http.HttpGet]
        public void DownloadHelpEntry([FromUri] int id) {
            var helpEntry = _dao.FindByPK<HelpEntry>(typeof(HelpEntry), id);
            var decompressed = CompressionUtil.Decompress(helpEntry.Data);
            var response = System.Web.HttpContext.Current.Response;
            response.ContentType = "application/pdf";
            response.AddHeader("content-disposition", "attachment; filename=" + helpEntry.DocumentName);
            response.BufferOutput = true;
            response.OutputStream.Write(decompressed, 0, decompressed.Length);
            response.End();
        }




        public ActionResult Index([FromUri] int id, string label) {

            return View(new PdfFormsModel {
                Title = label,
                ClientName = ApplicationConfiguration.ClientName,
                PdfUrl = GetContextPath() + "/Help/DownloadHelpEntry?id=" + id,
                FormName = label
            });
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

        public class PdfFormsModel {
            public string Title {
                get; set;
            }
            public string ClientName {
                get; set;
            }
            public string PdfUrl {
                get; set;
            }
            public string FormName {
                get; set;
            }
        }



    }

    public class HelpApiController : ApiController {

        private readonly ISWDBHibernateDAO _dao;

        public HelpApiController(ISWDBHibernateDAO dao) {
            _dao = dao;
        }


        [System.Web.Http.HttpPost]
        public async Task<BlankApplicationResponse> Delete([FromUri] int id) {
            var helpEntry = _dao.FindByPK<HelpEntry>(typeof(HelpEntry), id);
            if (helpEntry != null) {
                await _dao.DeleteAsync(helpEntry);
            }
            return new BlankApplicationResponse{SuccessMessage = "Entry Deleted Successfully"};
        }
    }

}