(function (angular) {
    "use strict";

    const app = angular.module("sw_layout");
    app.directive("inlineCompositionListOutput", function (contextService) {
        "ngInject";

        return {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/inline_composition_list_output.html"),
            scope: {
                parentdata: "=",
                metadata: "=",
                iscollection: "=",
                parentschema: "=",
                mode: "@",
                ismodal: "@",
                forprint: "="
            },

            controller: ["$scope", "$injector", "$q", "$timeout", "compositionService", "fieldService", "i18NService", "layoutservice", "expressionService", "formatService", "printAwaitableService",
                function ($scope, $injector, $q, $timeout, compositionService, fieldService, i18NService, layoutservice, expressionService, formatService, printAwaitableService) {

                    if ($scope.forprint) {
                        $scope.printDefered = $q.defer();
                        printAwaitableService.registerAwaitable($scope.printDefered.promise);
                    }

                    $scope.contextPath = function (path) {
                        return url(path);
                    };

                    $scope.safeCSSselector = function (name) {
                        return safeCSSselector(name);
                    };

                    $scope.isCompositionItemFieldHidden = function (application, fieldMetadata, item) {
                        const datamap = item == null ? $scope.parentdata : compositionService.buildMergedDatamap(item, $scope.parentdata);
                        return fieldService.isFieldHidden(datamap, application, fieldMetadata);
                    };

                    $scope.columns = function() {
                        return $scope.compositionlistschema.displayables;
                    }

                    $scope.compositionData = function () {
                        return $scope.compositiondata || [];
                    }

                    $scope.hasDetailSchema = function() {
                        return !!$scope.compositiondetailschema;
                    }

                    $scope.expansionAllowed = function (item) {
                        const compositionId = item[$scope.compositionlistschema.idFieldName]; //we cannot expand an item that doesn´t have an id
                        return compositionId != null;
                    }

                    $scope.isItemExpanded = function (item) {
                        const compositionId = item[$scope.compositionlistschema.idFieldName];
                        return $scope.compositiondata[compositionId] && $scope.compositiondata[compositionId].expanded;
                    }

                    $scope.toggleDetails = function (compositionitem) {
                        $scope.expandedData[compositionitem[$scope.compositionlistschema.idFieldName]] = !$scope.expandedData[compositionitem[$scope.compositionlistschema.idFieldName]];
                    };

                    function compositionDataResolved(compositiondata) {
                        if (!compositiondata) {
                            //this is not the data this tab is interested
                            return;
                        }
                        const data = compositiondata[$scope.metadata.relationship];
                        if (!data) {
                            $scope.compositiondata = [];
                            return;
                        }
                        $scope.compositiondata = data.list || data.resultList;
                    }

                    $scope.$on(JavascriptEventConstants.COMPOSITION_RESOLVED, function (event, compositiondata) {
                        compositionDataResolved(compositiondata);
                        if (!$scope.printDefered) {
                            return;
                        }
                        $timeout(function () {
                            $scope.printDefered.resolve();
                        }, 0);
                    });

                    function init() {
                        $scope.compositionschemadefinition = $scope.metadata.schema;
                        $scope.compositiondata = $scope.parentdata[$scope.metadata.relationship];
                        $scope.compositionlistschema = $scope.compositionschemadefinition.schemas.list;
                        $scope.compositiondetailschema = $scope.compositionschemadefinition.schemas.detail;
                        $scope.schema = $scope.compositionlistschema;
                        $scope.expandedData = {};

                        if ($scope.compositiondata && $scope.printDefered) {
                            $scope.printDefered.resolve();
                        }

                        $injector.invoke(BaseController, this, {
                            $scope: $scope,
                            i18NService: i18NService,
                            fieldService: fieldService,
                            formatService: formatService,
                            layoutservice: layoutservice,
                            expressionService: expressionService
                        });
                    }

                    init();
                }]
        };
    });


})(angular);