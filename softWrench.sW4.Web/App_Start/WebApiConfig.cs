using System.Web.Http;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.SPF.Filters;

namespace softWrench.sW4.Web.App_Start {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            ConfigRoutes(config);

            config.Filters.Add(new LogFilter());
            config.Filters.Add(new ContextFilter());
            config.Filters.Add(new GenericExceptionFilter());
            config.Filters.Add(new CompressionFilter());
            config.Filters.Add(new RedirectUrlFilter());
            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();

            // TODO: To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            config.EnableSystemDiagnosticsTracing();
        }

        private static void ConfigRoutes(HttpConfiguration config) {
            config.Routes.MapHttpRoute(
                "DataApi",
                "api/data/crud/{application}/{id}",
                new { id = RouteParameter.Optional, application = RouteParameter.Optional, controller = "Data" });

            config.Routes.MapHttpRoute(
                "SyncApi",
                "api/data/sync/",
                new { controller = "Data" });



            config.Routes.MapHttpRoute(
                "CustomDataApi",
                "api/data/operation/{application}/{operation}",
                new { controller = "Data" , action = "Operation" });

            config.Routes.MapHttpRoute(
                "MetadataApi",
                "api/metadata/{controller}/{application}/{id}",
                new { id = RouteParameter.Optional, application = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                "MenuApi",
                "api/menu/",
                new { controller = "Menu" });

            config.Routes.MapHttpRoute(
                name: "SecurityApi",
                routeTemplate: "api/security/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            config.Routes.MapHttpRoute(
                "GenericAPI",
                "api/generic/{controller}/{action}/{context}",
                new { context = RouteParameter.Optional,  }
                );


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{application}/{id}",
                defaults: new { id = RouteParameter.Optional, application = RouteParameter.Optional }
                );
        }
    }
}
