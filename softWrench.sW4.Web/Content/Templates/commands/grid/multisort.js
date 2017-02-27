(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("MultiSortController", ["$scope", "crudContextHolderService", function ($scope, crudContextHolderService) {
            //all available columns
            $scope.viewColumns = [];

            const sortModel = () => crudContextHolderService.getSortModel($scope.panelid);

            $scope.vm = {
                showDefaultSortOrder: true
            };

            if ($scope.showdefaultsort === false) {
                $scope.vm.showDefaultSortOrder = false;
            }

            $scope.sortableOptions = {
                start: function (e, ui) {
                    $(e.target).data("ui-sortable").floating = true;
                },
                'ui-floating': true,
                update: function (e, ui) {
                },
                stop: function (e, ui) {
                    refreshGrid(false);
                }
            };

            $scope.orderChanged = function (viewColumn) {
                viewColumn.asc = !viewColumn.asc;

                const selected = sortModel().sortColumns.find((sortColumn) => {
                    return sortColumn.columnName === viewColumn.attribute;
                });

                if (selected) {
                    selected.isAscending = viewColumn.asc;
                }

                refreshGrid(false);
            };

            $scope.addedColumns = function () {
                const sortColumns = sortModel().sortColumns;
                if (!sortColumns || sortColumns.length === 0) {
                    return [];
                }

                const viewColumnsInOrder = [];
                angular.forEach(sortColumns, (sortColumn) => {
                    const toAdd = $scope.viewColumns.find((viewColumn) => {
                        return viewColumn.attribute === sortColumn.columnName;
                    });
                    if (toAdd) {
                        toAdd.asc = sortColumn.isAscending;
                        viewColumnsInOrder.push(toAdd);
                    }
                });
                return viewColumnsInOrder;
            }

            $scope.notAddedViewColumns = function () {
                const sortColumns = sortModel().sortColumns;
                if (!sortColumns || sortColumns.length === 0) {
                    return $scope.viewColumns;
                }

                const notAdded = [];
                angular.forEach($scope.viewColumns, (viewColumn) => {
                    const isNotAdded = sortColumns.every((sortColumn) => {
                        return viewColumn.attribute !== sortColumn.columnName;
                    });
                    if (isNotAdded) {
                        notAdded.push(viewColumn);
                    }
                });
                return notAdded;
            }

            $scope.removeColumn = function (viewColumn) {
                viewColumn.added = false;
                const sortColumns = sortModel().sortColumns;
                const idx = sortColumns.findIndex(a => a.columnName === viewColumn.attribute);
                if (idx !== -1) {
                    sortColumns.splice(idx, 1);
                }
                refreshGrid();
            };

            $scope.addColumn = function (viewColumn) {
                if (!viewColumn) {
                    return;
                }

                viewColumn.added = true;
                sortModel().sortColumns.push({
                    columnName: viewColumn.attribute,
                    isAscending: viewColumn.asc
                });
                refreshGrid();
            };

            function reset() {
                $scope.viewColumns = [];
                sortModel().sortColumns = [];
                init();
            }

            $scope.resetSortOrder = function () {
                reset();
                refreshGrid();
            };

            $scope.$watch("schema", (newValue, oldValue) => {
                if (newValue !== oldValue) {
                    reset();
                }
            });

            $scope.$watch("displayables", (newValue, oldValue) => {
                if (newValue !== oldValue) {
                    reset();
                }
            });

            function init() {
                // overrides the model
                if ($scope.initData) {
                    sortModel().sortColumns = $scope.initData;
                }

                let displayables = $scope.displayables;
                if (!displayables && $scope.schema) {
                    displayables = $scope.schema.sortableFields;
                }

                if (!displayables) {
                    return;
                }

                const sortColumns = sortModel().sortColumns;
                angular.forEach(displayables, (viewColumn) => {
                    if (viewColumn.rendererParameters && viewColumn.rendererParameters.showsort && viewColumn.rendererParameters.showsort.equalsIc("false")) {
                        return;
                    }
                    let asc = true;
                    let isAdded = false;
                    
                    if (sortColumns && sortColumns.length > 0) {
                        const sortColumn = sortColumns.find((element) => {
                            return element.columnName === viewColumn.attribute;
                        });
                        if (sortColumn) {
                            asc = sortColumn.isAscending;
                            isAdded = true;
                        }
                    }

                    $scope.viewColumns.push({
                        name: getColumnName(viewColumn),
                        attribute: viewColumn.attribute,
                        asc: asc,
                        added: isAdded
                    });
                });
            };

            function getColumnName(viewColumn) {
                return viewColumn.label || viewColumn.attribute;
            };

            function refreshGrid() {
                $scope.sort();
            };

            init();
        }
        ])
    .directive("multiSort", ["contextService", function (contextService) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/commands/grid/multiSort.html"),
            scope: {
                schema: "=",
                displayables: '=',
                //a list of SortOrder object representations, to indicate how to populate the initial state of the directive
                initData: "=",
                //the method to be executed whenever the sort changes
                sort: "&",
                showdefaultsort: "=",
                panelid: "="
            },
            controller: "MultiSortController"
        };
        return directive;
    }
    ]);

})(angular);