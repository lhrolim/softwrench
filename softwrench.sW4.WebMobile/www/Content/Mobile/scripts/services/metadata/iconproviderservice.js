(function (angular) {
    "use strict";

    function iconProviderService($log, crudContextService, $state, $injector, offlineSchemaService, statuscolorService) {
        //#region Utils

        function getCurrentSchema() {
            const state = $state.current.name;
            if (state.startsWith("main.crudlist")) {
                return crudContextService.currentListSchema();
            } else if (state.startsWith("main.cruddetail")) {
                return crudContextService.currentDetailSchema();
            }
            return null;
        }

        function currentIconProvider() {
            const schema = getCurrentSchema();
            if (!schema || !schema.properties) return null;
            const iconProviderName = schema.properties["mobile.icon.provider"];
            if (!iconProviderName) return null;

            const log = $log.get("iconProviderService#currentIconProvider", ["icon", "metadata"]);
            if (!$injector.has(iconProviderName)) {
                log.warn(`service '${iconProviderName}' not found`);
                return null;
            }

            log.debug(`using '${iconProviderName}' as icon provider for schema '${schema.schemaId}' in application '${schema.applicationName}'`);
            return $injector.get(iconProviderName);
        }

        function delegateToDeclaredServiceProxy(method) {
            const methodName = method.name;
            return function () {
                const iconService = currentIconProvider();
                if (!iconService || !iconService[methodName] || !angular.isFunction(iconService[methodName])) {
                    return method.apply(null, arguments);
                }
                const serviceResult = iconService[methodName].apply(iconService, arguments);
                return !serviceResult
                    ? method.apply(null, arguments)
                    : serviceResult;
            };
        }

        //#endregion

        //#region Public methods

        function getIconColor(item) {
            const datamap = item.datamap;
            const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");
            if (!displayable || !displayable.attribute || displayable.attribute === "status") {
                return statuscolorService.getColor(datamap["status"], crudContextService.currentApplicationName());
            }

            if (displayable.attribute === "wopriority") {
                return statuscolorService.getPriorityColor(datamap[displayable.attribute]);
            }

            return "#777";
        }

        function getTextColor(item) {
            const background = getIconColor(item);
            return background === "white" || background === "transparent" ? "black" : "white";
        }

        function getIconText(item) {
            if (item.isDirty || item.pending || item.hasProblem) {
                return "";
            }

            const datamap = item.datamap;
            const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");

            if (!displayable || !displayable.attribute || displayable.attribute === "status") {
                const status = datamap["status"];
                return status == null ? "N" : status.charAt(0);
            }

            var value = datamap[displayable.attribute];

            if (displayable.attribute === "wopriority") {
                item.icon = value ? null : "flag";
                return value ? value.substring(0, 1) : "";
            }

            if (!value) {
                return null;
            }
            value += "";
            return value.substring(0, 1);
        }

        function getIconIcon(item) {
            if (item.pending) {
                return "cloud";
            }
            if (item.hasProblem) {
                return "exclamation-triangle";
            }
            if (item.isDirty) {
                return "refresh";
            }

            const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");
            const value = item.datamap[displayable.attribute];
            if (displayable.attribute === "wopriority" && !value) {
                return "flag";
            }

            return null;
        }

        //#endregion

        //#region Service Instance
        const service = {
            getIconColor: delegateToDeclaredServiceProxy(getIconColor),
            getTextColor: delegateToDeclaredServiceProxy(getTextColor),
            getIconText: delegateToDeclaredServiceProxy(getIconText),
            getIconIcon: delegateToDeclaredServiceProxy(getIconIcon)
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services")
        .factory("iconProviderService", ["$log", "crudContextService", "$state", "$injector", "offlineSchemaService", "statuscolorService", iconProviderService]);

    //#endregion

})(angular);