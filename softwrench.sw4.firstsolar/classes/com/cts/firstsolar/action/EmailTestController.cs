using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using cts.commons.simpleinjector;
using cts.commons.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softwrench.sw4.webcommons.classes.api;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {

    [RawController]
    public class EmailTestController : Controller {


        [Import]
        public SWDBHibernateDAO Dao { get; set; }

        [System.Web.Http.HttpGet]
        public ActionResult Callout() {

            var service = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarCallOutEmailService>();
            var callOut = Dao.FindAll<CallOut>(typeof(CallOut)).First();
            var package = Dao.FindAll<WorkPackage>(typeof(WorkPackage)).First();
            AsyncHelper.RunSync(() => SimpleInjectorGenericFactory.Instance
                .GetObject<FirstSolarCustomGlobalFedService>().LoadGfedData(package, callOut));
            var html = service.GenerateEmailBody(callOut, package, "1803");

            var file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Desktop\\callout.html");
            file.WriteLine(html);
            file.Close();

            dynamic expando = new ExpandoObject();
            var htmlModel = expando as IDictionary<string, object>;
            htmlModel.Add("content", html);

            return View("Index", expando);
        }


        [System.Web.Http.HttpGet]
        public ActionResult Maintenance() {

            var service = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarMaintenanceEmailService>();
            var callOut = Dao.FindAll<MaintenanceEngineering>(typeof(MaintenanceEngineering)).First();
            var package = Dao.FindAll<WorkPackage>(typeof(WorkPackage)).First();

            var html = service.GenerateEmailBody(callOut, package, "1803");

            var file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Desktop\\maintenance.html");
            file.WriteLine(html);
            file.Close();

            dynamic expando = new ExpandoObject();
            var htmlModel = expando as IDictionary<string, object>;
            htmlModel.Add("content", html);

            return View("Index", expando);
        }

        [System.Web.Http.HttpGet]
        public ActionResult MaintenanceReject() {

            var service = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarMaintenanceEmailService>();
            var me = Dao.FindAll<MaintenanceEngineering>(typeof(MaintenanceEngineering)).First();
            var package = Dao.FindAll<WorkPackage>(typeof(WorkPackage)).First();

            var html = service.GenerateRejectEmailBody(me, package, "1803");

            var file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Desktop\\maintenancereject.html");
            file.WriteLine(html);
            file.Close();

            dynamic expando = new ExpandoObject();
            var htmlModel = expando as IDictionary<string, object>;
            htmlModel.Add("content", html);

            return View("Index", expando);
        }

        [System.Web.Http.HttpGet]
        public ActionResult Meeting() {
            var service = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarDailyOutageMeetingEmailService>();
            var handler = SimpleInjectorGenericFactory.Instance.GetObject<FirstSolarDailyOutageMeetingHandler>();
            var dom = Dao.FindAll<DailyOutageMeeting>(typeof(DailyOutageMeeting)).First();
            var package = Dao.FindAll<WorkPackage>(typeof(WorkPackage)).First();
            handler.HandleMwhTotalsAfterSave(package);

            var html = service.GenerateEmailBody(dom, package, "1803");

            var file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Desktop\\meeting.html");
            file.WriteLine(html);
            file.Close();

            dynamic expando = new ExpandoObject();
            var htmlModel = expando as IDictionary<string, object>;
            htmlModel.Add("content", html);

            return View("Index", expando);
        }
    }
}
