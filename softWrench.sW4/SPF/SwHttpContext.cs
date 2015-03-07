using System;
using System.Globalization;
using cts.commons.portable.Util;
using cts.commons.Util;
using softWrench.sW4.Util;

namespace softWrench.sW4.SPF {
    public class SwHttpContext {
        public String Protocol { get; set; }
        public String URL { get; set; }
        public String Port { get; set; }
        public String Context { get; set; }

        public SwHttpContext(string protocol, string url, int port, string context) {
            Protocol = protocol;
            URL = url;
            Port = port.ToString(CultureInfo.InvariantCulture);
            Context = context;
        }

        public override string ToString() {
            return "{0}://{1}:{2}{3}".Fmt(Protocol ?? "http", URL, Port ?? "80", Context);
        }
    }
}