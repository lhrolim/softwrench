using System;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;

namespace softwrench.sw4.api.classes.fwk.context {
    public class RedirectService : ISingletonComponent {

        private readonly IMemoryContextLookuper _lookuper;

        private const string ApplicationUrlTemplate =
           "{0}/Home/RedirectToAction?application={1}&querystring=id${2}@key%5BschemaId%5D${3}@key%5Bmode%5D$@{4}key%5Bplatform%5D$web";

        private const string ActionUrlTemplate = "{0}/{1}/{2}";


        public RedirectService(IMemoryContextLookuper lookuper) {
            _lookuper = lookuper;
        }

        public string GetApplicationUrl(string application, string schemaId, String mode, String id) {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            return ApplicationUrlTemplate.Fmt(fullContext, application, id, schemaId, mode);
        }

        public string GetActionUrl(string controller, string action, string queryString) {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            if (fullContext == null) {
                throw new InvalidOperationException("context cannot be null");
            }
            var actionUrl = ActionUrlTemplate.Fmt(fullContext, controller, action);
            if (queryString != null) {
                actionUrl += "?" + queryString;
            }
            return actionUrl;
        }

        public string GetApiActionUrl(string controller, string action, string queryString) {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            if (fullContext == null) {
                throw new InvalidOperationException("context cannot be null");
            }
            var actionUrl = string.Format("{0}/{1}/{2}/{3}", fullContext, "api/generic", controller, action);
            if (queryString != null) {
                actionUrl += "?" + queryString;
            }
            return actionUrl;
        }

        public string GetRootUrl() {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            return fullContext + "/";
        }

    }
}
