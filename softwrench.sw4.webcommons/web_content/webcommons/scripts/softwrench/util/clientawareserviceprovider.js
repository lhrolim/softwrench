//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/
(function (angular, modules) {
    "use strict";

    modules.webcommons.run(["$injector", "contextService", "$log", enhanceInjector]);


    function enhanceInjector($injector, contextService, $log) {

        $injector.getInstance = function (serviceName) {
            var client = contextService.client();
            var clientServiceName = client + "." + serviceName;
            // has client specif implementation
            if ($injector.has(clientServiceName)) {
                $log.debug("Client specific service", clientServiceName, "found.");
                var clientService = $injector.get(clientServiceName);
                // delegate 'super' methods to the base implementation
                if ($injector.has(serviceName)) {
                    $log.debug(serviceName, "base implementation found. Applying base property bindings to", clientServiceName);
                    var baseService = $injector.get(serviceName);
                    angular.forEach(baseService, function (property, name) {
                        // skip useless (prototypically inherited from JS runtime) properties and overriden properties
                        if (!baseService.hasOwnProperty(name) || clientService.hasOwnProperty(name)) return;
                        var overridenProperty = angular.isFunction(property) ? property.bind(baseService) : property;
                        clientService[name] = overridenProperty;
                    });
                }
                return clientService;
            }
            $log.debug("Client specific service", clientServiceName, "not found. Attempting to instantiate base service", serviceName);
            // if there's no base implementation let the error go up
            return $injector.get(serviceName);
        };
    
    };

})(angular, modules);