(function (angular) {
    "use strict";

    function commandCommonsService($log,$q, expressionService, contextService) {
        
        //#region Public methods

        /**
         * Resolves the correct set of commands from the commandBarRegistry 
         * based on the position and the schema.
         * 
         * @param {Schema} schema 
         * @param {String} position 
         * @param {Object} commandBarRegistry dictionary with cached command bars
         * @returns {Array<Command>} commands for the specific position and schema. 
         */
        function getCommands(schema, position, commandBarRegistry) {
            const hasPossibilityOfbeingOverriden = schema.commandSchema.hasDeclaration;
            const bar = commandBarRegistry;
            const fallbackKey = `#${position}`;
            var commandKey = hasPossibilityOfbeingOverriden ? `${schema.applicationName}_${schema.schemaId}_${schema.mode.toLowerCase()}.#${position}` : fallbackKey;
            var commandbar = bar[commandKey];
            if (commandbar == null) {
                if (hasPossibilityOfbeingOverriden && schema.mode.toLocaleLowerCase() !== "none") {
                    //let´s give the none schema a shot
                    commandKey = `${schema.applicationName}_${schema.schemaId}_none.#${position}`;
                    commandbar = bar[commandKey];
                }
                if (commandbar == null) {
                    commandbar = bar[fallbackKey];
                    if (commandbar == null) {
                        $log.get("commandCommonsService#getCommands", ["commands", "metadata"])
                            .warn(`command bar ${commandKey}, and fallback ${fallbackKey} not found`);
                    }
                }
            }
            const commands = commandbar != null ? commandbar.commands : null;
            return commands;
        }

        function getBarCommands(schema, position) {
            if (schema == null) {
                return $q.reject();
            }
            schema.jscache = schema.jscache || {};
            schema.jscache.commandbars = schema.jscache.commandbars || {};
            if (schema.jscache.commandbars[position] !== undefined) {
                //null should be considered as a cache hit also
                return schema.jscache.commandbars[position];
            }
            const bars = contextService.fetchFromContext("commandbars", true);
            const commands = this.getCommands(schema, position, bars);
            schema.jscache.commandbars[position] = commands;

            return commands;
        }

        //tabId parameter can be used in showexpression, do not remove it
        function isCommandHidden(datamap, schema, command, tabId) {
            if (command.remove) {
                return true;
            }
            const expression = command.showExpression;
            if (expression == undefined || expression === "") {
                return false;
            }
            return !expressionService.evaluate(expression, datamap, { schema: schema }, null);
        }

        //#endregion

        //#region Service Instance
        const service = {
            getCommands,
            getBarCommands,
            isCommandHidden
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("webcommons_services").service("commandCommonsService", ["$log", "$q", "expressionService", "contextService", commandCommonsService]);

    //#endregion

})(angular);