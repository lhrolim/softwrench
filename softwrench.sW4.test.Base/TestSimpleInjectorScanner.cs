﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using cts.commons.simpleinjector;
using cts.commons.Util;
using log4net;
using Moq;
using softWrench.sW4.Util;
using SimpleInjector;
using softwrench.sW4.TestBase.Extensions;

namespace softwrench.sW4.TestBase {
    public class TestSimpleInjectorScanner {

        private static readonly ILog Log = LogManager.GetLogger(typeof(TestSimpleInjectorScanner));

        private readonly IDictionary<Type, IList<Registration>> _tempDict = new Dictionary<Type, IList<Registration>>();
        private readonly List<Type> _singletonMockTypes = new List<Type>();

        public Container Container {
            get; private set;
        }

        public TestSimpleInjectorScanner() {
            Container = new Container();
            Container.Options.AllowOverridingRegistrations = true;
        }


        public void InitDIController() {
            var before = Stopwatch.StartNew();
            SimpleInjectorGenericFactory.ClearCache();
            RegisterComponents(Container);
            Log.Debug(LoggingUtil.BaseDurationMessage("SimpleInjector context initialized in {0}", before));
            new SimpleInjectorGenericFactory(Container);
//            var dispatcher =
//                SimpleInjectorGenericFactory.Instance.GetObject<SimpleInjectorDomainEventDispatcher>(
//                    typeof (SimpleInjectorDomainEventDispatcher));
//            dispatcher.Dispatch(new ApplicationStartedEvent());
        }

        public void ResgisterSingletonMock<T>(Mock<T> mock) where T : class, IComponent {
            var mockType = typeof(T);
            if (_singletonMockTypes.Contains(mockType)) {
                return;
            }
            _singletonMockTypes.Add(mockType);
            Container.RegisterSingle(mockType, MockInstanceCreator(mock));
            SimpleInjectorGenericFactory.RegisterNameAndType(mockType);

            foreach (var type in typeof(T).GetInterfaces().Where(type => typeof(IComponent).IsAssignableFrom(type)).Where(type => !_singletonMockTypes.Contains(type))) {
                _singletonMockTypes.Add(type);
                Container.RegisterSingle(type, MockInstanceCreator(mock));
                SimpleInjectorGenericFactory.RegisterNameAndType(type);
            }
        }

        private static Func<object> MockInstanceCreator<T>(Mock<T> mock) where T : class, IComponent
        {
            return () => mock.Object;
        }

        public void ResgisterSingletonObject<T>(T obj) where T : class, IComponent {
            var mockType = typeof(T);
            if (_singletonMockTypes.Contains(mockType)) {
                return;
            }
            _singletonMockTypes.Add(mockType);
            Container.RegisterSingle(mockType, () => obj);
            SimpleInjectorGenericFactory.RegisterNameAndType(mockType);

            foreach (var type in typeof(T).GetInterfaces().Where(type => typeof(IComponent).IsAssignableFrom(type)).Where(type => !_singletonMockTypes.Contains(type))) {
                _singletonMockTypes.Add(type);
                Container.RegisterSingle(type, () => obj);
                SimpleInjectorGenericFactory.RegisterNameAndType(type);
            }
        }

        private void RegisterComponents(Container container) {
            var assemblies = AssemblyLocator.GetSWAssemblies();
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
                    var name = SimpleInjectorGenericFactory.BuildRegisterName(registration);
                    var attributes = registration.GetAllAttributes<ComponentAttribute>();
                    var attr = attributes.FirstOrDefault();
                    var reg = Lifestyle.Singleton.CreateRegistration(registration, container);
                    if (attr != null) {
                        SimpleInjectoScannerUtil.RegisterFromAttribute(attr,_tempDict, reg);
                    }
                    var overridingAnnotation = registration.GetCustomAttribute(typeof(OverridingComponentAttribute));
                    if (overridingAnnotation != null && registration.BaseType != null) {
                        SimpleInjectoScannerUtil.RegisterOverridingBaseClass(container, ApplicationConfiguration.ClientName, (OverridingComponentAttribute)overridingAnnotation, registration, reg, name);
                    }
                    RegisterFromInterfaces(registration, reg);
                    RegisterClassItSelf(container, registration, reg);
                }
            }
            foreach (var entry in _tempDict) {
                var coll = entry.Value;
                var serviceType = entry.Key;
                if (typeof(ISingletonComponent).IsAssignableFrom(serviceType)) {
                    if (_singletonMockTypes.Contains(serviceType)) {
                        continue;
                    }
                    container.AddRegistration(serviceType, coll.FirstOrDefault());
                    SimpleInjectorGenericFactory.RegisterNameAndType(serviceType);
                } else {
                    container.RegisterAll(serviceType, coll);
                }
            }
        }

        private void RegisterClassItSelf(Container container, Type registration, Registration reg) {
            if (registration.IsPublic) {
                if (_singletonMockTypes.Contains(registration)) {
                    return;
                }
                container.AddRegistration(registration, reg);
                SimpleInjectorGenericFactory.RegisterNameAndType(registration);
            }
        }

        private void RegisterFromInterfaces(Type registration, Registration reg) {
            foreach (var type in registration.GetInterfaces().Where(type => typeof(IComponent).IsAssignableFrom(type))) {
                AddToTemp(type, reg);
            }
        }

        private void AddToTemp(Type type, Registration reg) {
            if (!_tempDict.ContainsKey(type)) {
                _tempDict.Add(type, new List<Registration>());
            }
            _tempDict[type].Add(reg);
        }
    }
}