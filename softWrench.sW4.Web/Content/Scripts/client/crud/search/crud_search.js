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
            controller: ["$scope", "$rootScope", "$log", "$q", "$timeout", "sidePanelService", "schemaCacheService", "restService", "searchService", "redirectService", "applicationService", "$http", "validationService", "focusService", "crudContextHolderService", "fieldService", function ($scope, $rootScope, $log, $q, $timeout, sidePanelService, schemaCacheService, restService, searchService, redirectService, applicationService, $http, validationService, focusService, crudContextHolderService, fieldService) {

                $scope.panelid = $scope.$parent.panelid;
                $scope.crudPanelid = "search";

                var log = $log.getInstance("sw4.crudSearch");
                var lastApplication = "";
                var lastSchemaId = "";

                // workaround - if the schemaCacheService.getCachedSchema is used before $http.get(applicationService.getApplicationUrl(...))
                // the null schema result is considered cached and the server don't build the correct schema after that
                // only the key is cached here to avoid invoke schemaCacheService.getCachedSchema first
                // TODO Fix schemaCacheService.getCachedSchema + $http.get(applicationService.getApplicationUrl(...))
                var schemacache = {};

                sidePanelService.hide($scope.panelid, true);
                sidePanelService.setIcon($scope.panelid, "fa-search");

                // calcs the crudsearch panel height - used on scroll pane
                $scope.setPaneHeight = function () {
                    log.debug("setPaneHeight");

                    var headerHeight = $("#crudsearch header").height();
                    var panePaddingTop = parseInt($("#crudsearch .pane").css("padding-top"));
                    var panePaddingBottom = parseInt($("#crudsearch .pane").css("padding-bottom"));

                    var height = $(window).height() - headerHeight - panePaddingTop - panePaddingBottom;
                    return height;
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

                function getSearchSchema(applicationName, schemaId) {
                    if (!applicationName || !schemaId) {
                        log.debug("getSearchSchema  - no applicationName ({0}) or schemaId ({1})".format(applicationName, schemaId));
                        return $q.when(null);
                    }

                    if (schemacache[applicationName + "." + schemaId]) {
                        log.debug("getSearchSchema  - cache hit on applicationName ({0}) and  schemaId ({1})".format(applicationName, schemaId));
                        return $q.when(schemaCacheService.getCachedSchema(applicationName, schemaId));
                    }

                    var redirectUrl = applicationService.getApplicationUrl(applicationName, schemaId, "input");
                    return $http.get(redirectUrl).then(function (httpResponse) {
                        log.debug("getSearchSchema - server response on applicationName ({0}) and  schemaId ({1})".format(applicationName, schemaId));
                        var schema = httpResponse.data.schema;
                        schemaCacheService.addSchemaToCache(schema);
                        schemacache[applicationName + "." + schemaId] = true;
                        return schema;
                    });
                }

                function updateSearchForm(applicationName, schemaId) {
                    getSearchSchema(applicationName, schemaId).then(function (schema) {
                        if (!schema) {
                            sidePanelService.hide($scope.panelid);
                            crudContextHolderService.clearCrudContext($scope.crudPanelid);
                            return;
                        }

                        sidePanelService.setTitle($scope.panelid, schema.title);

                        var handleWidth = schema.properties ? schema.properties["search.panelwidth"] : null;
                        sidePanelService.setHandleWidth($scope.panelid, handleWidth);

                        sidePanelService.show($scope.panelid);
                        $scope.datamap = {};
                        fieldService.fillDefaultValues(schema.displayables, $scope.datamap);
                        $scope.defaultDatamap = angular.copy($scope.datamap);
                        $scope.schema = schema;
                        $scope.title = schema.title;

                        crudContextHolderService.applicationChanged(schema, $scope.datamap, $scope.crudPanelid);
                        var ctx = sidePanelService.getContext($scope.panelid);
                        ctx.toggleCallback = setFocus;

                        // controls the initial state of side panel
                        var startExpanded = schema.properties && schema.properties["search.startexpanded"] === "true";
                        if (startExpanded) {
                            if (!sidePanelService.isOpened($scope.panelid)) {
                                sidePanelService.toggle($scope.panelid);
                            } else {
                                setFocus(ctx);
                            }
                        }

                        if (sidePanelService.getExpandedPanelFromPreference() === $scope.panelid && !sidePanelService.isOpened($scope.panelid)) {
                            sidePanelService.toggle($scope.panelid);
                        }
                        setTimeout(function () {
                            $(window).trigger("resize");
                        }, 1000);
                    });
                }

                $rootScope.$on("sw_applicationrendered", function (event, applicationName, renderedSchema) {
                    if (!applicationName || !renderedSchema) {
                        log.debug("no applicationName ({0}) or renderedSchema ({1})".format(applicationName, renderedSchema));
                        sidePanelService.hide($scope.panelid);
                        crudContextHolderService.clearCrudContext($scope.crudPanelid);
                        return;
                    }

                    var searchSchemaid = renderedSchema.properties ? renderedSchema.properties["search.schemaid"] : null;
                    if (!searchSchemaid) {
                        log.debug("no searchSchemaid on applicationName ({0}) and renderedSchema ({1})".format(applicationName, renderedSchema));
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
                        var displayable = $scope.schema.displayables.find(function (d) {
                            return d.attribute === att;
                        });
                        if (!displayable || !displayable.searchOperation) {
                            continue;
                        }
                        var operation = searchService.getSearchOperationById(displayable.searchOperation);
                        if (operation) {
                            searchOperator[att] = operation;
                        }
                    }
                    return searchOperator;
                }

                $scope.$on("sw.crud.search", function (event, args) {
                    log.debug("search panel search");
                    var validation = validationService.validate($scope.schema, $scope.schema.displayables, $scope.datamap);
                    if (validation.length > 0) {
                        return;
                    }
                    var searchdata = args[1];
                    var applicationName = $scope.schema.applicationName;
                    var extraParameters = {};
                    var searchOperator = args[2];
                    extraParameters.searchOperator = buildSearchOperators(searchdata, searchOperator);

                    var targetSchemaId = $scope.schema.properties["search.target.schemaid"] || "list";
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
                        for (var i = obj.length - 1; i >= 0 ; i--) {
                            if (typeof obj[i] === "object") {
                                clear(obj[i]);
                            } else {
                                obj.splice(i, 1);
                            }
                        }
                        return;
                    }

                    for (var key in obj) {
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
                    for (var key in srcObj) {
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
            }]
        }
    });

})(angular);