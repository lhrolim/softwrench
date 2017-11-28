using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using cts.commons.simpleinjector;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email;
using softwrench.sw4.webcommons.classes.api;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.action {

    [RawController]
    public class DispatchEmailTestController : Controller {

        [Import]
        public SWDBHibernateDAO Dao { get; set; }

        [System.Web.Http.HttpGet]
        public ActionResult Dispatch() {
            var service = SimpleInjectorGenericFactory.Instance.GetObject<DispatchEmailService>();
            return BaseDispatch(service, "\\Desktop\\dispatch.html");
        }

        [System.Web.Http.HttpGet]
        public ActionResult DispatchSms() {
            var service = SimpleInjectorGenericFactory.Instance.GetObject<DispatchSmsEmailService>();
            return BaseDispatch(service, "\\Desktop\\dispatchsms.html");
        }

        private ActionResult BaseDispatch(BaseDispatchEmailService service, string path) {
            var ticket = Dao.FindAll<DispatchTicket>(typeof(DispatchTicket)).First();
            var site = Dao.FindAll<GfedSite>(typeof(GfedSite)).First();
            var html = service.BuildMessage(ticket, site,true);
            var file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + path);
            file.WriteLine(html);
            file.Close();
            dynamic expando = new ExpandoObject();
            var htmlModel = expando as IDictionary<string, object>;
            htmlModel.Add("content", html);
            return View("Index", expando);
        }
    }
}
