using System.Web.Mvc;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Common.Log;
using softWrench.sW4.Web.SPF.Filters;

namespace softWrench.sW4.Web.App_Start {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MvContextFilter());
            filters.Add(new MVCLogFilter());
            //TODO: needs more testings
            filters.Add(new GenericExceptionFilter());
        }
    }
}