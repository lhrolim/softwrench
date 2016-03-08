(function (app, angular) {
    "use strict";

app.directive('crudInputWrapper', function (contextService, $compile) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",

        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            extraparameters: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            cancelfn: '&',
            savefn: '&',
            previousschema: '=',
            previousdata: '=',
            parentdata: '=',
            parentschema: '=',
            title: '=',
            elementid: '@',
            isMainTab: '@',
            tabid: '@',
            ismodal: '@',
            panelid: '@'
        },

        link: function (scope, element, attrs) {
            var doLoad = function () {
                element.append(
                  "<crud-input elementid='crudInputMain' schema='schema' extraparameters='extraparameters'" +
                  "datamap='datamap'  blockedassociations='blockedassociations'" +
                  "association-schemas='associationSchemas'cancelfn='cancel(data,schema)' displayables='displayables'" +
                  "savefn='save(selecteditem, parameters)' previousschema='previousschema' previousdata='previousdata' " +
                  "parentschema='parentschema' parentdata='parentdata'  ismodal='{{ismodal}}' panelid='panelid'/>"
               );
                $compile(element.contents())(scope);
                scope.loaded = true;
            }

            if (scope.schema.mode.equalsAny("input", "none") && ("true" === scope.isMainTab)) {
                doLoad();
            }

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid === tabid && !scope.loaded) {
                    doLoad();
                }
            });

            scope.save = function () {
                scope.savefn();
            };

            scope.cancel = function (data, schema) {
                scope.cancelfn({ data: data, schema: schema });
                scope.$emit('sw_cancelclicked');
            };
        }
    }
});

app.directive('crudInput', ["contextService", "associationService", function (contextService, associationService) {

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_input.html'),
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            extraparameters: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            cancelfn: '&',
            savefn: '&',
            previousschema: '=',
            previousdata: '=',
            parentschema: '=',
            parentdata: '=',
            title: '=',
            elementid: '@',
            composition: '@',
            ismodal: '@',
            panelid: '='
        },

        controller: ["$scope", "$http", "$injector", "$element", "alertService", "printService", "compositionService", "commandService", "fieldService", "i18NService", "formatService", "crudContextHolderService", "$log",
            function ($scope, $http, $injector, $element, alertService, printService, compositionService, commandService, fieldService, i18NService, formatService, crudContextHolderService, $log) {

            $scope.$name = 'crudinput';

            this.cancel = function () {
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            this.save = function () {
                $scope.savefn();
            };


            this.shouldshowprint = function () {
                return $scope.composition != "true";
            }

            $scope.isEditing = function (schema, datamap) {
                var id = datamap[schema.idFieldName];
                return id != null;
            };

            if ($scope.composition === "true") {
                //really makes sense?
                associationService.loadSchemaAssociations($scope.datamap, $scope.schema, { avoidspin: true });
            }

            $scope.getPosition = function (schema) {
                if (!schema.properties || !schema.properties["commandbar.bottom"]) {
                    return "detail.primary";
                }
                return schema.properties["commandbar.bottom"];
            }

            //#region $dirty checking
            function handleDirtyChecking() {
                var log = $log.get("crud_input#dirtychecking", ["datamap", "dirtycheck"]);
                var dirtyWatcherDeregister;

                function dirtyWatcher(newDatamap, oldDatamap) {
                    if (newDatamap === oldDatamap || !crudContextHolderService.getDetailDataResolved()) return;

                    if (log.isLevelEnabled("trace")) {
                        Object.keys(newDatamap)
                            .forEach(function (k) {
                                if (angular.equals(newDatamap[k], oldDatamap[k])) return;
                                if (!angular.isArray(newDatamap[k]) && !angular.isArray(oldDatamap[k])) {
                                    log.trace("changed", k, "from", oldDatamap[k], "to", newDatamap[k]);
                                    return;
                                }
                                for (var i = 0; i < newDatamap[k].length; i++) {
                                    var newArrayItem = newDatamap[k][i], oldArrayItem = oldDatamap[k][i];
                                    if (angular.equals(newArrayItem, oldArrayItem)) continue;
                                    if (!angular.isObject(newArrayItem) && !angular.isObject(oldArrayItem)) {
                                        log.trace("changed", "[" + k + "," + i + "]", "from", oldArrayItem, "to", newArrayItem);
                                        return;
                                    }
                                    Object.keys(newArrayItem).forEach(function (ki) {
                                        if (angular.equals(newArrayItem[ki], oldArrayItem[ki])) return;
                                        log.trace("changed", "[" + k + "," + i + "," + ki + "]", "from", oldArrayItem[ki], "to", newArrayItem[ki]);
                                    });
                                }
                            });
                    }
                    crudContextHolderService.setDirty();
                }

                $scope.$watch(
                    function () {
                        return crudContextHolderService.getDetailDataResolved();
                    },
                    debounce(function (newValue, oldValue) {
                        if (newValue === oldValue) return;
                        if (newValue) {
                            log.trace("crudContextHolderService#dataResolved is true: start $dirty checking");
                            // detailData was resolved: start $dirty watching (if wasn't already registered)
                            if (!angular.isFunction(dirtyWatcherDeregister)) {
                                dirtyWatcherDeregister = $scope.$watch("datamap", dirtyWatcher, true);
                                log.trace("bluring current element and focusing on document.body -> trigger $watch on first change of the current input");
                                document.activeElement.blur();
                                $(document.body).focus();
                            }
                        } else {
                            log.trace("crudContextHolderService#dataResolved is false: disable $dirty checking");
                            if (angular.isFunction(dirtyWatcherDeregister)) dirtyWatcherDeregister();
                            dirtyWatcherDeregister = null;
                        }
                    }));

            }

            handleDirtyChecking();
            //#endregion
            

            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService,
                formatService: formatService
            });
        }]
    };
}]);

})(app, angular);