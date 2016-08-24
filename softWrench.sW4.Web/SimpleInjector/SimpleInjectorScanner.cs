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
            container.Options.AllowOverridingRegistrations = true;

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
                var types = assembly.GetTypes().Where(type => typeof(IComponent).IsAssignableFrom(type));
                foreach (var typeToRegister in types) {
                    if (typeToRegister.IsInterface || typeToRegister.IsAbstract) {
                        continue;
                    }
                    var shouldIgnore = typeToRegister.GetCustomAttribute(typeof(IgnoreComponentAttribute));
                    if (shouldIgnore != null) {
                        continue;
                    }
                    var attributes = typeToRegister.GetAllAttributes<ComponentAttribute>();
                    var attr = attributes.FirstOrDefault();
                    var reg = Lifestyle.Singleton.CreateRegistration(typeToRegister, container);
                    if (attr != null) {
                        RegisterFromAttribute(attr, tempDict, reg);
                    }
                    var overridingAnnotation = typeToRegister.GetCustomAttribute(typeof(OverridingComponentAttribute));
                    if (overridingAnnotation != null && typeToRegister.BaseType != null) {
                        RegisterOverridingBaseClass(container, (OverridingComponentAttribute)overridingAnnotation, typeToRegister, reg);
                    }

                    var name = typeToRegister.Name;
                    RegisterFromInterfaces(typeToRegister, tempDict, reg);
                    if (overridingAnnotation == null && attr == null) {
                        RegisterClassItSelf(container, typeToRegister, reg);
                    }

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

        private static void RegisterOverridingBaseClass(Container container, OverridingComponentAttribute overridingAnnotation, Type realType, Registration simpleInjectorRegistration) {
            var type = realType.BaseType;
            if (overridingAnnotation.ClientFilters != null && (!overridingAnnotation.ClientFilters.Split(',').Contains(ApplicationConfiguration.ClientName))) {
                Log.DebugOrInfoFormat("ignoring overriding type {0} due to client filters", realType.Name);
                return;
            }

            if (type.IsPublic) {
                Log.DebugOrInfoFormat("registering replacement {0} for base class {1}", realType.Name, type.Name);
                container.AddRegistration(type, simpleInjectorRegistration);
                SimpleInjectorGenericFactory.RegisterNameAndType(realType);
            }
        }

        private static void RegisterClassItSelf(Container container, Type registration, Registration reg) {
            if (registration.IsPublic)
            {
                var name = registration.Name;
                if (SimpleInjectorGenericFactory.ContainsService(registration)) {
                    Log.DebugOrInfoFormat("ignoring type {0} due to presence of existing overriding", registration.Name);
                    return;
                }

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
            if (registrationType != null) {
                if (!tempDict.ContainsKey(registrationType)) {
                    tempDict.Add(registrationType, new List<Registration>());
                }
                tempDict[registrationType].Add(registration);
            }
        }


    }
}
