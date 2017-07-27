using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.webcommons.classes.api;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models;
using softWrench.sW4.Web.Models.Home;

namespace softWrench.sW4.Web.Controllers.FirstSolar {

    //TODO: move to FirstSolar package later... couldn´t figure a way to reuse HomeService methods, or to redirect to a controller that requires authorization
    [NoMenuController]
    public class FirstSolarWpGenericEmailController : Controller {

        private const string Index = "~/Views/Home/Index.cshtml";
        private const string Error = "~/Views/Shared/Error.cshtml";

        [Import]
        public SWDBHibernateDAO Dao { get; set; }

        [Import]
        public HomeService HomeService { get; set; }

        [Import]
        public DataSetProvider DataSetProvider { get; set; }

        [Import]
        public I18NResolver I18NResolver { get; set; }


        private readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarWpGenericEmailController));

        [System.Web.Mvc.AllowAnonymous]
        public async Task<ActionResult> Ack(string token, int emailStatusId) {
            var wp = await Dao.FindSingleByQueryAsync<WorkPackage>(WorkPackage.ByToken, token);

            if (wp == null) {
                Log.WarnFormat("work package with token {0} not found", token);
                return View(Error, ErrorModel(Request));
            }

            var emailStatus = wp.EmailStatuses.FirstOrDefault(e => e.Id == emailStatusId);
            if (emailStatus != null) {
                Log.InfoFormat("acking workpackage {0} for user email {1}", wp.Wonum, emailStatus.Email);
                emailStatus.AckDate = DateTime.Now;
                await Dao.SaveAsync(emailStatus);
            }

            return await BuildResult(wp, new ApplicationMetadataSchemaKey("viewdetail", SchemaMode.output, ClientPlatform.Web));
        }

        [System.Web.Mvc.AllowAnonymous]
        public async Task<ActionResult> DailyOutageView(string token) {
            var dom = await Dao.FindSingleByQueryAsync<DailyOutageMeeting>(new DailyOutageMeeting().ByToken, token);

            if (dom == null) {
                Log.WarnFormat("daily outage meeting with token {0} not found", token);
                return View(Error, ErrorModel(Request));
            }

            var wp = dom.WorkPackage;
            return await BuildResult(wp, new ApplicationMetadataSchemaKey("viewdailyoutage", SchemaMode.output, ClientPlatform.Web));
        }

        private async Task<ActionResult> BuildResult(WorkPackage wp, ApplicationMetadataSchemaKey key) {
            var user = InMemoryUser.NewAnonymousInstance();

            var applicationMetadata = MetadataProvider
                .Application("_workpackage")
                .ApplyPolicies(key, user, ClientPlatform.Web);


            var detailRequest = new DetailRequest(wp.Id.ToString(), key);

            var dataSet = DataSetProvider.LookupDataSet("_workpackage", applicationMetadata.Schema.SchemaId);

            var response = await dataSet.GetApplicationDetail(applicationMetadata, user, detailRequest);

            response.Title = I18NResolver.I18NSchemaTitle(response.Schema);
            response.Mode = "output";

            var model = HomeService.BaseHomeModel(Request, user, applicationMetadata.Schema);
            response.RedirectURL = "/Content/Controller/Application.html";
            model.Anonymous = true;

            model.ResultDataJSON = JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.None,
                new JsonSerializerSettings() {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            return View(Index, model);
        }

        public HomeModel ErrorModel(HttpRequestBase request) {
            var model = HomeService.BaseHomeModel(request, null);
            model.Error = ErrorConfig.GetLastError();
            model.Title = "Error | softWrench";
            return model;
        }

    }
}