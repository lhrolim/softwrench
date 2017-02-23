/*
 */
var app = angular.module('sw_layout');

app.factory('dispatcherService', function ($injector, $log) {

    "ngInject";

    var loadService = function (service, method) {
        var log = $log.getInstance('dispatcherService#loadService');

        if (service === undefined || method === undefined) {
            return null;
        }

        var dispatcher = $injector.get(service);
        if (!dispatcher) {
            log.warn('Service {0} missing '.format(service));
            return null;
        }
        var fn = dispatcher[method];
        if (!fn) {
            log.warn('Method {0} not found on service {1}'.format(method, service));
            return null;
        }
        return fn;
    };
    return {
        loadService: function (service, method) {
            return loadService(service, method);
        }
    };

});

