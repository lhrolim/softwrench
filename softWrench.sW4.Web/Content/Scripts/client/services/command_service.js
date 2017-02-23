var app = angular.module('sw_layout');

app.factory('commandService', function (i18NService, $injector, expressionService) {

    "ngInject";

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
            var expressionToEval = expressionService.getExpression(expression,datamap);
            return !eval(expressionToEval);
        },

        isCommandEnabled: function (datamap, schema, command, tabId) {
            var expression = command.enableExpression;
            if (expression == undefined || expression == "") {
                return true;
            }
            var expressionToEval = expressionService.getExpression(expression, datamap);
            var returnValue = eval(expressionToEval);
            return returnValue;
        },

        doCommand: function (scope, command) {
            var clientFunction = command.method;
            if (typeof (clientFunction) === 'function') {
                clientFunction();
                return;
            }
            if (nullOrUndef(clientFunction)) {
                clientFunction = command.id;
            }
            if (command.service == undefined) {
                return;
            }

            var service = $injector.get(command.service);
            if (service == undefined) {
                //this should not happen, it indicates a metadata misconfiguration
                return;
            }

            var method = service[clientFunction];

            var args = [];
            if (command.scopeParameters != null) {
                $.each(command.scopeParameters, function (key, parameterName) {
                    args.push(scope[parameterName]);
                });
            }

            method.apply(this, args);
            return;

        },
        //TODO: make it generic
        executeClickCustomCommand: function (fullServiceName, rowdm,column) {
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

            method.apply(this, args);
            return;

        }

    };

});


