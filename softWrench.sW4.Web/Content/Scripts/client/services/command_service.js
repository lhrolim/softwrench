(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('commandService', [
        "$q", "i18NService", "$injector", "expressionService", "contextService", "schemaService", "modalService", "applicationService", "$log", "alertService", "commandCommonsService",
        function ($q, i18NService, $injector, expressionService, contextService, schemaService, modalService, applicationService, $log, alertService, commandCommonsService) {

    return {
        commandLabel: function (schema, id, defaultValue) {
            const commandSchema = schema.commandSchema;
            if (schema.properties != null && id == "cancel" && schema.properties['detail.cancel.lbl'] != null) {
                const value = schema.properties['detail.cancel.lbl'];
                return i18NService.get18nValue('general.' + value.toLowerCase(), value);
            }
            if (!commandSchema.hasDeclaration) {
                return defaultValue;
            }
            const idx = $.inArray(id, commandSchema.toInclude);
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

        // tabId parameter can be used in showexpression, do not remove it
        isCommandHidden: function (datamap, schema, command, tabId) {
            return commandCommonsService.isCommandHidden(datamap, schema, command, tabId);
        },

        isCommandEnabled: function (datamap, schema, command, tabId) {
            const expression = command.showExpression;
            if (expression == undefined || expression === "") {
                return false;
            }
            const expressionToEval = expressionService.getExpression(expression, datamap);
            return !eval(expressionToEval);
        },

        doExecuteService: function (scope, clientFunction, command,overridenDatamap) {
            const service = $injector.getInstance(command.service);
            if (service == undefined) {
                //this should not happen, it indicates a metadata misconfiguration
                return $q.when();
            }
            const method = service[clientFunction];
            if (method == null) {
                $log.get("commandService#doExecuteService").warn('method {0} not found on service {1}'.format(clientFunction, command.service));
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

            return $q.when(method.apply(service, args));
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
                const ob = schemaService.parseAppAndSchema(command.nextSchemaId);
                let application = ob.application;
                if (!application) {
                    application = scope.schema.applicationName;
                }
                const id = schemaService.getId(scope.datamap, scope.schema);
                var that = this;

                applicationService.getApplicationWithInitialDataPromise(application, ob.schemaId, { id: id }, scope.datamap).then(function (result) {
                    const title = result.data.schema.title;
                    if (result.data.extraParameters['exception']) {
                        alertService.alert(result.data.extraParameters['exception']);
                        return;
                    }


                    modalService.show(result.data.schema, result.data.resultObject, { title: title, cssclass: modalclass }, function (modalData) {
                        that.doExecuteService(scope, clientFunction, command,modalData).then(function () {
                            modalService.hide();
                        });
                    });
                });

            }

            return this.doExecuteService(scope, clientFunction, command).catch(err=> log.debug(err));
        },

        isServiceMethod: function (fullName) {
            if (!fullName) {
                return false;
            }
            if (fullName.startsWith("@")) {
                return false;
            }
            if (fullName.startsWith("service:")) {
                return true;
            }

            const idx = fullName.indexOf(".");
            const serviceName = fullName.substring(0, idx);
            if (!serviceName) {
                return false;
            }
            const service = $injector.getInstance(serviceName);
            if (!service) {
                return false;
            }
            const methodName = fullName.substring(idx + 1);
            const method = service[methodName];
            if (!method) {
                return false;
            }

            return true;
        },

        //TODO: make it generic
        executeClickCustomCommand: function (fullServiceName, rowdm, column, schema, panelid, newValue) {
            if (fullServiceName.startsWith("service:")) {
                fullServiceName = fullServiceName.substr(8);
            }

            const idx = fullServiceName.indexOf(".");
            const serviceName = fullServiceName.substring(0, idx);
            const methodName = fullServiceName.substring(idx + 1);
            const service = $injector.getInstance(serviceName);
            if (service == undefined) {
                const errost = "missing clicking service".format(serviceName);
                throw new Error(errost);
            }
            const method = service[methodName];
            const args = [];
            args.push(rowdm);
            args.push(column);
            args.push(schema);
            args.push(panelid);
            args.push(newValue);

            return method.apply(service, args);
        },

        getBarCommands: function (schema, position) {
            return commandCommonsService.getBarCommands(schema, position);
        },

        evalToggleExpression: function (datamap, expression) {
            if (expression == undefined || expression === "") {
                return false;
            }
            return  expressionService.evaluate(expression, datamap);
        }

    };

}]);

})(angular);