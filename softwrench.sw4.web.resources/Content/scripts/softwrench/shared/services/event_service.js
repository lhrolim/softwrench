/*
 */
var app = angular.module('sw_layout');

app.factory('eventService', function ($log, dispatcherService) {
    var loadEvent = function(schema, eventName) {
        if (schema.events === undefined) {
            return null;
        }
        var event = schema.events[eventName];
        if (!event) {
            return null;
        }

        var service = event['service'];
        var method = event['method'];
        

        return dispatcherService.loadService(service, method);
    };
    return {
        loadEvent: function (schema, eventName) {
            return loadEvent(schema, eventName);
        },
        onload: function(scope, schema, datamap, parameters) {
            var fn = loadEvent(schema, 'onload');
            if (!fn) {
                return;
            }
            fn(scope, schema, datamap, parameters);
        },
        onviewdetail: function (schema, parameters) {
            var fn = loadEvent(schema, 'onviewdetail');
            if (!fn) {
                return;
            }
            fn(parameters);
        },
        beforesubmit_onvalidation: function (schema, datamap, parameters) {
            var fn = loadEvent(schema, 'beforesubmit.onvalidation');
            if (!fn) {
                return [];
            }
            return fn(schema, datamap, parameters);
        },
        beforesubmit_prevalidation: function (schema, datamap, parameters) {
            var fn = loadEvent(schema, 'beforesubmit.prevalidation');
            if (!fn) {
                return;
            }
            return fn(schema, datamap, parameters);
        },
        beforesubmit_postvalidation: function (schema, datamap, parameters) {
            var fn = loadEvent(schema, 'beforesubmit.postvalidation');
            if (!fn) {
                return null;
            }
            return fn(schema, datamap, parameters);
        }
    };

});


