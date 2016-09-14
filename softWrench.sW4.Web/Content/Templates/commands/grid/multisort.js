(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .controller("MultiSortController", ["$scope", '$timeout', function ($scope, $timeout) {     
            $scope.addedColumns = [];
            $scope.columns = [];
            $scope.selected = null;
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
                refreshGrid();
            };

            $scope.removeColumn = function (column) {
                column.added = false;
                $scope.addedColumns.splice($scope.addedColumns.indexOf(column), 1);
                refreshGrid();
            };

            $scope.addColumn = function (column) {
                if (column != null) {
                    $scope.selected.added = true;
                    $scope.addedColumns.push($scope.selected);
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
            
            function init() {
                if ($scope.schema && $scope.schema.nonHiddenFields) {
                    var defaultOrderBy = $scope.schema.properties['list.defaultorderby'];
                    var defaultOrderAsc = true;
                    var defaultColumn = '';

                    if (defaultOrderBy) {
                        var indexOfDot = defaultOrderBy.indexOf('.');
                        defaultColumn = (indexOfDot === -1) ? defaultOrderBy.substring(0, defaultOrderBy.indexOf(' ')) : defaultOrderBy.substring(indexOfDot + 1, defaultOrderBy.indexOf(' '))
                        defaultOrderAsc = defaultOrderBy.endsWith('desc') ? false : true;
                    }

                    for (var i = 0; i < $scope.schema.nonHiddenFields.length; i++) {
                        var column = $scope.schema.nonHiddenFields[i];

                        if (column.rendererParameters && column.rendererParameters.showsort && column.rendererParameters.showsort.equalsIc("false")) {
                            continue;
                        }

                        var sortOrder = true;
                        var isAdded = false;

                        var selected = $scope.initData.find(function (element, index, array) {
                            return element.columnName === column.attribute;
                        });

                        if (selected || (defaultColumn && column.attribute.equalsIc(defaultColumn))) {
                            sortOrder = selected ? selected.isAscending : defaultOrderAsc;
                            isAdded = true;
                        } 

                        var sortColumn = {
                            name: getColumnName(column),
                            attribute: column.attribute,
                            asc: sortOrder,
                            added: isAdded
                        };

                        $scope.columns.push(sortColumn);

                        if (isAdded) {
                            $scope.addedColumns.push(sortColumn);
                        }
                    }
                }
            };

            function clear() {
                for (var i = 0; i < $scope.addedColumns.length; i++) {
                    var column = $scope.addedColumns[i];
                    column.added = false;
                    column.asc = true;
                }

                $scope.addedColumns = [];
                $scope.selected = null;
            };

            function getColumnName (column){
                return column.label || column.attribute;
            };

            function refreshGrid() {
                $timeout(function () {
                    var columns = [];
                    $('#sortcolumns li').each(function (li) {
                        var attribute = $(this).find('span.attribute').text();
                        var column = $scope.addedColumns.find(function (element, index, array) {
                            return element.attribute === attribute;
                        });

                        if (column) {
                            columns.push({ columnName: column.attribute, isAscending: column.asc });
                        }
                    });

                    $scope.sort({ columns: columns });
                }, 500);
            };

            init();
        }
    ])
    .directive("multiSort", ["contextService", function (contextService) {
        var directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/commands/grid/multiSort.html"),
            scope: {
                schema: "=",
                initData:"=",
                sort: "&"
            },
            controller: "MultiSortController"
        };
        return directive;
    }
    ]);

})(angular);