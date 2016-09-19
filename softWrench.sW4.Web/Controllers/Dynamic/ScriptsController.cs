using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using NHibernate.Util;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Dynamic;
using softWrench.sW4.SPF;
using softWrench.sW4.Web.Email;
using softWrench.sW4.Web.SimpleInjector;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.Controllers.Dynamic {

    [Authorize]
    [SWControllerConfiguration]
    public class ScriptsController : ApiController {
        private readonly ScriptsService _scriptsService;
        private readonly ScriptsEmailer _scriptsEmailer;

        public ScriptsController(ScriptsService scriptsService, ScriptsEmailer scriptsEmailer) {
            _scriptsService = scriptsService;
            _scriptsEmailer = scriptsEmailer;
        }

        [HttpGet]
        [SPFRedirect("Evaluate Scripts", "_headermenu.scripts", "Scripts")]
        public GenericResponseResult<string> Index() {
            return new GenericResponseResult<string>("");
        }

        [HttpPost]
        public GenericResponseResult<string> Evaluate([FromBody] ScriptsService.ScriptDTO scriptDto, HttpRequestMessage request) {
            var result = _scriptsService.EvaluateScript(scriptDto.Script);
            return new GenericResponseResult<string>(result == null ? "The result was null." : result.ToString());
        }

        [HttpPost]
        public GenericResponseResult<string> ReloadContainer([FromBody] ScriptsService.ReloadContainerDTO scriptDto, HttpRequestMessage request) {
            var dynTypesBefore = DynamicScannerHelper.CloneDynTypes();
            _scriptsService.ReloadContainer(null);
            var dynTypesAfter = DynamicScannerHelper.CloneDynTypes();

            Task.Run(() => {
                var deployed = dynTypesAfter.Where(afterPair => !dynTypesBefore.ContainsKey(afterPair.Key)); ;
                var undeployed = dynTypesBefore.Where(beforePair => !dynTypesAfter.ContainsKey(beforePair.Key));

                var email = new ContainerReloadEmail {
                    OnContainer = JoinRecords(dynTypesAfter),
                    Deployed = JoinRecords(deployed),
                    Undeployed = JoinRecords(undeployed)
                };
                _scriptsEmailer.FillBaseEmailDTO(email, request.GetIPAddress(), scriptDto.Comment, scriptDto.Username, "Container Reloaded");
                _scriptsEmailer.SendContainerReloadEmail(email);
            });


            return new GenericResponseResult<string>("");
        }

        private static string JoinRecords(IEnumerable<KeyValuePair<string, DynamicScannerHelper.DynamicComponentRecord>> recordsMap) {
            var nameList = new List<string>();
            recordsMap.ForEach(pair => {
                var record = pair.Value;
                nameList.Add(record.Name + " (" + pair.Key + ")");
            });
            return string.Join(", ", nameList);
        }
    }
}