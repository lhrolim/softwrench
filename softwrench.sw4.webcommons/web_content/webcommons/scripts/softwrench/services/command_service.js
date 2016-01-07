(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('commandService', [
        "$q", "i18NService", "$injector", "expressionService", "contextService", "schemaService", "modalService", "applicationService", "$log", "alertService", 
        function ($q, i18NService, $injector, expressionService, contextService, schemaService, modalService, applicationService, $log, alertService) {

    return {
        commandLabel: function (schema, id, defaultValue) {
            var commandSchema = schema.commandSchema;
            if (schema.properties != null && id == "cancel" && schema.properties['detail.cancel.lbl'] != null) {
                var value = schema.properties['detail.cancel.lbl'];
                return i18NService.get18nValue('general.' + value.toLowerCase(), value);
            }
            if (!commandSchema.hasDeclaration) {
                return defaultValue;
            }
            var idx = $.inArray(id, commandSchema.toInclude);
            if (idx == -1) {
                return defaultValue;
            }
            return commandSchema.toInclude[idx].label;
        },

        shouldDisplayCommand: function (commandSchema, id) {
            if (commandSchema.ignoreUndeclaredCommands) {
                return false;
            }
            return $.inArray(id, commandSchema.toExclude) == -1 && $.inArray(id, commandSchema.toInclude) == -1;
        },
        //tabId parameter can be used in showexpression, do not remove it
        isCommandHidden: function (datamap, schema, command, tabId) {
            if (command.remove) {
                return true;
            }
            var expression = command.showExpression;
            if (expression == undefined || expression == "") {
                return false;
            }
            var expressionToEval = expressionService.getExpression(expression, datamap);
            return !eval(expressionToEval);
        },

        isCommandEnabled: function (datamap, schema, command, tabId) {
            var expression = command.showExpression;
            if (expression == undefined || expression == "") {
                return false;
            }
            var expressionToEval = expressionService.getExpression(expression, datamap);
            return !eval(expressionToEval);
        },

        doExecuteService: function (scope, clientFunction, command,overridenDatamap) {
            var service = $injector.get(command.service);
            if (service == undefined) {
                //this should not happen, it indicates a metadata misconfiguration
                return $q.when();
            }

            var method = service[clientFunction];
            if (method == null) {
                log.warn('method {0} not found on service {1}'.format(clientFunction, command.service));
                return $q.when();
            }

            var args = [];
            if (command.scopeParameters != null) {
                $.each(command.scopeParameters, function (key, parameterName) {
                    var arg = scope[parameterName];
                    if (parameterName == "datamap" && overridenDatamap) {
                        arg = overridenDatamap;
                    }
                    if (arg || parameterName in scope) {
                        args.push(arg);
                    } else {
                        args.push(parameterName);
                    } 
                    
                });
            }

            return $q.when(method.apply(this, args));
        },

        doCommand: function (scope, command) {
            var log = $log.getInstance("commandService#doCommand");
            var clientFunction = command.method;
            if (typeof (clientFunction) === 'function') {
                clientFunction();
                return;
            }
            if (nullOrUndef(clientFunction)) {
                clientFunction = command.id;
            }

            if ("modal".equalIc(command.stereotype)) {
                //TODO: move this whole thing to modal_service.js extracting a bit
                if (!command.nextSchemaId) {
                    log.warn("missing nextschemaId for command {0} declaration".format(command.id));
                    return;
                }
                var modalclass = 'modalsmall';
                if (command.properties && command.properties['modalclass']) {
                    modalclass = command.properties['modalclass'];
                }
     

                log.debug("executing modal default implementation for command {0}".format(command.id));
                var ob = schemaService.parseAppAndSchema(command.nextSchemaId);
                var application = ob.application;
                if (!application) {
                    application = scope.schema.applicationName;
                }
                var id = schemaService.getId(scope.datamap, scope.schema);
                var that = this;

                applicationService.getApplicationWithInitialDataPromise(application, ob.schemaId, { id: id }, scope.datamap).then(function (result) {
                    var title = result.data.schema.title;
                    if (result.data.extraParameters['exception']) {
                        alertService.alert(result.data.extraParameters['exception']);
                        return;
                    }


                    modalService.show(result.data.schema, result.data.resultObject.fields, { title: title, cssclass: modalclass }, function (modalData) {
                        that.doExecuteService(scope, clientFunction, command,modalData).then(function () {
                            modalService.hide();
                        });
                    });
                });

                return;
            }

            this.doExecuteService(scope, clientFunction, command);
        },

        //TODO: make it generic
        executeClickCustomCommand: function (fullServiceName, rowdm, column, schema) {
            var idx = fullServiceName.indexOf(".");
            var serviceName = fullServiceName.substring(0, idx);
            var methodName = fullServiceName.substring(idx + 1);

            var service = $injector.get(serviceName);
            if (service == undefined) {
                var errost = "missing clicking service".format(serviceName);
                throw new Error(errost);
            }

            var method = service[methodName];

            var args = [];
            args.push(rowdm);
            args.push(column);
            args.push(schema);

            method.apply(this, args);
            return;

        },

        getBarCommands: function (schema, position) {
            schema.jscache = instantiateIfUndefined(schema.jscache);
            schema.jscache.commandbars = instantiateIfUndefined(schema.jscache.commandbars);
            var log = $log.getInstance("commandService#getBarCommands");
            if (schema.jscache.commandbars[position] !== undefined) {
                //null should be considered as a cache hit also
                return schema.jscache.commandbars[position];
            }
            var hasPossibilityOfbeingOverriden = schema.commandSchema.hasDeclaration;
            var bar = contextService.fetchFromContext("commandbars", true);
            var fallbackKey = "#" + position;
            var commandKey = hasPossibilityOfbeingOverriden ? "{0}_{1}_{2}.#{3}".format(schema.applicationName, schema.schemaId, schema.mode.toLowerCase(), position) : fallbackKey;
            var commandbar = bar[commandKey];
            if (commandbar == null) {
                if (hasPossibilityOfbeingOverriden && schema.mode.toLocaleLowerCase() != "none") {
                    //let´s give the none schema a shot
                    commandKey = "{0}_{1}_{2}.#{3}".format(schema.applicationName, schema.schemaId, "none", position);
                    commandbar = bar[commandKey];
                }
                if (commandbar == null) {
                    commandbar = bar[fallbackKey];
                    if (commandbar == null) {
                        log.warn("command bar {0}, and fallback {1} not found".format(commandKey, fallbackKey));
                    }
                }
            }
            var commands = commandbar != null ? commandbar.commands : null;
            schema.jscache.commandbars[position] = commands;

            return commands;
        }

    };

}]);

})(angular);