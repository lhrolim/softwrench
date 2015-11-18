(function (angular, BaseController, BaseList, $) {
    "use strict";

    angular.module("sw_layout")
        .directive("crudFilterDropdown", ["contextService", "$timeout", function (contextService, $timeout) {
            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Templates/crud/filter/crud_filter_dropdown.html"),
                scope: {
                    filter: "=", // filter to show
                    schema: "=", // model's current schema
                    datamap: "=", // model's current datamap
                    searchData: "=", // shared dictionary [column : current filter search value]
                    searchOperator: "=", // shared dictionary [column : current filter operator object]
                    selectAll: "=", // shared boolean flag indicating if multiple select in filters is selected
                    advancedFilterMode: "=", // shared boolean flag indicating if advanced filter mode is activated
                    filterApplied: "&" // callback executed when the filters are applied
                },
                //#region controller
                controller: ["$scope", "$injector", "i18NService", "fieldService", "commandService", "formatService", "expressionService", "searchService", "filterModelService",
                    function ($scope, $injector, i18NService, fieldService, commandService, formatService, expressionService, searchService, filterModelService) {

                        $scope.layout = {
                            standalone: false
                        }

                        var config = {
                            /** 'don't filter' operator: helper to clear current filter */
                            noopoperator: { id: "NF", symbol: "", begin: "", end: "", title: "No Filter" }
                        };

                        $scope.markDefaultOperator = function (filter) {
                            if (!$scope.searchOperator[filter.attribute]) {
                                $scope.searchOperator[filter.attribute] = searchService.defaultSearchOperation();
                            }
                        }

                        $scope.hasFilter = function (filter) {
                            var operator = $scope.searchOperator[filter.attribute];
                            if (!operator) return false;
                            var search = $scope.searchData[filter.attribute];
                            if (operator.id === "BLANK") return true;
                            return !!search && !operator.id.equalsAny("", "NF");

                        }

                        /**
                         * Sets the operator in $scope.searchOperator[columnName]
                         * 
                         * @param String columnName 
                         * @param {} operator 
                         */
                        $scope.selectOperator = function (columnName, operator) {
                            if ($scope.searchOperator[columnName] && $scope.searchOperator[columnName].id === operator.id) {
                                $scope.filterIsActive = true;
                                this.filterBarApplied();
                                return;
                            }
                            $scope.searchOperator[columnName] = operator;
                            var searchData = $scope.searchData;

                            if (operator.id.equalsAny("", "NF")) {
                                searchData[columnName] = "";
                            } else if (searchData[columnName] != null && searchData[columnName] !== "") {
                                //if there is a search value, apply on click
                                this.filterBarApplied();
                                return;
                            } else if (operator.id === "BLANK") {
                                searchData[columnName] = "";
                                this.filterBarApplied();
                            }
                        };

                        /**
                         * Clears the filter associated to columnName 
                         * and invokes the 'filterApplied' callback.
                         * 
                         * @param String columnName 
                         */
                        $scope.clearFilter = function (filterAttribute) {
                            $(".dropdown.open").removeClass("open");
                            $scope.selectOperator(filterAttribute, config.noopoperator);
                            $scope.filterApplied();
                            $scope.$broadcast("sw.filter.clear", filterAttribute);
                        };

                        /**
                         * Intermediary function to perform operations specific to this directive before delegating to the & delegated function
                         */
                        $scope.filterBarApplied = function (keepitOpen) {
                            if (true !== keepitOpen) {
                                $(".dropdown.open").removeClass("open");
                            }
                            $scope.filterApplied();
                        }

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
                            $(".dropdown.open").removeClass("open");
                            this.filterBarApplied();
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

                        $scope.getFilterText = function (filter) {
                            return filterModelService.getFilterText(filter, $scope.searchData, $scope.getOperator(filter.attribute));
                        }

                        function collapseOperatorList($event, mode) {
                            // required to 'stop' the event in input groups
                            // some inputs (like datepicker) trigger focus
                            $event.preventDefault();
                            $event.stopPropagation();
                            $event.stopImmediatePropagation();
                            // toggling list collapse
                            $($event.delegateTarget)
                                .parents(".js_filter_content")
                                .find("ul.js_operator_list")
                                .collapse(mode);
                        }

                        $scope.toggleCollapseOperatorList = function ($event) {
                            collapseOperatorList($event, "toggle");
                        }

                        $scope.closeCollapseOperatorList = function ($event) {
                            collapseOperatorList($event, "hide");
                        }

                        /**
                         * Retrieves the tooltip of the current search operator applied to the attribute.
                         * If no operator is selected for the attribute it uses the default search operator.
                         * 
                         * @param String attribute 
                         * @returns String 
                         */
                        $scope.getOperatorTooltip = function (attribute) {
                            var operator = $scope.getOperator(attribute);
                            return !!operator && !!operator.id ? operator.tooltip : $scope.getDefaultOperator().tooltip;
                        };

                        /**
                         * Retrieves the icon of the current search operator applied to the attribute.
                         * If no operator is selected for the attribute it uses the default search icon
                         * (icon for the default operator).
                         * 
                         * @param String attribute 
                         * @returns String 
                         */
                        $scope.getOperatorIcon = function (attribute) {
                            var icon = $scope.getSearchIcon(attribute);
                            return !!icon ? icon : $scope.getDefaultSearchIcon();
                        };

                        /**
                         * Filters the operations that should be displayed.
                         * 
                         * @param {} filter
                         * @returns Array<Operation> 
                         */
                        $scope.displayableSearchOperations = function (filter) {
                            var operations = $scope.searchOperations();
                            if (!operations) return operations;
                            return operations.filter(function (operation) {
                                return $scope.shouldShowFilter(operation, filter);
                            });
                        }

                        $scope.closeFilterDropdown = function ($event) {
                            $($event.delegateTarget).parents(".dropdown.open").removeClass("open");
                        }

                        $scope.setStandaloneMode = function (value) {
                            $scope.layout.standalone = value;
                        };

                        $injector.invoke(BaseController, this, {
                            $scope: $scope,
                            i18NService: i18NService,
                            fieldService: fieldService,
                            commandService: commandService,
                            formatService: formatService
                        });

                        // 'inherit' from BaseList controller
                        $injector.invoke(BaseList, this, {
                            $scope: $scope,
                            formatService: formatService,
                            expressionService: expressionService,
                            searchService: searchService,
                            commandService: commandService
                        });

                    }],
                //#endregion
                //#region postlink
                link: function (scope, element, attrs) {
                    scope.setStandaloneMode(attrs.hasOwnProperty("filterStandalone"));

                    var prepareUi = function () {
                        // don't let dropdowns close automatically when clicked inside
                        var dropdowns = angular.element(element[0].querySelectorAll(".js_filter .dropdown .dropdown-menu"));
                        dropdowns.click(function (event) {
                            event.stopPropagation();
                        });
                        // autofocus the search input when the dropdown opens
                        $(".js_filter .dropdown").on("show.bs.dropdown", function (event) {
                            $timeout(function () {
                                $(event.target).find("input[type=search]").focus();
                            });
                        });
                    }

                    $timeout(prepareUi, 0, false);

                    scope.$on("sw_griddatachanged", function () {
                        // need to register this call for whenever the grid changes
                        $timeout(prepareUi, 0, false);
                    });
                }
                //#endregion
            };
            return directive;
        }]);

})(angular, BaseController, BaseList, jQuery);