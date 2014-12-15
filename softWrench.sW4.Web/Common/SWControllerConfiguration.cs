using softWrench.sW4.Web.Formatting;
using System;
using System.Web.Http.Controllers;

namespace softWrench.sW4.Web.Common {
    /// <summary>
    /// Use this attribute for configuring the controllers for the SPF sw application
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SWControllerConfiguration : Attribute, IControllerConfiguration {

        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor) {

            controllerSettings.Formatters.Remove(controllerSettings.Formatters.JsonFormatter);
            var formatter = new ResponseJsonFormatter();
            controllerSettings.Formatters.Insert(0, formatter);
        }
    }
}