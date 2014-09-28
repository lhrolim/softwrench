using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SimpleInjector;
using softWrench.sW4.Web.Common;

namespace softWrench.sW4.Web.SimpleInjector.WebApi {
    public class SimpleInjectorWebAPIUtil {

        public static void RegisterWebApiControllers(Container container) {
            var registrations = typeof(SimpleInjectorScanner).Assembly.GetTypes().Where(type => typeof(ApiController).IsAssignableFrom(type));
            var dict = new Dictionary<string, Type>();
            foreach (var registration in registrations) {
                if (registration.IsInterface || registration.IsAbstract) {
                    continue;
                }
                dict.Add(WebAPIUtil.RemoveControllerSufix(registration), registration);
            }
            container.RegisterSingle<IAPIControllerFactory>(new WEbApiSimpleInjectorFactory(container, dict));
        }

        private class WEbApiSimpleInjectorFactory : IAPIControllerFactory {
            private readonly Container _container;
            private readonly Dictionary<string, Type> _dictionary;

            public WEbApiSimpleInjectorFactory(Container container, Dictionary<string, Type> dictionary) {
                _container = container;
                _dictionary = dictionary;
            }

            public ApiController CreateNew(string name) {
                return (ApiController)_container.GetInstance(_dictionary[name]);
            }
        }
    }
}