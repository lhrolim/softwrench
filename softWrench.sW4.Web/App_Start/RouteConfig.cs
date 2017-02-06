using System.Web.Mvc;
using System.Web.Routing;

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

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}