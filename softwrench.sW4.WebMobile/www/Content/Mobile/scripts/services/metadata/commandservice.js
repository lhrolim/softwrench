(function (angular, mobileServices, _) {
    "use strict";

    function offlineCommandService(dao, $q, $log, commandCommonsService, $injector) {
        //#region Utils
        var cachedCommands = {};
        //#endregion

        //#region Public methods

        function initAndCacheFromDataBase() {
            return dao.findAll("CommandBar").then(bars => {
                var commandBars = {};
                if (!bars || bars.length <= 0) {
                    return commandBars;
                }
                angular.forEach(bars, (bar) => commandBars[bar.key] = bar.data);
                return cachedCommands = commandBars;
            });
        }

        function updateCommandBars(bars) {
            $log.get("commandService#updateCommandBars", ["metadata", "commands"]).debug("updating commandbars", bars);
            return !bars || _.isEmpty(bars)
                ? $q.when()
                : dao.deleteTable("CommandBar")
                    .then(() => $q.all(Object.keys(bars).map(key => dao.instantiate("CommandBar", { key, data: bars[key] }))))
                    .then(b => dao.bulkSave(b))
                    .then(b => cachedCommands = bars);
        }

        function getCommands(schema, position) {
            return commandCommonsService.getCommands(schema, position, cachedCommands);
        }

        function executeCommand(command, schema, datamap) {
            const log = $log.get("offlineCommandService#executeCommand", ["commands"]);
            log.debug(`Executing command ${command.service}.${command.method}`);
            const q = $q.defer();
            try {
                const commandService = $injector.getInstance(command.service);
                q.resolve(commandService[command.method](schema, datamap));
            } catch (e) {
                const error = new Error(`Failed to execute command ${command.service}.${command.method} due to ${e}`);
                log.error(error);
                q.reject(error);
            }
            return q.promise;
        }

        function isCommandHidden(datamap, schema, command) {
            return commandCommonsService.isCommandHidden(datamap, schema, command);
        }

        //#endregion

        //#region Service Instance
        const service = {
            updateCommandBars,
            getCommands,
            initAndCacheFromDataBase,
            executeCommand,
            isCommandHidden
        };
        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("offlineCommandService", ["swdbDAO", "$q", "$log", "commandCommonsService", "$injector", offlineCommandService]);

    //#endregion

})(angular, mobileServices, _);