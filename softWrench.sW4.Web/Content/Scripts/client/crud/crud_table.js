(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("crudTable", ["crudContextHolderService", "contextService", function (crudContextHolderService, contextService) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/crud_table.html"),
            scope: {
                fieldMetadata: "=",
                datamap: "=",
                schema: "="
            },

            link: function (scope, element, attrs) {

            },

            controller: ["$scope", "$injector", "i18NService", "fieldService", "formatService", "layoutservice", "expressionService",
                function ($scope, $injector, i18NService, fieldService, formatService, layoutservice, expressionService) {
                    $injector.invoke(BaseController, this, {
                        $scope: $scope,
                        i18NService: i18NService,
                        fieldService: fieldService,
                        formatService: formatService,
                        layoutservice: layoutservice,
                        expressionService: expressionService
                    });

                    const getOption = function (optionMetadata, placeHolder) {
                        const optionIndex = placeHolder ? placeHolder.indexOnParent : 0;
                        return optionMetadata.options[optionIndex];
                    }

                    $scope.optionValue = function(optionMetadata, placeHolder) {
                        return getOption(optionMetadata, placeHolder).value;
                    }

                    $scope.isOptionSelected = function (rowindex, optionMetadata, placeHolder) {
                        const tableDm = $scope.safe($scope.datamap, $scope.fieldMetadata.attribute);
                        const rowDm = $scope.safe(tableDm, rowindex);
                        return rowDm[optionMetadata.attribute] === $scope.optionValue(optionMetadata, placeHolder);
                    }

                    $scope.changeSelection = function (rowindex, optionMetadata, placeHolder) {
                        const tableDm = $scope.safe($scope.datamap, $scope.fieldMetadata.attribute);
                        const rowDm = $scope.safe(tableDm, rowindex);
                        const value = $scope.optionValue(optionMetadata, placeHolder);
                        if (rowDm[optionMetadata.attribute] === value) {
                            rowDm[optionMetadata.attribute] = undefined;
                        } else {
                            rowDm[optionMetadata.attribute] = value;
                        }
                    }

                    $scope.safeCSSselector = function (name) {
                        return safeCSSselector(name);
                    }

                    const init = function() {
                        const tableDm = $scope.safe($scope.datamap, $scope.fieldMetadata.attribute);
                        if (!$scope.fieldMetadata.rows || $scope.fieldMetadata.rows.length === 0) return;

                        $scope.fieldMetadata.rows.forEach((row, rowindex) => {
                            const rowDm = $scope.safe(tableDm, rowindex);
                            fieldService.fillDefaultValues(row, rowDm, $scope);
                        });
                    }

                    init();
                }]

        };
        return directive;
    }]);
})(angular);