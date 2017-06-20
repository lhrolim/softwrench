using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JetBrains.Annotations;
using log4net;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.webcommons.classes.api;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.Home;
using softWrench.sW4.Web.Security;

namespace softWrench.sW4.Web.Controllers.FirstSolar {

    //TODO: move to FirstSolar package later... couldn´t figure a way to reuse HomeService methods, or to redirect to a controller that requires authorization
    [NoMenuController]
    public class FirstSolarWpEmailController : Controller {

        private const string Index = "~/Views/Home/Index.cshtml";

        [Import]
        public SWDBHibernateDAO Dao { get; set; }

        [Import]
        public HomeService HomeService { get; set; }

        [Import]
        public DataSetProvider DataSetProvider { get; set; }

        [Import]
        public I18NResolver _i18NResolver { get; set; }


        private ILog Log = LogManager.GetLogger(typeof(FirstSolarWpEmailController));

        public async Task<ActionResult> Ack(string token) {
            var wp = await Dao.FindSingleByQueryAsync<WorkPackage>(WorkPackage.ByToken, token);
            if (wp == null) {
                Log.WarnFormat("work package with token {0} not found", token);
            }

            var user = InMemoryUser.NewAnonymousInstance();

            var key = new ApplicationMetadataSchemaKey("detail", SchemaMode.output, ClientPlatform.Web);

            var applicationMetadata = MetadataProvider
                .Application("workpackage_")
                .ApplyPolicies(key, user, ClientPlatform.Web);


            var response = await DataSetProvider.LookupDataSet("workpackage_", applicationMetadata.Schema.SchemaId).Get(applicationMetadata, user, new DetailRequest(wp.Id.ToString(), key));

            response.Title = _i18NResolver.I18NSchemaTitle(response.Schema);

            var model = HomeService.BaseHomeModel(Request, user, applicationMetadata.Schema);
            model.ResultData = response;

            return View(Index, model);

        }

    }
}