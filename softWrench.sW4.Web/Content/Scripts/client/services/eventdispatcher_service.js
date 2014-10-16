/*
 */
var app = angular.module('sw_layout');

app.factory('eventdispatcherService', function ($injector, $log) {
    var loadService = function (schema, eventName) {
        var log = $log.getInstance('eventdispatcherService#loadService')
        var event = schema.events[eventName]
        if (!event) {
            return null;
        }
        var service = $injector.get(event['service'])
        if (!service) {
            log.warn('Service {0} missing '.format(event['service']))
            return null;
        }
        var fn = service[event['method']]
        if (!fn) {
            log.warn('Method {0} not found on service {1}'.format(event['method'], event['service']))
            return null;
        }
        return fn;
    }
    return {
        onload: function (schema, datamap) {
            var fn = loadService(schema, 'onload');
            if (!fn) {
                return;
            }
            fn(schema, datamap);
        },
        onviewdetail: function(schema, parameters) {
            var fn = loadService(schema, 'onviewdetail');
            if (!fn) {
                return;
            }
            fn(parameters);
        }
    }

});


