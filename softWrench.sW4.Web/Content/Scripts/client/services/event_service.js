/*
 */
var app = angular.module('sw_layout');

app.factory('eventService', function ($log, dispatcherService) {
    var loadEvent = function(eventholder, eventName) {
        if (eventholder.events === undefined) {
            return null;
        }
        var event = eventholder.events[eventName];
        if (!event) {
            return null;
        }

        var service = event['service'];
        var method = event['method'];

        return dispatcherService.loadService(service, method);
    };
    return {
        onload: function(scope, eventholder, datamap, parameters) {
            var fn = loadEvent(eventholder, 'onload');
            if (!fn) {
                return;
            }
            fn(scope, eventholder, datamap, parameters);
        },
        onviewdetail: function (eventholder, parameters) {
            var fn = loadEvent(eventholder, 'onviewdetail');
            if (!fn) {
                return;
            }
            fn(parameters);
        },
        loadEvent: function (eventholder, eventName) {
            return loadEvent(eventholder, eventName);
        }
    };

});


