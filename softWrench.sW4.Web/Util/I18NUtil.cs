using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Util {
    public class I18NUtil {

        public static IDictionary<string, string> FetchCatalogs() {
            var currentClient = ApplicationConfiguration.ClientName;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory + "\\Client\\";
            var clientDir = baseDir + currentClient;
            return new Dictionary<string, string>();
        }
    }
}