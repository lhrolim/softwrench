(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("crudsearch", function (contextService) {
        "ngInject";

        return {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/search/crud_search.html"),
            scope: true,
            link: function () {
            },
            controller: ["$scope", "$rootScope", "$log", "$q", "$timeout", "sidePanelService", "restService", "searchService", "redirectService", "validationService", "focusService", "crudContextHolderService", "fieldService", "crudSearchService",
                function ($scope, $rootScope, $log, $q, $timeout, sidePanelService, restService, searchService, redirectService, validationService, focusService, crudContextHolderService, fieldService, crudSearchService) {

                $scope.panelid = $scope.$parent.panelid;
                $scope.crudPanelid = "search";

                var log = $log.getInstance("sw4.crudSearch",["layout"]);
                var lastApplication = "";
                var lastSchemaId = "";

                sidePanelService.setIcon($scope.panelid, "fa-search");

                // calcs the crudsearch panel height - used on scroll pane
                $scope.setPaneHeight = function () {
                    return sidePanelService.calculateScrollPanelHeight($scope.panelid);
                };

                function setFocus(newState) {
                    if (!newState) {
                        return;
                    }

                    log.debug("setFocus start");
                    $timeout(function () {
                        //time for the components to be rendered
                        focusService.setFocusToFirstField($scope.schema, $scope.datamap);
                        log.debug("setFocus end");
                    }, 1000, false);
                }

                // runs the as soon as the side panel is added
                // the show/hidden initial state is set on sidepanels before the panel is added
                function init() {
                    const ctx = sidePanelService.getContext($scope.panelid);
                    lastApplication = ctx.application;
                    lastSchemaId = ctx.schema ? ctx.schema.schemaId : null;
                    updateContent(ctx.schema);
                }

                function updateContent(schema) {
                    if (!schema) {
                        crudContextHolderService.clearCrudContext($scope.crudPanelid);
                        return;
                    }

                    $scope.datamap = {};
                    fieldService.fillDefaultValues(schema.displayables, $scope.datamap);
                    $scope.defaultDatamap = angular.copy($scope.datamap);
                    $scope.schema = schema;
                    $scope.title = schema.title;

                    crudContextHolderService.applicationChanged(schema, $scope.datamap, $scope.crudPanelid);
                    const ctx = sidePanelService.getContext($scope.panelid);
                    ctx.toggleCallback = setFocus;

                    // controls the initial state of side panel
                    const startExpanded = schema.properties && schema.properties["search.startexpanded"] === "true";
                    if (!startExpanded) {
                        return;
                    }
                    if (sidePanelService.isOpened($scope.panelid)) {
                        setFocus(ctx);
                        return;
                    }
                    if ($rootScope.deviceType === "desktop") {
                        sidePanelService.toggle($scope.panelid);
                    }
                }

                function updateSearchForm(applicationName, schemaId) {
                    crudSearchService.getSearchSchema(applicationName, schemaId).then((schema) => {
                        crudSearchService.updateCrudSearchSidePanel($scope.panelid, schema);
                        updateContent(schema);

                        if (!schema) {
                            return;
                        }

                        if (sidePanelService.getExpandedPanelFromPreference() === $scope.panelid && !sidePanelService.isOpened($scope.panelid) && $rootScope.deviceType === "desktop") {
                            sidePanelService.toggle($scope.panelid);
                        }
                        setTimeout(function () {
                            $(window).trigger("resize");
                        }, 1000);
                    });
                }

                $scope.$on(JavascriptEventConstants.ApplicationRedirected, function (event, applicationName, renderedSchema) {
                    const searchSchemaid = crudSearchService.getSearchSchemaId(applicationName, renderedSchema);
                    if (!searchSchemaid) {
                        sidePanelService.hide($scope.panelid);
                        crudContextHolderService.clearCrudContext($scope.crudPanelid);
                        return;
                    }

                    if (lastApplication === applicationName && lastSchemaId === searchSchemaid) {
                        log.debug("search panel already rendered for applicationName ({0}) and schemaid ({1})".format(applicationName, searchSchemaid));
                        return;
                    }
                    lastApplication = applicationName;
                    lastSchemaId = searchSchemaid;

                    updateSearchForm(applicationName, searchSchemaid);
                });

                // recovers the set search operator for each field of searchData
                function buildSearchOperators(searchData, searchOperator) {
                    searchOperator = searchOperator ? searchOperator : {};
                    if (!searchData || !$scope.schema.displayables) {
                        return searchOperator;
                    }

                    for (var att in searchData) {
                        if (!searchData.hasOwnProperty(att) || searchData[att] === null || searchData[att] === "") {
                            continue;
                        }
                        const displayable = $scope.schema.displayables.find(function (d) {
                            return d.attribute === att;
                        });
                        if (!displayable || !displayable.searchOperation) {
                            continue;
                        }
                        const operation = searchService.getSearchOperationById(displayable.searchOperation);
                        if (operation) {
                            searchOperator[att] = operation;
                        }
                    }
                    return searchOperator;
                }

                $scope.$on("sw.crud.search", function (event, args) {
                    log.debug("search panel search");
                    const validation = validationService.validate($scope.schema, $scope.schema.displayables, $scope.datamap);
                    if (validation.length > 0) {
                        return;
                    }
                    const searchdata = args[1];
                    const applicationName = $scope.schema.applicationName;
                    const extraParameters = {};
                    const searchOperator = args[2];
                    extraParameters.searchOperator = buildSearchOperators(searchdata, searchOperator);
                    const targetSchemaId = $scope.schema.properties["search.target.schemaid"] || "list";
                    redirectService.redirectWithData(applicationName, targetSchemaId, searchdata, extraParameters);
                });

                // clears a obj - used for clearing the datamap on a clear form call
                function clear(obj) {
                    if (!obj) {
                        return;
                    }
                    if (Array.isArray(obj)) {
                        if (obj.length === 0) {
                            return;
                        }
                        for (let i = obj.length - 1; i >= 0 ; i--) {
                            if (typeof obj[i] === "object") {
                                clear(obj[i]);
                            } else {
                                obj.splice(i, 1);
                            }
                        }
                        return;
                    }

                    for (let key in obj) {
                        if (!obj.hasOwnProperty(key)) {
                            continue;
                        }
                        if (typeof obj[key] === "object") {
                            clear(obj[key]);
                        } else {
                            obj[key] = null;
                        }
                    }
                }

                // copies a object into another - used after a form clear to copy the default datamap (datamap with only default values)
                function copy(srcObj, destObj) {
                    for (let key in srcObj) {
                        if (!srcObj.hasOwnProperty(key)) {
                            continue;
                        }
                        destObj[key] = srcObj[key];
                    }
                }

                $scope.$on("sw.crud.search.clear", function (event, args) {
                    log.debug("search panel clear");
                    $scope.datamap = crudContextHolderService.rootDataMap($scope.crudPanelid);
                    clear($scope.datamap);
                    copy($scope.defaultDatamap, $scope.datamap);
                });

                init();
            }]
        }
    });

})(angular);