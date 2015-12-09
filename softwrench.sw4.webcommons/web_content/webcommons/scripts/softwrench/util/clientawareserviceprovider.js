//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/
(function (angular, modules) {
    "use strict";

modules.webcommons.run(['$injector', 'contextService', '$log', enhanceInjector]);


function enhanceInjector($injector, contextService, $log) {

    $injector.getInstance = function (serviceName) {
        try {
            return doGet(serviceName);
        } catch (e) {
            $log.error(e);
            return null;
        }
    };

    function doGet(serviceName) {
        var client = contextService.client();
        var clientServiceName = client + "." + serviceName;
        // has client specif implementation
        if ($injector.has(clientServiceName)) {
            var clientService = $injector.get(clientServiceName);
            // delegate 'super' methods to the base implementation
            if ($injector.has(serviceName)) {
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
        // has base service implementation
        if ($injector.has(serviceName)) {
            return $injector.get(serviceName);
        }
        // has nothing...
        return null;
    }
};

})(angular, modules);