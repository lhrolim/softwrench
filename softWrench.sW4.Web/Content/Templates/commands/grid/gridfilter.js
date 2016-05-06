(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .controller("GridFilterController", ["$scope", "$rootScope", "gridPreferenceService", "searchService", "i18NService", "alertService", "userPreferencesService", "crudContextHolderService",
            function ($scope, $rootScope, gridPreferenceService, searchService, i18NService, alertService, userPreferencesService, crudContextHolderService) {
                $scope.selectedfilter = null;

                $scope.filterChanged = function () {
                    crudContextHolderService.setSelectedFilter($scope.selectedfilter, $scope.panelid);
                    if (!$scope.selectedfilter) {
                        searchService.refreshGrid({}, null, { panelid: $scope.panelid, forcecleanup: true, addPreSelectedFilters: true });
                        return;
                    }
                    $scope.applyFilter($scope.selectedfilter);
                }

                $scope.nonSharedFilters = function () {
                    return gridPreferenceService.loadUserNonSharedFilters($scope.schema.applicationName, $scope.schema.schemaId);
                }

                $scope.i18N = function (key, defaultValue, paramArray) {
                    return i18NService.get18nValue(key, defaultValue, paramArray);
                };

                $scope.saveFilter = function () {
                    var saveFormSt = $("#savefilterform").prop("outerHTML");
                    //TODO: use angularjs?!
                    //remove display:none
                    saveFormSt = saveFormSt.replace("none", "");
                    //change id of the filter so that it becomes reacheable via jquery
                    saveFormSt = saveFormSt.replace("savefiltername", "savefiltername2");
                    bootbox.dialog({
                        templates: {
                            header:
                              "<div class='modal-header'>" +
                                "<i class='fa fa-question-circle'></i>" +
                                "<h4 class='modal-title'></h4>" +
                              "</div>"
                        },
                        message: saveFormSt,
                        title: "Save Filter",
                        buttons: {
                            cancel: {
                                label: $scope.i18N(".cancel", "Cancel"),
                                className: "btn btn-default",
                                callback: function () {
                                    return null;
                                }
                            },
                            main: {
                                label: $scope.i18N("_grid.filter.savefiltebtn", "Save"),
                                className: "btn-primary",
                                callback: function (result) {
                                    if (result) {
                                        $scope.createFilter($("#savefiltername2").val());
                                        $("#savefiltername2").val("");
                                    }
                                }
                            }
                        },
                        className: "smallmodal"
                    });
                }

                function hasAdvancedSearch() {
                    return $scope.quickSearchDto && $scope.quickSearchDto.quickSearchData;
                }

                $scope.shouldEnableSaveButton = function () {
                    // have saved filter selected
                    // or any filter applied
                    // or a quick search
                    return $scope.selectedfilter || $scope.hasFilterData() || hasAdvancedSearch();
                }

                $scope.hasFilterData = function () {
                    var searchData = $scope.searchData;
                    for (var data in searchData) {
                        if (!searchData.hasOwnProperty(data) || data === "lastSearchedValues") {
                            continue;
                        }
                        return searchData[data] !== "";
                    }
                    return false;
                }

                $scope.deleteFilter = function () {
                    var filter = $scope.selectedfilter;
                    alertService.confirm(null, null, function () {
                        gridPreferenceService.deleteFilter(filter.id, filter.creatorId, function () {
                            $scope.selectedfilter = null;
                        });
                    }, "Are you sure that you want to remove filter {0}?".format(filter.alias), null);
                }

                $scope.createFilter = function (alias) {
                    var id = $scope.selectedfilter ? $scope.selectedfilter.id : null;
                    var owner = $scope.selectedfilter ? $scope.selectedfilter.creatorId : null;
                    var advancedSearch = hasAdvancedSearch() ? JSON.stringify($scope.quickSearchDto) : null;

                    gridPreferenceService.saveFilter($scope.schema, $scope.searchData, $scope.searchTemplate, $scope.searchOperator, advancedSearch, alias, id, owner,
                        function (filter) {
                            $scope.selectedfilter = filter;
                        });
                }

                $scope.applyFilter = function (filter) {
                    $scope.quickSearchDto = filter.advancedSearch ? JSON.parse(filter.advancedSearch) : { compositionsToInclude: [] };
                    gridPreferenceService.applyFilter(filter, $scope.searchOperator, $scope.quickSearchDto, $scope.panelid);
                }

                function clearFilter(panelid) {
                    if ($scope.panelid !== panelid) {
                        return;
                    }
                    $scope.selectedfilter = null;
                }

                $scope.$on("sw.grid.refresh", function (event, panelid) {
                    clearFilter(panelid);
                });

                $scope.$on("sw_redirectapplicationsuccess", function (event) {
                    $scope.selectedfilter = null;
                });
            }
        ])
        .directive("gridFilter", ["contextService", function (contextService) {
            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Templates/commands/grid/gridfilter.html"),
                scope: {
                    schema: "=",
                    searchData: "=",
                    searchOperator: "=",
                    searchTemplate: "=",
                    panelid: "=",
                    quickSearchDto: "="
                },
                controller: "GridFilterController"
            };
            return directive;
        }
        ]);

})(angular);