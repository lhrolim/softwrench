using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.SPF {
    public class RedirectService : ISingletonComponent {

        private readonly IContextLookuper _lookuper;

        private const string UrlTemplate =
           "{0}/Home/RedirectToAction?application={1}&querystring=id${2}@key%5BschemaId%5D${3}@key%5Bmode%5D$@{4}key%5Bplatform%5D$web";

        public RedirectService(IContextLookuper lookuper) {
            _lookuper = lookuper;
        }

        public string GetApplicationUrl(string application, string schemaId, String mode, String id) {
            var fullContext = _lookuper.GetFromMemoryContext<SwHttpContext>("httpcontext");
            return UrlTemplate.Fmt(fullContext, application, id, schemaId, mode);
        }

    }
}
