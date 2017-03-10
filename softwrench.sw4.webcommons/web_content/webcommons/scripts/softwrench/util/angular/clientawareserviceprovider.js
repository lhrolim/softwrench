//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/
(function (angular, modules) {
    "use strict";

   
    modules.webcommons.run(["$injector", "contextService","dynamicScriptsCacheService", "$log", enhanceInjector]);


        function enhanceInjector($injector, contextService,dynamicScriptsCacheService, $log) {

            var original = $injector.invoke;

            $injector.getInstance = function (serviceName) {
                const client = contextService.client();
                const clientServiceName = client + "." + serviceName;
                const log = $log.get("clientawareserviceprovider", ["services"]);
                // has client specific implementation
                if ($injector.has(clientServiceName)) {
                    log.debug("Client specific service", clientServiceName, "found.");
                    var clientService = $injector.get(clientServiceName);
                    // delegate 'super' methods to the base implementation
                    if ($injector.has(serviceName)) {
                        log.debug(serviceName, "base implementation found. Applying base property bindings to", clientServiceName);
                        var baseService = $injector.get(serviceName);
                        angular.forEach(baseService, function (property, name) {
                            // skip useless (prototypically inherited from JS runtime) properties and overriden properties
                            if (!baseService.hasOwnProperty(name) || clientService.hasOwnProperty(name)) return;
                            const overridenProperty = angular.isFunction(property) ? property.bind(baseService) : property;
                            clientService[name] = overridenProperty;
                        });
                        clientService.__super__ = baseService;
                    }
                    return clientService;
                }
                if (client && client !== "otb") {
                    log.debug("Client specific service", clientServiceName, "not found. Attempting to instantiate base service", serviceName);
                }
                // if there's no base implementation let the error go up
                return $injector.get(serviceName);
            };


            $injector.invoke = function (fn, self, locals, serviceName) {
                const customService = dynamicScriptsCacheService.useCustomServiceIfPresent(serviceName);
                if (customService != null) {
                    $log.get("clientawareserviceprovider", ["angular", "services"]).debug(`applying custom service for ${serviceName}`);
                    return customService;
                }

                return original(fn, self, locals, serviceName);
            }

        };

    

})(angular, modules);