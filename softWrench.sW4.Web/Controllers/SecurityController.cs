using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using cts.commons.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    public class SecurityController : ApiController {

        [Import]
        public IConfigurationFacade Facade { get; set; }


        [HttpGet]
        public async Task<string> GenerateHashedKey(string message) {
            var key = await Facade.LookupAsync<string>(ConfigurationConstants.HashKey);
            if (string.IsNullOrEmpty(key)) {
                throw new InvalidOperationException("please inform hash key at /Global/Security/HashKey");
            }
            return AuthUtils.HmacShaEncode(message, Encoding.ASCII.GetBytes(key));
        }
    }
}