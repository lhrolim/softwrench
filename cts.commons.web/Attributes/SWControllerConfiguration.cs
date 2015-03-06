using System;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using cts.commons.simpleinjector;
using cts.commons.web.Formatting;

namespace cts.commons.web.Attributes {

    //TODO: review this, needs to be more automatic
    /// <summary>
    /// Use this attribute for configuring the controllers for the SPF sw application
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SWControllerConfiguration : Attribute, IControllerConfiguration {

        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor) {
            controllerSettings.Formatters.Remove(controllerSettings.Formatters.JsonFormatter);
            var formatter = SimpleInjectorGenericFactory.Instance.GetObject<ISWJsonFormatter>(typeof(ISWJsonFormatter));
            controllerSettings.Formatters.Insert(0, (MediaTypeFormatter)formatter);
        }
    }
}