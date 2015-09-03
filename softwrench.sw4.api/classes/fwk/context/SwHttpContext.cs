using System;
using System.Globalization;
using cts.commons.portable.Util;

namespace softwrench.sw4.api.classes.fwk.context {
    public class SwHttpContext {
        public string Protocol { get; set; }
        public string URL { get; set; }
        public string Port { get; set; }
        public string Context { get; set; }

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