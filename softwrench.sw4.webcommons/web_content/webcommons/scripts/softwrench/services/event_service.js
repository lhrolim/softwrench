(function (modules) {
    "use strict";

    modules.webcommons.factory('eventService', ["$log", "$q", "dispatcherService", "crudContextHolderService", "expressionService", function ($log, $q, dispatcherService, crudContextHolderService, expressionService) {
        const loadEvent = function (schemaOrDisplayable, eventName) {
            if (!schemaOrDisplayable || schemaOrDisplayable.events === undefined) {
                return null;
            }
            return schemaOrDisplayable.events[eventName];
        }

        const logMsg = function (type, msg) {
            const prefix = type ? type + " - " : "";
            return prefix + msg;
        }

        const execute = function (event, eventParameters) {
            if (!event) {
                return null;
            }

            const type = event;
            const log = $log.getInstance("eventService#execute");

            const hasService = event.service && event.method;
            if (!hasService && !event.expression) {
                log.warn(logMsg(type, "the event has not service and method or expression."));
                return null;
            }

            if (hasService) {
                const fn = dispatcherService.loadService(event.service, event.method);
                if (!fn) {
                    log.warn(logMsg(type, `method ${event.method} from service ${event.service} not found.`));
                    return null;
                }
                log.debug(logMsg(type, `invoking method ${event.method} from service ${event.service}.`));
                const serviceParameters = Object.keys(eventParameters).map(key => eventParameters[key]);
                return fn.apply(null, serviceParameters);
            }

            var expression = event.expression;
            log.debug(logMsg(type, `evaluating the expression ${expression}.`));

            //constant evaluations
            if (expression === "true" || expression === true) {
                return true;
            }
            if (expression == null || expression === "false" || expression === false) {
                return false;
            }

            expression = expression.replace(/\$e/g, "eventParameters");

            try {
                return eval(expression);
            } catch (e) {
                log.error(e);
                return false;
            }
        }

        return {
            dispatch: function(eventName, holder, parameters) {
                const event = loadEvent(holder, eventName);
                if (!event) {
                    return null;
                }
                return execute(event, parameters);
            },
            onload: function (scope, schema, datamap, parameters) {
                const event = loadEvent(schema, "onload");
                if (!event) {
                    return;
                }
                parameters.tabid = parameters.tabid || crudContextHolderService.getActiveTab();
                parameters.applicationName = parameters.applicationName || crudContextHolderService.currentApplicationName();
                parameters.schemaId = parameters.schemaId || crudContextHolderService.currentSchema().schemaId;
                execute(event, { scope, schema, datamap, parameters });
            },
            onviewdetail: function (schema, parameters) {
                const event = loadEvent(schema, "onviewdetail");
                if (!event) {
                    return;
                }
                execute(event, { parameters });
            },
            beforesubmit_onvalidation: function (schema, datamap, parameters) {
                const event = loadEvent(schema, "beforesubmit.onvalidation");
                if (!event) {
                    return [];
                }
                return execute(event, { schema, datamap, parameters });
            },
            beforesubmit_prevalidation: function (schema, datamap, parameters) {
                const event = loadEvent(schema, "beforesubmit.prevalidation");
                if (!event) {
                    return null;
                }
                return execute(event, { schema, datamap, parameters });
            },
            beforesubmit_postvalidation: function (schema, datamap, parameters) {
                const event = loadEvent(schema, "beforesubmit.postvalidation");
                if (!event) {
                    return $q.when(null);
                }
                return $q.when(execute(event, { schema, datamap, parameters }));
            },
            onedit_validation: function (datamap, schema) {
                const event = loadEvent(schema, "onedit.validation");
                if (!event) {
                    return $q.when(null);
                }
                return execute(event, { datamap, schema });
            },
            onremoval_validation: function (datamap, schema) {
                const event = loadEvent(schema, "onremoval.validation");
                if (!event) {
                    return $q.when(null);
                }
                return execute(event, { datamap, schema });
            },
            providerloaded: function (displayable, providerLoadedParameters) {
                const event = loadEvent(displayable, "providerloaded");
                if (!event) {
                    return null;
                }
                return execute(event, { providerLoadedParameters });
            },
            afterchange: function (displayable, parameters) {
                const event = loadEvent(displayable, "afterchange");
                if (!event) {
                    return null;
                }
                return execute(event, { parameters });
            },
            beforechange: function (displayable, event) {
                const metadataEvent = loadEvent(displayable, "beforechange");
                if (!metadataEvent) {
                    return true;
                }
                return execute(metadataEvent, { event });
            },
            onassociationsloaded: function (schema) {
                const metadataEvent = loadEvent(schema, "onassociationsloaded");
                if (!metadataEvent) {
                    return null;
                }
                return execute(metadataEvent, {});
            },
            onschemafullyloaded: function (schema) {
                const metadataEvent = loadEvent(schema, "onschemafullyloaded");
                if (!metadataEvent) {
                    return null;
                }
                return execute(metadataEvent, {});
            }
        };
    }]);
})(modules);