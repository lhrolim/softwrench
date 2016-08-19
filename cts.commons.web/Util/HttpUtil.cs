using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cts.commons.web.Util {
    public static class HttpUtil {

        public static Dictionary<string, string> DecodeQueryParameters(this string uriString) {
            var uri = new Uri(uriString);

            if (uri == null)
                throw new ArgumentNullException("uri");

            if (uri.Query.Length == 0)
                return new Dictionary<string, string>();

            return uri.Query.TrimStart('?')
                            .Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(kvp => kvp.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
                            .ToDictionary(kvp => kvp[0],
                                          kvp => kvp.Length > 2 ? string.Join("=", kvp, 1, kvp.Length - 1) : (kvp.Length > 1 ? kvp[1] : ""));
        }

    }
}
