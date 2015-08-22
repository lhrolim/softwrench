﻿using System;
using System.Globalization;
using cts.commons.portable.Util;

namespace softwrench.sw4.api.classes.fwk.context {
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