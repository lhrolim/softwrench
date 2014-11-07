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
        onload: function(schema, datamap, parameters) {
            var fn = loadEvent(schema, 'onload');
            if (!fn) {
                return;
            }
            fn(schema, datamap, parameters);
        },
        onviewdetail: function(schema, parameters) {
            var fn = loadEvent(schema, 'onviewdetail');
            if (!fn) {
                return;
            }
            fn(parameters);
        },
        loadEvent: function(schema, eventName) {
            return loadEvent(schema, eventName);
        }
    };

});


