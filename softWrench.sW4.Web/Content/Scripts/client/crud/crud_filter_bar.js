(function (angular, BaseController, BaseList) {
    "use strict";

    angular.module("sw_layout").directive("crudFilterBar", ["contextService", "$timeout", function (contextService, $timeout) {
        var directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/crud_filter_bar.html"),
            scope: {
                schema: "=",
                datamap: "=",
                searchData: "=",
                searchOperator: "=",
                selectAll: "=",
                advancedFilterMode: "=",
                filterApplied: "&"
            },
            controller: ["$scope", "$injector", "i18NService", "fieldService", "commandService", "formatService", "expressionService", "searchService",
                function ($scope, $injector, i18NService, fieldService, commandService, formatService, expressionService, searchService) {

                    var config = {
                        noopoperator: { id: "", symbol: "", begin: "", end: "", title: "No Filter" }
                    };

                    $scope.selectOperator = function (columnName, operator) {
                        $scope.searchOperator[columnName] = operator;
                        var searchData = $scope.searchData;

                        if (operator.id === "") {
                            searchData[columnName] = "";
                        } else if (searchData[columnName] != null && searchData[columnName] !== "") {
                            return;
                        } else if (operator.id === "BLANK") {
                            searchData[columnName] = "";
                        }
                    };

                    $scope.clearFilter = function(columnName) {
                        $scope.selectOperator(columnName, config.noopoperator);
                        $scope.filterApplied();
                    };

                    $scope.filterSearch = function (columnName) {
                        if (!$scope.searchOperator[columnName] || $scope.searchOperator[columnName].symbol === "") {
                            $scope.searchOperator[columnName] = searchService.defaultSearchOperation();
                        }
                        var searchString = $scope.searchData[columnName];
                        if (!searchString) {
                            $scope.searchOperator[columnName] = searchService.getSearchOperationById("BLANK");
                            $scope.searchData[columnName] = " ";
                        }
                        $scope.filterApplied();
                    };

                    $scope.toggleSelectAll = function (checked) {
                        angular.forEach($scope.datamap, function (value) {
                            value.fields["_#selected"] = checked;
                        });
                    };

                    //#region Initialization
                    (function(ctrlInstance) {
                        // 'inherit' from BaseController controller
                        $injector.invoke(BaseController, ctrlInstance, {
                            $scope: $scope,
                            i18NService: i18NService,
                            fieldService: fieldService,
                            commandService: commandService,
                            formatService: formatService
                        });
                        // 'inherit' from BaseList controller
                        $injector.invoke(BaseList, ctrlInstance, {
                            $scope: $scope,
                            formatService: formatService,
                            expressionService: expressionService,
                            searchService: searchService,
                            commandService: commandService
                        });
                    })(this);
                    //#endregion
                }],

            link: function (scope, element, attrs) {
                // don't let dropdowns close automatically when clicked inside
                $timeout(function () {
                    var dropdowns = angular.element(element[0].querySelectorAll(".js_filter .dropdown .dropdown-menu"));
                    dropdowns.click(function (event) {
                        event.stopPropagation();
                    });
                });
            }
        };

        return directive;

    }]);

})(angular, BaseController, BaseList);

