(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .controller("MultiSortController", ["$scope", '$timeout', function ($scope, $timeout) {

            //the columns which were selected on the component
            $scope.addedColumns = [];
            //all available columns
            $scope.columns = [];
            //the current column being edited
            $scope.selected = null;

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
                    refreshGrid();
                }
            };

            $scope.orderChanged = function (column) {
                column.asc = !column.asc;

                const selected = $scope.initData.find(function (element, index, array) {
                    return element.columnName === column.attribute;
                });

                if (selected) {
                    selected.isAscending = column.asc;
                }


                refreshGrid();
            };

            $scope.removeColumn = function (column) {
                column.added = false;
                $scope.addedColumns.splice($scope.addedColumns.indexOf(column), 1);

                const idx = $scope.initData.findIndex(a => a.columnName === column.columnName);
                if (idx !== -1) {
                    $scope.initData.splice(idx, 1);
                }
                refreshGrid();
            };

            $scope.addColumn = function (column) {
                if (column != null) {
                    $scope.selected.added = true;
                    $scope.addedColumns.push($scope.selected);
                    $scope.initData.push($scope.selected);
                    refreshGrid();
                }
            };

            $scope.resetSortOrder = function () {
                clear();
                refreshGrid();
            };

            $scope.$watch('schema', function (newValue, oldValue) {
                $scope.columns = [];
                $scope.addedColumns = [];
                $scope.selected = null;
                init();
            });

            $scope.$watch('displayables', function (newValue, oldValue) {
                $scope.columns = [];
                $scope.addedColumns = [];
                $scope.selected = null;
                init();
            });

            function init() {
                let displayables = $scope.displayables;
                if (!displayables && $scope.schema) {
                    displayables = $scope.schema.nonHiddenFields;
                }

                let defaultOrderBy = null;

                if ($scope.schema) {
                    defaultOrderBy = $scope.schema.properties['list.defaultorderby'];
                }

                if (!displayables) {
                    return;
                }

                let defaultOrderAsc = true;
                let defaultColumn = '';
                if (defaultOrderBy) {
                    const indexOfDot = defaultOrderBy.indexOf('.');
                    defaultColumn = (indexOfDot === -1) ? defaultOrderBy.substring(0, defaultOrderBy.indexOf(' ')) : defaultOrderBy.substring(indexOfDot + 1, defaultOrderBy.indexOf(' '))
                    defaultOrderAsc = defaultOrderBy.endsWith('desc') ? false : true;
                }

                for (let i = 0; i < displayables.length; i++) {
                    var column = displayables[i];

                    if (column.rendererParameters && column.rendererParameters.showsort && column.rendererParameters.showsort.equalsIc("false")) {
                        continue;
                    }
                    let sortOrder = true;
                    let isAdded = false;

                    const selected = $scope.initData.find(function (element, index, array) {
                        return element.columnName === column.attribute;
                    });
                    if (selected || (defaultColumn && column.attribute.equalsIc(defaultColumn))) {
                        sortOrder = selected ? selected.isAscending : defaultOrderAsc;
                        isAdded = true;
                    }
                    const sortColumn = {
                        name: getColumnName(column),
                        attribute: column.attribute,
                        columnName: column.attribute,
                        isAscending: sortOrder,
                        asc: sortOrder,
                        added: isAdded
                    };
                    $scope.columns.push(sortColumn);

                    if (isAdded) {
                        $scope.addedColumns.push(sortColumn);
                    }
                }
            };

            function clear() {
                for (let i = 0; i < $scope.addedColumns.length; i++) {
                    const column = $scope.addedColumns[i];
                    column.added = false;
                    column.asc = true;
                }

                $scope.addedColumns = [];
                $scope.selected = null;
            };

            function getColumnName(column) {
                return column.label || column.attribute;
            };

            function refreshGrid() {
                $scope.sort({ columns: $scope.initData });
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
                //this list will be kept uptodate
                initData: "=",
                //the method to be executed whenever the sort changes
                sort: "&",
                showdefaultsort: '=',
                initMethod: "&"
            },
            controller: "MultiSortController"
        };
        return directive;
    }
    ]);

})(angular);