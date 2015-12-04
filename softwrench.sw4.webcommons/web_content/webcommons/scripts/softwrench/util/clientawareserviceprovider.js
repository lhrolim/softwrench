//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/
//(function (modules) {
//    "use strict";

modules.webcommons.run(['$injector', 'contextService', '$log', enhanceInjector]);


function enhanceInjector($injector, contextService, $log) {

    $injector.getInstance = function(serviceName) {
        return doGet(serviceName);
    };

    function doGet(serviceName) {
        var client = contextService.client();
//        angular.

        try {
            var clientService = $injector.get(client + "." + serviceName);
            if (clientService != null) {
                return clientService;
            }
            var baseService = $injector.get(serviceName);
            return baseService;
        } catch (e) {
            try {
                return $injector.get(serviceName);
            } catch (e) {
                $log.error(e);
                return null;
            }
        }
    }
};

//})(modules);