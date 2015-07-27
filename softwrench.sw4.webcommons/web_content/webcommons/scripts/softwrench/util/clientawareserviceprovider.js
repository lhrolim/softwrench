﻿//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/

modules.webcommons.run(['$injector', 'contextService','$log', enhanceInjector]);


function enhanceInjector($injector, contextService,$log) {

    this.clientfactory = function(serviceName) {
        var client = contextService.client();
        return this.factory(client + "." + serviceName);
    }

    $injector.getInstance = function(serviceName) {
        return doGet(serviceName);
    };


    function doGet(serviceName) {
        var client = contextService.client();
//        angular.

        try {
            var clientService = $injector.get(client + "." + serviceName);
            var baseService = $injector.get(serviceName);
            if (clientService != null) {
                return clientService;
            }
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
