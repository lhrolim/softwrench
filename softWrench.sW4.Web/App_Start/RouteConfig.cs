using System.Web.Mvc;
using System.Web.Routing;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web {
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // route to crud detail using the entity id
            routes.MapRoute(
                name: "RouteUID",
                url: "web/{application}/uid/{uid}",
                defaults: new { controller = "Route", action = "Route" }
            );

            // route to a crud grid of a application
            routes.MapRoute(
                name: "Route",
                url: "web/{application}",
                defaults: new { controller = "Route", action = "Route" }
            );

            // route to a crud detail with the userid as extra or a crud new detail with "new" as extra
            routes.MapRoute(
                name: "RouteWithExtra",
                url: "web/{application}/{extra}",
                defaults: new { controller = "Route", action = "Route" }
            );

            if ("umc".Equals(ApplicationConfiguration.ClientName)) {
                // route to make request anonymous to umc
                routes.MapRoute(
                    name: "UmcNoLoginRequest",
                    url: "umcrequest",
                    defaults: new { controller = "UmcRequest", action = "New" }
                );
                // route to succes page of umc request
                routes.MapRoute(
                    name: "UmcNoLoginSuccess",
                    url: "umcrequestsuccess",
                    defaults: new { controller = "UmcRequest", action = "Success" }
                );
            }

            if ("swgas".Equals(ApplicationConfiguration.ClientName)) {
                // route to make request anonymous to umc
                routes.MapRoute(
                    name: "SwgNoLoginRequest",
                    url: "swgasrequest",
                    defaults: new { controller = "SwgasRequest", action = "New" }
                );
                // route to succes page of umc request
                routes.MapRoute(
                    name: "SwgNoLoginSuccess",
                    url: "swgasrequestsuccess",
                    defaults: new { controller = "SwgasRequest", action = "Success" }
                );
            }

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}