(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("crudTreeNode", ["crudContextHolderService", "contextService", function (crudContextHolderService, contextService) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/crud_tree_node.html"),
            scope: {
                fieldMetadata: "=",
                datamap: "=",
                schema: "=",
                listtype: "=",
                nodeindex: "=",
                startindex: "=",
                rootTree: "=",
                level: "@"
            },

            link: function (scope, element, attrs) {

            },

            controller: ["$rootScope","$scope", "$injector", "$timeout", "i18NService", "fieldService", "formatService", "layoutservice", "expressionService", "dynFormService","numberedListBuilderService",
                function ($rootScope,$scope, $injector, $timeout, i18NService, fieldService, formatService, layoutservice, expressionService, dynFormService, numberedListBuilderService) {
                    $injector.invoke(BaseController, this, {
                        $scope: $scope,
                        i18NService: i18NService,
                        fieldService: fieldService,
                        formatService: formatService,
                        layoutservice: layoutservice,
                        expressionService: expressionService
                    });

                    $scope.hasIndex = !!$scope.listtype;
                    if ($scope.startIndex === 1) {
                        $scope.startIndex = 2;
                    }
                    $scope.index = numberedListBuilderService.calcIndex($scope.listtype, $scope.nodeindex, $scope.startindex -1);

                    $scope.childrenLevel = Number.parseInt($scope.level) + 1;
                    $scope.indentationStyle = {
                        display: "inline-block",
                        "min-width": (Number.parseInt($scope.level * 20)) + "px"
                    };

                    $scope.fieldsDatamap = $scope.safe($scope.datamap, "fields");

                    $scope.nodeDatamap = function (nodeMetadata) {
                        const nodes = $scope.safe($scope.datamap, "nodes");
                        return $scope.safe(nodes, nodeMetadata.attribute);
                    }

                    $scope.removeInputFromNode = function(node) {
                        node.displayables = [];
                    }

                    $scope.addInputToNode = function (node) {
                        if (!$rootScope["treenode_isEvaluatingEnter"]) {
                            //workaround because jquery is wrongly triggering the ng-click upon keyboard enter
                            numberedListBuilderService.addNodeInput(node);
                        }
                        
                    }

                    $scope.isEditing = function() {
                        return dynFormService.isEditing();
                    }

//                    $scope.addRow = function(fieldMetadata) {
//                       
//                    }

                    $scope.evalKeyBoardPress = function (event) {
                        if (event.which === 40 || event.which === 38) {
                            numberedListBuilderService.advanceFocus(event.which === 40);
                        }
                        if (event.which === 9) {
                            if (event.shiftKey) {
                                numberedListBuilderService.identDown($scope.rootTree, $scope.fieldMetadata);
                            } else {
                                numberedListBuilderService.identUp($scope.rootTree, $scope.fieldMetadata);
                            }
                        }
                        if ((event.which === 46 || event.which === 8) && $scope.fieldMetadata.label === "" ) {
                            numberedListBuilderService.removeNode($scope.rootTree, $scope.fieldMetadata);
                            $timeout(() => {
                                numberedListBuilderService.advanceFocus(false);
                            }, 0, false);
                        }
                        if (event.which === 13) {
                            $rootScope["treenode_isEvaluatingEnter"]= true;
                            numberedListBuilderService.addRow($scope.rootTree, $scope.fieldMetadata);
                            $timeout(() => {
                                numberedListBuilderService.advanceFocus(true);
                                $rootScope["treenode_isEvaluatingEnter"] = false;
                            }, 0, false);

                        }

                    }

                    $scope.$watch('nodeindex', (newValue,o) => {
                        if (o !== newValue) {
                            numberedListBuilderService.adjustAttributeName($scope.rootTree, $scope.fieldMetadata, newValue + 1);
                            $scope.index = numberedListBuilderService.calcIndex($scope.listtype, $scope.nodeindex, $scope.startindex);
                        }
                        
                    });

                    const init = function () {
                        if (!$scope.fieldMetadata.fields || $scope.fieldMetadata.fields.length === 0) return;
                        fieldService.fillDefaultValues($scope.fieldMetadata.fields, $scope.fieldsDatamap, $scope);
                    }

                    init();
                }]

        };
        return directive;
    }]);
})(angular);