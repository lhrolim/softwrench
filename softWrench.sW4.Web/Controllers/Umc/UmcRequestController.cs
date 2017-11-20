using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softwrench.sw4.webcommons.classes.api;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.Home;

namespace softwrench.sw4.web.Controllers.Umc {
    [NoMenuController]
    public class UmcRequestController : Controller {

        private const string Index = "~/Views/Home/Index.cshtml";
        private const string Client = "umc";

        [Import]
        public HomeService HomeService { get; set; }

        [Import]
        public DataSetProvider DataSetProvider { get; set; }

        [Import]
        public I18NResolver I18NResolver { get; set; }

        [AllowAnonymous]
        public async Task<ActionResult> New() {
            if (!Client.Equals(ApplicationConfiguration.ClientName)) {
                return null;
            }
            return await BuildResult(new ApplicationMetadataSchemaKey("nologinnewdetail", SchemaMode.input, ClientPlatform.Web));
        }

        [AllowAnonymous]
        public ActionResult Success(string id) {
            if (!Client.Equals(ApplicationConfiguration.ClientName)) {
                return null;
            }

            var user = InMemoryUser.NewAnonymousInstance();
            var model = HomeService.BaseHomeModel(Request, user);
            model.Anonymous = true;

            var response = new softWrench.sW4.Data.API.Response.GenericApplicationResponse() {
                Title = "Success",
                RedirectURL = "/Content/Customers/umc/htmls/SubmitSuccess.html",
                ResultObject = id
            };

            model.ResultDataJSON = JsonConvert.SerializeObject(response, Formatting.None, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return View(Index, model);
        }

        private async Task<ActionResult> BuildResult(ApplicationMetadataSchemaKey key) {
            var user = InMemoryUser.NewAnonymousInstance();

            var applicationMetadata = MetadataProvider
                .Application("servicerequest")
                .ApplyPolicies(key, user, ClientPlatform.Web);

            var detailRequest = new DetailRequest(null, key);
            var dataSet = DataSetProvider.LookupDataSet("servicerequest", applicationMetadata.Schema.SchemaId);
            var response = await dataSet.GetApplicationDetail(applicationMetadata, user, detailRequest);

            response.Title = I18NResolver.I18NSchemaTitle(response.Schema);
            response.Mode = "input";

            var model = HomeService.BaseHomeModel(Request, user, applicationMetadata.Schema);
            response.RedirectURL = "/Content/Controller/Application.html";
            model.Anonymous = true;
            model.ResultDataJSON = JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            return View(Index, model);
        }
    }
}
