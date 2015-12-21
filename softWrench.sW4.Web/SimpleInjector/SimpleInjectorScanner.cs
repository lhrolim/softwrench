using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using cts.commons.Util;
using FluentMigrator.Infrastructure.Extensions;
using log4net;
using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;
using softWrench.sW4.Web.SimpleInjector.WebApi;

namespace softWrench.sW4.Web.SimpleInjector {
    class SimpleInjectorScanner {

        private static readonly ILog Log = LogManager.GetLogger(typeof(SimpleInjectorScanner));

        public static Container InitDIController() {
            var before = Stopwatch.StartNew();
            // Create the container as usual.
            var container = new Container();

            RegisterComponents(container);
            SimpleInjectorWebAPIUtil.RegisterWebApiControllers(container);

            // Register the dependency resolver.
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            // This is an extension method from the integration package as well.
            container.RegisterMvcAttributeFilterProvider();

            // Verify the container configuration
            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            Log.Debug(LoggingUtil.BaseDurationMessage("SimpleInjector context initialized in {0}", before));
            return container;
        }



        private static void RegisterComponents(Container container) {

            var assemblies = AssemblyLocator.GetSWAssemblies();
            IDictionary<Type, IList<Registration>> tempDict = new Dictionary<Type, IList<Registration>>();
            foreach (var assembly in assemblies) {
                var registrations = assembly.GetTypes().Where(type => typeof(IComponent).IsAssignableFrom(type));
                foreach (var registration in registrations) {
                    if (registration.IsInterface || registration.IsAbstract) {
                        continue;
                    }
                    var shouldIgnore = registration.GetCustomAttribute(typeof(IgnoreComponentAttribute));
                    if (shouldIgnore != null) {
                        continue;
                    }
                    var attributes = registration.GetAllAttributes<ComponentAttribute>();
                    var attr = attributes.FirstOrDefault();
                    var reg = Lifestyle.Singleton.CreateRegistration(registration, container);
                    if (attr != null) {
                        RegisterFromAttribute(attr, tempDict, reg);
                    }
                    var name = registration.Name;
                    RegisterFromInterfaces(registration, tempDict, reg);
                    RegisterClassItSelf(container, registration, reg);
                }
            }
            foreach (var entry in tempDict) {
                var coll = entry.Value;
                var serviceType = entry.Key;
                if (typeof(ISingletonComponent).IsAssignableFrom(serviceType)) {
                    container.AddRegistration(serviceType, coll.FirstOrDefault());
                    SimpleInjectorGenericFactory.RegisterNameAndType(serviceType);
                } else {
                    container.RegisterAll(serviceType, coll);
                }
            }
        }

        private static void RegisterClassItSelf(Container container, Type registration, Registration reg) {
            if (registration.IsPublic) {
                container.AddRegistration(registration, reg);
                SimpleInjectorGenericFactory.RegisterNameAndType(registration);
            }
        }

        private static void RegisterFromInterfaces(Type registration, IDictionary<Type, IList<Registration>> tempDict, Registration reg) {
            foreach (var type in registration.GetInterfaces().Where(type => typeof(IComponent).IsAssignableFrom(type))) {
                if (!tempDict.ContainsKey(type)) {
                    tempDict.Add(type, new List<Registration>());
                }
                tempDict[type].Add(reg);
            }


        }

        private static void RegisterFromAttribute(ComponentAttribute attr, IDictionary<Type, IList<Registration>> tempDict, Registration registration) {
            var registrationType = attr.RegistrationType;
            if (!tempDict.ContainsKey(registrationType)) {
                tempDict.Add(registrationType, new List<Registration>());
            }
            tempDict[registrationType].Add(registration);
        }
    }
}
