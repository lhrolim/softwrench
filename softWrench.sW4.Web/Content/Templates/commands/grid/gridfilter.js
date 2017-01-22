(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .controller("GridFilterController", ["$scope", "$rootScope", "gridPreferenceService", "searchService", "i18NService", "alertService", "userPreferencesService", "crudContextHolderService","$log",

            function ($scope, $rootScope, gridPreferenceService, searchService, i18NService, alertService, userPreferencesService, crudContextHolderService, $log) {
                $scope.selectedfilter = null;
                const log = $log.get("gridfilter#filterchanged", ["filter"]);


                $scope.filterChanged = function () {
                    crudContextHolderService.setSelectedFilter($scope.selectedfilter, $scope.panelid);
                    if (!$scope.selectedfilter) {
                        log.debug("selected filter not defined, applying blank filter");
                        searchService.refreshGrid({}, null, { panelid: $scope.panelid, forcecleanup: true, addPreSelectedFilters: true });
                        return;
                    }
                    $scope.applyFilter($scope.selectedfilter);
                }

                $scope.nonSharedFilters = function () {
                    if ($scope.cachedFilters) {
                        return $scope.cachedFilters;
                    }
                    $scope.cachedFilters = gridPreferenceService.loadUserNonSharedFilters($scope.schema.applicationName, $scope.schema);
                    if ($scope.selectedfilter) {
                        const matchingcachedFilter = $scope.cachedFilters.find(f => f.id === $scope.selectedfilter.id);
                        if (matchingcachedFilter)
                            matchingcachedFilter["$$hashKey"] = $scope.selectedfilter["$$hashKey"];
                    }

                    return $scope.cachedFilters;
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
                        onEscape:true,
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
                    $scope.cachedFilters = null;
                }

                function hasAdvancedSearch() {
                    return $scope.quickSearchDto && $scope.quickSearchDto.quickSearchData;
                }

                $scope.shouldEnableSaveButton = function () {
                    // have saved filter selected
                    // or any filter applied
                    // or a quick search
                    return ($scope.selectedfilter && $scope.selectedfilter.deletable) || $scope.hasFilterData() || hasAdvancedSearch();
                }

                $scope.shouldEnableDeleteButton = function () {
                    const filter = $scope.selectedfilter;
                    // have saved filter selected
                    // or any filter applied
                    // or a quick search
                    return !!filter && (filter.id !== -2 && filter.deletable);
                }

                $scope.hasFilterData = function () {
                    const searchData = $scope.searchData;
                    for (let data in searchData) {
                        if (!searchData.hasOwnProperty(data) || data === "lastSearchedValues") {
                            continue;
                        }
                        return searchData[data] !== "";
                    }
                    return false;
                }

                $scope.deleteFilter = function () {
                    var filter = $scope.selectedfilter;
                    if (filter["id"] === -2) {
                        alertService.notifyWarning({ "warningDto": { "warnMessage": "The Previous Unsaved Filter cannot be deleted." } });
                        return;
                    }
                    alertService.confirm("Are you sure that you want to remove filter {0}?".format(filter.alias)).then(function () {
                        gridPreferenceService.deleteFilter(filter.id, filter.creatorId, function () {
                            $scope.selectedfilter = null;
                            $scope.cachedFilters = null;
                        });
                    });

                }

                $scope.createFilter = function (alias) {
                    const id = $scope.selectedfilter ? $scope.selectedfilter.id : null;
                    const owner = $scope.selectedfilter ? $scope.selectedfilter.creatorId : null;
                    const advancedSearch = hasAdvancedSearch() ? JSON.stringify($scope.quickSearchDto) : null;
                    gridPreferenceService.saveFilter($scope.schema, $scope.searchData, $scope.searchTemplate, $scope.searchOperator, null, advancedSearch, alias, id, owner, (filter) => {
                        $scope.selectedfilter = filter;
                        $scope.cachedFilters = null;
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

                $scope.$on(JavascriptEventConstants.GRID_REFRESH2, function (event, panelid) {
                    clearFilter(panelid);
                    $scope.cachedFilters = null;
                });

                $scope.$on(JavascriptEventConstants.REDIRECT_AFTER, function (event, data) {
                    $scope.selectedfilter = null;
                    $scope.cachedFilters = null;
                });

                $scope.$on(JavascriptEventConstants.GRID_SETFILTER, function (event, filter) {
                    $scope.cachedFilters = null;
                    $scope.selectedfilter = filter;

                });
            }
        ])
        .directive("gridFilter", ["contextService", function (contextService) {
            const directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Templates/commands/grid/gridfilter.html"),
                scope: {
                    schema: "=",
                    searchData: "=",
                    searchOperator: "=",
                    searchTemplate: "=",
                    panelid: "=",
                    quickSearchDto: "=",
                    toggleMultiSortPanel: "&"
                },
                controller: "GridFilterController"
            };
            return directive;
        }
        ]);

})(angular);