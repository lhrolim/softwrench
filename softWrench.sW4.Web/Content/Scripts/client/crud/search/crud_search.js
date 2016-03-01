(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("crudsearch", function (contextService) {
        "ngInject";

        return {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/search/crud_search.html"),
            scope: true,
            link: function() {
            },
            controller: ["$scope", "$rootScope", "$log", "$q", "$timeout", "sidePanelService", "schemaCacheService", "restService", "searchService", "redirectService", "applicationService", "$http", "validationService", "focusService", function ($scope, $rootScope, $log, $q, $timeout, sidePanelService, schemaCacheService, restService, searchService, redirectService, applicationService, $http, validationService, focusService) {

                $scope.panelid = $scope.$parent.panelid;

                var log = $log.getInstance("sw4.crudSearch");
                var lastApplication = "";
                var lastSchemaId = "";

                sidePanelService.hide($scope.panelid);
                sidePanelService.setIcon($scope.panelid, "fa-search");

                $scope.setPaneHeight = function () {
                    log.debug("setPaneHeight");

                    var headerHeight = $("#crudsearch header").height();
                    var panePaddingTop = parseInt($("#crudsearch .pane").css("padding-top"));
                    var panePaddingBottom = parseInt($("#crudsearch .pane").css("padding-bottom"));

                    var height = $(window).height() - headerHeight - panePaddingTop - panePaddingBottom;
                    return height;
                };

                function setFocus(ctx) {
                    if (!ctx.opened) {
                        return;
                    }
                    $timeout(function () {
                        //time for the components to be rendered
                        focusService.setFocusToFirstField($scope.schema, $scope.datamap);
                    }, 1000, false);
                }

                function getSearchForm(applicationName, schemaId) {
                    var redirectUrl = applicationService.getApplicationUrl(applicationName, schemaId, "input");
                    $http.get(redirectUrl).then(function (httpResponse) {
                        var data = httpResponse.data;

                        var schema = data.schema;
                        if (data.cachedSchemaId) {
                            schema = schemaCacheService.getCachedSchema(applicationName, data.cachedSchemaId);
                        } else {
                            schemaCacheService.addSchemaToCache(schema);
                        }

                        sidePanelService.setTitle($scope.panelid, schema.title);

                        var handleWidth = schema.properties ? schema.properties["search.handlewidth"] : null;
                        sidePanelService.setHandleWidth($scope.panelid, handleWidth);

                        sidePanelService.show($scope.panelid);
                        $scope.datamap = data.resultObject.fields;
                        $scope.schema = schema;
                        $scope.title = schema.title;

                        var ctx = sidePanelService.getContext($scope.panelid);
                        ctx.toggleCallback = setFocus;

                        // controls the initial state of side panel
                        var startExpanded = schema.properties && schema.properties["search.startexpanded"] === "true";
                        if (startExpanded) {
                            if (!ctx.opened) {
                                sidePanelService.toggle($scope.panelid);
                            } else {
                                setFocus(ctx);
                            }
                        }
                        else if (ctx.opened) {
                            sidePanelService.toggle($scope.panelid);
                        }
                    });
                }

                $rootScope.$on("sw_applicationrendered", function (event, applicationName, renderedSchema) {
                    if (!applicationName || !renderedSchema) {
                        sidePanelService.hide($scope.panelid);
                        return;
                    }

                    var searchSchemaid = renderedSchema.properties ? renderedSchema.properties["search.schemaid"] : null;
                    if (!searchSchemaid) {
                        sidePanelService.hide($scope.panelid);
                        return;
                    }

                    if (lastApplication === applicationName && lastSchemaId === searchSchemaid) {
                        return;
                    }
                    lastApplication = applicationName;
                    lastSchemaId = searchSchemaid;

                    getSearchForm(applicationName, searchSchemaid);
                });

                function buildSearchOperators(searchData, searchOperator) {
                    searchOperator = searchOperator ? searchOperator : {};
                    if (!searchData || !$scope.schema.displayables) {
                        return searchOperator;
                    }

                    for (var att in searchData) {
                        if (!searchData.hasOwnProperty(att) || !searchData[att]) {
                            continue;
                        }
                        var displayable = $scope.schema.displayables.find(function(d) {
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
                    var validation = validationService.validate($scope.schema, $scope.schema.displayables, $scope.datamap);
                    if (validation.length > 0) {
                        return;
                    }
                    var searchdata = args[1];
                    var applicationName = $scope.schema.applicationName;
                    var extraParameters = {};
                    var searchOperator = args[2];
                    extraParameters.searchOperator = buildSearchOperators(searchdata, searchOperator);
                    redirectService.redirectWithData(applicationName, "list", searchdata, extraParameters);
                });
            }]
        }
    });

})(angular);