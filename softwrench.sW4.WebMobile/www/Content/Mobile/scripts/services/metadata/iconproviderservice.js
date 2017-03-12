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

        function delegateToDeclaredServiceProxy(method, methodName) {
            // const methodName = method.name; --> does not work in uglified scripts
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
            if (!item) {
                return null;
            }

            const datamap = item.datamap;
            const defaultColor = "#808080";

            if (!datamap) {
                return defaultColor;
            }

            const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");

            if (!displayable || !displayable.attribute || displayable.attribute === "status") {
                if (!!datamap) {
                    return statuscolorService.getColor(datamap["status"], crudContextService.currentApplicationName());
                }
            }

            if (!displayable || !displayable.attribute || displayable.attribute === "wopriority") {
                return statuscolorService.getPriorityColor(datamap[displayable.attribute]);
            }

            return defaultColor;
        }

        function getTextColor(item) {
            const background = getIconColor(item);
            return background === "white" || background === "transparent" ? "black" : "white";
        }

        function getIconClass(item) {
            if (!item) {
                return null;
            }

            if (item.hasProblem) {
                return 'hasproblem';
            }

            if (item.isDirty || item[constants.localIdKey]) {
                return 'isdirty';
            }

            if (item.pending) {
                return 'ispending';
            }

            //composition item
            if (!item.application) {
                return crudContextService.tabIcon();
            }

            return null;
        }

        function getIconText(item) {
            if (!item) {
                return null;
            }
            if (item.isDirty || item.pending || item.hasProblem) {
                return "";
            }

            const datamap = item.datamap;

            if (!datamap) {
                return null;
            }

            const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");

            if (!displayable || !displayable.attribute || displayable.attribute === "status") {
                var status = null;

                if (!!datamap) {
                    status = datamap["status"];
                }

                return status == null ? "N" : status.charAt(0);
            }

            var value = datamap[displayable.attribute];

            if (!displayable || !displayable.attribute || displayable.attribute === "wopriority") {
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
            if (!item) {
                return null;
            }

            if (item.hasProblem) {
                return "exclamation-triangle";
            }

            if (item.isDirty || item[constants.localIdKey]) {
                return "refresh";
            }

            if (item.pending) {
                return "cloud";
            }

            const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");
            if (!displayable) {
                return null;
            }

            if (!item.datamap) {
                return null;
            }

            const value = item.datamap[displayable.attribute];
            if (displayable.attribute === "wopriority" && !value) {
                return "flag";
            }

            return null;
        }

        //#endregion

        //#region Service Instance
        const service = {
            getIconClass: delegateToDeclaredServiceProxy(getIconClass, "getIconClass"),
            getIconColor: delegateToDeclaredServiceProxy(getIconColor, "getIconColor"),
            getTextColor: delegateToDeclaredServiceProxy(getTextColor, "getTextColor"),
            getIconText: delegateToDeclaredServiceProxy(getIconText, "getIconText"),
            getIconIcon: delegateToDeclaredServiceProxy(getIconIcon, "getIconIcon")
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services")
        .factory("iconProviderService", ["$log", "crudContextService", "$state", "$injector", "offlineSchemaService", "statuscolorService", iconProviderService]);

    //#endregion

})(angular);