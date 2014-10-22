using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace softWrench.sW4.Web.Models.Home {
    public class HomeConfigs {

        public long InitTimeMillis { get; set; }

        public string Logo { get; set; }
        public Boolean MyProfileEnabled { get; set; }

        public Boolean I18NRequired { get; set; }

        public string Environment { get; set; }
        public Boolean IsLocal { get; set; }

        public string ClientName { get; set; }
        public string ClientSideLogLevel { get; set; }
        public int SuccessMessageTimeOut { get; set; }
        public string ScanOrder { get; set; }
    }
}