(function (softwrench) {
    "use strict";

    softwrench.controller("CrudListSearchController", ["$log", "$scope", "crudContextHolderService", "crudContextService", "routeService", "offlineAssociationService", "fieldService", "dispatcherService",
        function ($log, $scope, crudContextHolderService, crudContextService, routeService, offlineAssociationService, fieldService, dispatcherService) {


            const ascIcon = "android-arrow-up";
            const descIcon = "android-arrow-down";

            // the search state before enter the search scheen
            // used in case the user cancels the search and the previous
            // data should be recoverd
            var auxSearchValues = {};
            var auxSort = {};

            //#region aux functions

            // builds the label for a multiple option field
            function getOptionLabels(options, value) {
                if (!value || !options) {
                    return null;
                }
                var labels = [];
                const valuesArray = value.split(";");
                angular.forEach(valuesArray, currentValue => {
                    angular.forEach(options, option => {
                        if (option.value === currentValue) {
                            labels.push(option.label);
                        }
                    });
                });
                return labels.join(" & ");
            }

            // creates a search field data
            function createSearchable(filter, schema) {
                const displayables = schema.displayables;
                var field = {};
                angular.forEach(displayables, displayable => {
                    if (displayable.attribute === filter.attribute) {
                        field = displayable;
                    }
                });

                const searchable = {
                    label: filter.label,
                    value: filter.attribute,
                    type: filter.type,
                    dataType: field.dataType
                }

                if (filter.type !== "MetadataOptionFilter") {
                    return searchable;
                }

                searchable.whereClause = filter.whereClause;
                
                if (!filter.provider) {
                    searchable.options = filter.options;
                    if (searchable.options) {
                        angular.forEach(searchable.options, option => {
                            option.text = option.label || option.value;
                        });
                    }
                    searchable.optionChanged = $scope.searchOptionChangedFunction(filter.attribute, searchable.options);
                    return searchable;
                }

                searchable.options = [];
                searchable.optionChanged = $scope.searchOptionChangedFunction(filter.attribute, searchable.options);

                // a service provides the options
                if (filter.provider.startsWith("@")) {
                    const serviceString = filter.provider.substring(1);
                    const options = dispatcherService.invokeServiceByString(serviceString);
                    searchable.options.push(...options);
                    return searchable;
                }

                // options are provided by a relathionship
                // TODO: for now is required a association on detail schema to force the sync of the data for use on the option filter
                const parentSchema = crudContextHolderService.currentDetailSchema();
                const providerTokens = filter.provider.split(".");
                const associationName = providerTokens[0];
                const assocDisplayables = fieldService.getDisplayablesByAssociationKey(parentSchema, associationName);
                if (!assocDisplayables || assocDisplayables.length === 0) {
                    return searchable;
                }
                const labelField = providerTokens[1];
                const assocDisplayable = assocDisplayables[0];
                offlineAssociationService.filterPromise(parentSchema, {}, associationName, null, assocDisplayable).then(assocs => {
                    angular.forEach(assocs, assoc => {
                        const option = {
                            label: assoc.datamap[labelField],
                            value: assoc.datamap[assocDisplayable.valueField]
                        }
                        option.text = option.label || option.value;
                        searchable.options.push(option);
                    });
                });

                return searchable;
            }

            // creates a sort field data
            function createSortable(filter, asc) {
                return {
                    label: filter.label,
                    value: sortKey(filter.attribute, asc),
                    attribute: filter.attribute,
                    afterLabelIcon: asc ? ascIcon : descIcon,
                    direction: asc ? "asc" : "desc"
                }
            }

            // used to convert date back from string on the aux JSON.stringfy -> JSON.parse 
            function dateTimeReviver(key, value) {
                if ((key !== "start" && key !== "startUTC" && key !== "end" && key !== "endUTC") || typeof value !== "string") {
                    return value;
                }
                return new Date(value);
            }

            // create a fild key to enable two sort field from the same attribute 
            function sortKey(attribute, asc) {
                return attribute + "." + (asc ? "asc" : "desc");
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
                crudContextHolderService.clearGridSearchValues();
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

            $scope.searchOptionChangedFunction = function (attribute, options) {
                if (!options) {
                    return null;
                }
                return function (newValue) {
                    // updates the label considering the selected options
                    const searchValue = $scope.gridSearch.searchValues[attribute];
                    if (!searchValue) {
                        return;
                    }
                    searchValue.value = newValue;
                    searchValue.label = getOptionLabels(options, newValue);
                }
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

                // verifies if the search structured is already built
                const searchFields = $scope.gridSearch.searchFields;
                if (Object.keys(searchFields).length > 1) {
                    return;
                }

                // verifies if there are existing filters
                const schema = crudContextHolderService.currentListSchema();
                const filters = schema.schemaFilters;
                if (!filters || !filters.filters) {
                    return;
                }

                // builds search and sort structure
                angular.forEach(filters.filters, filter => {
                    $scope.gridSearch.searchFields[filter.attribute] = createSearchable(filter, schema);
                    if (!$scope.gridSearch.searchValues[filter.attribute]) {
                        $scope.gridSearch.searchValues[filter.attribute] = {};
                    }
                    const asc = createSortable(filter, true);
                    $scope.gridSearch.sortables[asc.value] = asc;
                    $scope.gridSearch.sortableFields.push(asc);
                    const desc = createSortable(filter, false);
                    $scope.gridSearch.sortables[desc.value] = desc;
                    $scope.gridSearch.sortableFields.push(desc);
                });
            }

            $scope.init();
        }]);

})(softwrench);