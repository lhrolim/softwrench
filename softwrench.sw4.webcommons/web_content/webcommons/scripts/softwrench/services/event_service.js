(function (modules) {
    "use strict";

    modules.webcommons.factory('eventService', ["$log", "dispatcherService", "crudContextHolderService", function ($log, dispatcherService, crudContextHolderService) {
        var loadEvent = function (schema, eventName) {
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


            dispatchEvent: function (schema, eventName, parameters) {
                var fn = loadEvent(schema, eventName);
                if (fn) {
                    if (parameters) {
                        return fn(parameters);
                    }
                    return fn();
                }
            },

            onload: function (scope, schema, datamap, parameters) {
                //TODO:REVIEW
                var fn = loadEvent(schema, 'onload');
                if (!fn) {
                    return;
                }
                parameters.tabid = parameters.tabid || crudContextHolderService.getActiveTab();
                parameters.applicationName = parameters.applicationName || crudContextHolderService.currentApplicationName();
                parameters.schemaId = parameters.schemaId || crudContextHolderService.currentSchema().schemaId;
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

    }]);

})(modules);