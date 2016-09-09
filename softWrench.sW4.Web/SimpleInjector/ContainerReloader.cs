﻿using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Dynamic;

namespace softWrench.sW4.Web.SimpleInjector {
    public class ContainerReloader : IContainerReloader {
        public void ReloadContainer(ScriptEntry singleDynComponent) {
            SimpleInjectorGenericFactory.ClearCache();
            var container = SimpleInjectorScanner.InitDIController(singleDynComponent);
            var dispatcher = (IEventDispatcher)container.GetInstance(typeof(IEventDispatcher));
            dispatcher.Dispatch(new ContainerReloadedEvent());
        }
    }
}