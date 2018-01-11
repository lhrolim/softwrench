using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dispatch;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.SPF;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {

    public class FSDBackendController : ApiController {

        [Import]
        public WorkOrderFromDispatchService WorkOrderFromDispatchService { get; set; }

        [Import]
        public IConfigurationFacade ConfigFacade { get; set; }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IGenericResponseResult> CreateWorkorders([FromBody]JsonRequestWrapper wrapper) {
            var keysSt = await ConfigFacade.LookupAsync<string>(ConfigurationConstants.CustomerHashKeys);
            var keys = PropertyUtil.ConvertToDictionary(keysSt);

            if (string.IsNullOrEmpty(wrapper.HashSignature) || string.IsNullOrEmpty(wrapper.MessageToSign)) {
                throw new InvalidOperationException("Signature objects missing. Service cannot be properly autheticated");
            }

            if (!keys.ContainsKey("firstsolardispatch")) {
                throw new InvalidOperationException("configure a customer hashkey for firstsolardispatch app");
            }

            var opSignature = wrapper.MessageToSign;
            var key = (string)keys["firstsolardispatch"];
            var hashKey = AuthUtils.HmacShaEncode(opSignature, Encoding.ASCII.GetBytes(key));
            if (hashKey != wrapper.HashSignature) {
                throw new InvalidOperationException("Signature mismatch. Service cannot be properly autheticated");
            }



            var targetResult = await WorkOrderFromDispatchService.SynchronizeDispatchTicket(wrapper.Json);
            var response = new BlankApplicationResponse { SuccessMessage = targetResult.SuccessMessage };
            if (targetResult.WarnMessage != null) {
                response.WarningDto = new ErrorDto() { WarnMessage = targetResult.WarnMessage };
            }

            return response;
        }

    }
}
