using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {

    public class FirstSolarMaintenanceEngineerDataSet : SWDBApplicationDataset {

        [Import]
        public IConfigurationFacade ConfigFacade { get; set; }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {

            var result = await base.GetApplicationDetail(application, user, request);

            if (!request.IsEditionRequest) {
                var defaultEmail = ConfigFacade.Lookup<string>(FirstSolarOptConfigurations.DefaultMeToEmailKey);
                result.ResultObject.SetAttribute("defaultmetoemail", defaultEmail);
            }


            return result;

        }


        public override string ApplicationName() {
            return "_MaintenanceEngineering";
        }

        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}
