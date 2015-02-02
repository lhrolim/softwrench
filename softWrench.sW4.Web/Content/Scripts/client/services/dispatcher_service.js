/*
 */
var app = angular.module('sw_layout');

app.factory('dispatcherService', function ($injector, $log) {
    var loadService = function(service, method, parameters) {
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

        if (parameters != null) {
            var args = [];
            for (var i = 0; i < parametrs.length; i++) {
                args.push(parameters[i]);
            }
            fn = method.apply(this, args);
        }

        if (!fn) {
            log.warn('Method {0} not found on service {1}'.format(method, service));
            return null;
        }
        return fn;
    };
    return {
        loadService: function(service, method, parameters) {
            return loadService(service, method, parameters);
        }
    };

});


