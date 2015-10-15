(function (angular, BaseController, BaseList) {
    "use strict";

    angular.module("sw_layout").directive("crudFilterBar", ["contextService", "$timeout", function (contextService, $timeout) {
        var directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/crud_filter_bar.html"),
            scope: {
                schema: "=", // model's current schema
                datamap: "=", // model's current datamap
                searchData: "=", // shared dictionary [column : current filter search value]
                searchOperator: "=", // shared dictionary [column : current filter operator object]
                selectAll: "=", // shared boolean flag indicating if multiple select in filters is selected
                advancedFilterMode: "=", // shared boolean flag indicating if advanced filter mode is activated
                filterApplied: "&" // callback executed when the filters are applied
            },
            controller: ["$scope", "$injector", "i18NService", "fieldService", "commandService", "formatService", "expressionService", "searchService",
                function ($scope, $injector, i18NService, fieldService, commandService, formatService, expressionService, searchService) {

                    var config = {
                        /** 'don't filter' operator: helper to clear current filter */
                        noopoperator: { id: "", symbol: "", begin: "", end: "", title: "No Filter" }
                    };

                    /**
                     * Sets the operator in $scope.searchOperator[columnName]
                     * 
                     * @param String columnName 
                     * @param {} operator 
                     */
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

                    /**
                     * Clears the filter associated to columnName 
                     * and invokes the 'filterApplied' callback.
                     * 
                     * @param String columnName 
                     */
                    $scope.clearFilter = function(columnName) {
                        $scope.selectOperator(columnName, config.noopoperator);
                        $scope.filterApplied();
                    };

                    /**
                     * Immediately applies the filter associated to columnName.
                     * 
                     * @param String columnName 
                     */
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

                    /**
                     * Marks the $scope.datamap's values's fields as checked ("_#selected" key)
                     * 
                     * @param Boolean checked 
                     */
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

