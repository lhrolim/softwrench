(function (softwrench) {
    "use strict";

    softwrench.controller("CrudListSearchController", ["$log", "$scope", "crudContextHolderService", "crudContextService", "routeService", "crudSearchService", 
        function ($log, $scope, crudContextHolderService, crudContextService, routeService, crudSearchService) {

            // the search state before enter the search scheen
            // used in case the user cancels the search and the previous
            // data should be recoverd
            var auxSearchValues = {};
            var auxSort = {};

            //#region aux functions

            // used to convert date back from string on the aux JSON.stringfy -> JSON.parse 
            function dateTimeReviver(key, value) {
                if ((key !== "start" && key !== "startUTC" && key !== "end" && key !== "endUTC") || typeof value !== "string") {
                    return value;
                }
                return new Date(value);
            }

            // workaround to force UTC date on sort and search
            function searchDateChanged(field, start) {
                const searchValue = $scope.gridSearch.searchValues[field.value];
                if (!searchValue) {
                    return;
                }
                const dateValue = searchValue[start ? "start" : "end"];
                if (!dateValue) {
                    searchValue[start ? "startUTC" : "endUTC"] = null;
                    return;
                }

                searchValue[start ? "startUTC" : "endUTC"] = new Date(Date.UTC(dateValue.getFullYear(), dateValue.getMonth(), dateValue.getDate()));
            }
            //#endregion

            // clears all search fields
            $scope.clear = function () {
                crudSearchService.clearGridSearchValues();
            }

            $scope.cancel = function () {
                // reverts to the previous state
                $scope.gridSearch.searchValues = auxSearchValues;
                $scope.gridSearch.sort = auxSort;
                auxSearchValues = {};
                auxSort = {};
                routeService.go("^");
            }

            // do search
            $scope.apply = function () {
                crudContextService.refreshGrid();
            }

            $scope.sortChanged = function (newValue) {
                $scope.gridSearch.sort = $scope.gridSearch.sortables[newValue];
            }

            $scope.searchDateStartChanged = function (field) {
                searchDateChanged(field, true);
            }

            $scope.searchDateEndChanged = function (field) {
                searchDateChanged(field, false);
            }

            $scope.searchFieldLabel = function (field) {
                return field.label || field.value;
            }

            $scope.sortFieldLabel = function () {
                if (!$scope.gridSearch.sort) {
                    return null;
                }
                return $scope.gridSearch.sort.label || $scope.gridSearch.sort.value;
            }

            $scope.getSearchType = function (field) {
                if (field.type === "MetadataDateTimeFilter") {
                    return "date";
                }
                if (field.type === "MetadataOptionFilter") {
                    return "option";
                }
                if (field.dataType === "date") {
                    return "date";
                }
                return "default";
            }

            $scope.searchValue = function (field) {
                return $scope.gridSearch.searchValues[field.value];
            }

            $scope.init = function () {
                $scope.gridSearch = crudContextHolderService.getGridSearchData();

                // saves the initial state in case of cancel
                auxSearchValues = JSON.parse(JSON.stringify($scope.gridSearch.searchValues), dateTimeReviver);
                auxSort = JSON.parse(JSON.stringify($scope.gridSearch.sort));

                $scope.gridSearch = crudSearchService.initGridSearch();
            }

            $scope.init();
        }]);

})(softwrench);