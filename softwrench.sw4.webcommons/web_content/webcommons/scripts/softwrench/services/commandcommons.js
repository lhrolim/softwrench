(function (angular) {
    "use strict";

    function commandCommonsService($log) {
        
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

        //#endregion

        //#region Service Instance
        const service = {
            getCommands
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("webcommons_services").factory("commandCommonsService", ["$log", commandCommonsService]);

    //#endregion

})(angular);