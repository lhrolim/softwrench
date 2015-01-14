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
        loadEvent: function (eventholder, eventName) {
            return loadEvent(eventholder, eventName);
        },
        onload: function(eventholder, datamap, parameters) {
            var fn = loadEvent(eventholder, 'onload');
            if (!fn) {
                return;
            }
            fn(eventholder, datamap, parameters);
        },
        onviewdetail: function (eventholder, parameters) {
            var fn = loadEvent(eventholder, 'onviewdetail');
            if (!fn) {
                return;
            }
            fn(parameters);
        },
        onvalidation: function (eventholder, datamap, parameters) {
            var fn = loadEvent(eventholder, 'onvalidation');
            if (!fn) {
                return [];
            }
            return fn(eventholder, datamap, parameters);
        },
        beforesubmit_confirmation: function (eventholder, datamap, parameters) {
            var fn = loadEvent(eventholder, 'beforesubmit.confirmation');
            if (!fn) {
                return;
            }
            fn(eventholder, datamap, parameters);
        },
        beforesubmit_transformation: function (eventholder, datamap, parameters) {
            var fn = loadEvent(eventholder, 'beforesubmit.transformation');
            if (!fn) {
                return null;
            }
            var transformedDatamap = angular.copy(datamap);
            return fn(eventholder, transformedDatamap.fields, parameters);
        }
        
    };

});


