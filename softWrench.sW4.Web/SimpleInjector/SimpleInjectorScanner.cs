using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using cts.commons.persistence.Transaction;
using cts.commons.Util;
using FluentMigrator.Infrastructure.Extensions;
using log4net;
using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;
using cts.commons.simpleinjector;
using softWrench.sW4.Dynamic;
using softWrench.sW4.Util;
using softWrench.sW4.Web.SimpleInjector.WebApi;

namespace softWrench.sW4.Web.SimpleInjector {
    class SimpleInjectorScanner {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SimpleInjectorScanner));
        private const string DynReplaceMsg = "Replacing component {0} with the dynamic component {1}";

        public static Container InitDIController(ScriptEntry singleDynComponent) {
            var before = Stopwatch.StartNew();
            // Create the container as usual.
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.Options.PropertySelectionBehavior = new ImportPropertySelectionBehavior();

            container.InterceptWith<TransactionalInterceptor>(TransactionalHelper.IsTransactionable);

            DynamicScannerHelper.LoadDynamicTypes(singleDynComponent);

            RegisterComponents(container);
            SimpleInjectorWebAPIUtil.RegisterWebApiControllers(container);

            // Register the dependency resolver.
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            // This is an extension method from the integration package as well.
            container.RegisterMvcAttributeFilterProvider();

            // Verify the container configuration
            container.Verify();

            // prepare the script service to be able to reload the container
            var scriptService = SimpleInjectorGenericFactory.Instance.GetObject<ScriptsService>(typeof(ScriptsService));
            scriptService.Reloader = new ContainerReloader();
            scriptService.ContainerReloaded(singleDynComponent);

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

                    var name = SimpleInjectorGenericFactory.BuildRegisterName(typeToRegister);
                    var finalType = typeToRegister;
                    if (DynamicScannerHelper.DynTypes.ContainsKey(name)) {
                        var record = DynamicScannerHelper.DynTypes[name];
                        Log.Debug(string.Format(DynReplaceMsg, name, record.Name));
                        finalType = record.Type;
                        SimpleInjectorGenericFactory.RegisterDynOriginalType(typeToRegister, name);
                    }

                    var attributes = finalType.GetAllAttributes<ComponentAttribute>();
                    var attr = attributes.FirstOrDefault();
                    var reg = Lifestyle.Singleton.CreateRegistration(finalType, container);
                    if (attr != null) {
                        SimpleInjectoScannerUtil.RegisterFromAttribute(attr, tempDict, reg);
                    }

                    var overridingAnnotation = finalType.GetCustomAttribute(typeof(OverridingComponentAttribute));
                    if (overridingAnnotation != null && finalType.BaseType != null) {
                        SimpleInjectoScannerUtil.RegisterOverridingBaseClass(container, ApplicationConfiguration.ClientName, (OverridingComponentAttribute)overridingAnnotation, finalType, reg, name);
                    }

                    RegisterFromInterfaces(typeToRegister, tempDict, reg);
                    if (overridingAnnotation == null && attr == null) {
                        RegisterClassItSelf(container, typeToRegister, reg, name);
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

        private static void RegisterClassItSelf(Container container, Type registration, Registration reg, string name) {
            if (!registration.IsPublic && !registration.IsNestedPublic) {
                return;
            }

            if (SimpleInjectorGenericFactory.ContainsService(name)) {
                Log.DebugOrInfoFormat("ignoring type {0} due to presence of existing overriding", registration.Name);
                return;
            }

            container.AddRegistration(registration, reg);
            SimpleInjectorGenericFactory.RegisterNameAndType(registration, name);
        }

        private static void RegisterFromInterfaces(Type registration, IDictionary<Type, IList<Registration>> tempDict, Registration reg) {
            foreach (var type in registration.GetInterfaces().Where(type => typeof(IComponent).IsAssignableFrom(type))) {
                if (!tempDict.ContainsKey(type)) {
                    tempDict.Add(type, new List<Registration>());
                }
                tempDict[type].Add(reg);
            }
        }
    }
}
