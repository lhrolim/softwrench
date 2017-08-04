(function (mobileServices) {
    "use strict";


    class crudSearchService {
        constructor(crudContextHolderService, dispatcherService, offlineAssociationService, fieldService) {
            this.crudContextHolderService = crudContextHolderService;
            this.dispatcherService = dispatcherService;
            this.offlineAssociationService = offlineAssociationService;
            this.fieldService = fieldService;

            const ascIcon = "android-arrow-up";
            const descIcon = "android-arrow-down";

            // builds the label for a multiple option field
            this.getOptionLabels = function (options, value) {
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

            // create a fild key to enable two sort field from the same attribute 
            this.sortKey = function (attribute, asc) {
                return attribute + "." + (asc ? "asc" : "desc");
            }

            this.searchOptionChangedFunction = function (attribute, options) {
                if (!options) {
                    return null;
                }
                return (newValue) => {
                    // updates the label considering the selected options
                    const gridSearch = this.crudContextHolderService.getGridSearchData();
                    const searchValue = gridSearch.searchValues[attribute];
                    if (!searchValue) {
                        return;
                    }
                    searchValue.value = newValue;
                    searchValue.label = this.getOptionLabels(options, newValue);
                }
            }

            // creates a search field data
            this.createSearchable = function (searchFields, filter, schema) {
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

                searchFields[filter.attribute] = searchable;

                if (filter.type !== "MetadataOptionFilter") {
                    return;
                }

                searchable.whereClause = filter.whereClause;

                if (!filter.provider) {
                    searchable.options = filter.options;
                    if (searchable.options) {
                        angular.forEach(searchable.options, option => {
                            option.text = option.label || option.value;
                        });
                    }
                    searchable.optionChanged = this.searchOptionChangedFunction(filter.attribute, searchable.options);
                    return;
                }

                searchable.options = [];
                searchable.optionChanged = this.searchOptionChangedFunction(filter.attribute, searchable.options);

                // a service provides the options
                if (filter.provider.startsWith("@")) {
                    const serviceString = filter.provider.substring(1);
                    const options = this.dispatcherService.invokeServiceByString(serviceString);
                    searchable.options.push(...options);
                    return;
                }

                // options are provided by a relathionship
                // TODO: for now is required a association on detail schema to force the sync of the data for use on the option filter
                const parentSchema = this.crudContextHolderService.currentDetailSchema();
                const providerTokens = filter.provider.split(".");
                const associationName = providerTokens[0];
                const assocDisplayables = this.fieldService.getDisplayablesByAssociationKey(parentSchema, associationName);
                if (!assocDisplayables || assocDisplayables.length === 0) {
                    return;
                }
                const labelField = providerTokens[1];
                const assocDisplayable = assocDisplayables[0];
                this.offlineAssociationService.filterPromise(parentSchema, {}, associationName, null, assocDisplayable).then(assocs => {
                    angular.forEach(assocs, assoc => {
                        const option = {
                            label: assoc.datamap[labelField],
                            value: assoc.datamap[assocDisplayable.valueField]
                        }
                        option.text = option.label || option.value;
                        searchable.options.push(option);
                    });
                });
            }

            // creates a sort field data
            this.createSortable = function (filter, asc) {
                return {
                    label: filter.label,
                    value: this.sortKey(filter.attribute, asc),
                    attribute: filter.attribute,
                    afterLabelIcon: asc ? ascIcon : descIcon,
                    direction: asc ? "asc" : "desc"
                }
            }

            // sets the pre selected values from a option filter
            this.setPreSelectedValue = function(gridSearch, filter) {
                if (filter.type !== "MetadataOptionFilter") {
                    return;
                }

                const searchable = gridSearch.searchFields[filter.attribute];
                if (!searchable) {
                    return;
                }

                if (filter.preselected) {
                    searchable.optionChanged(filter.preselected);
                    return;
                }
                
                const preselectedValues = [];
                angular.forEach(searchable.options, option => {
                    if (option.preSelected) {
                        preselectedValues.push(option.value);
                    }
                });
                if (preselectedValues.length > 0) {
                    searchable.optionChanged(preselectedValues.join(";"));
                }
            }

            // sets the pre selected values from all option filters
            this.setPreSelectedValues = function () {
                const gridSearch = this.crudContextHolderService.getGridSearchData();
                const schema = this.crudContextHolderService.currentListSchema();
                const filters = schema.schemaFilters;
                if (!filters || !filters.filters) {
                    return;
                }

                angular.forEach(filters.filters, filter => {
                    this.setPreSelectedValue(gridSearch, filter);
                });
            }
        }

        initGridSearch() {
            const gridSearch = this.crudContextHolderService.getGridSearchData();

            // verifies if the search structured is already built
            const searchFields = gridSearch.searchFields;
            if (Object.keys(searchFields).length > 1) {
                return gridSearch;
            }

            // verifies if there are existing filters
            const schema = this.crudContextHolderService.currentListSchema();
            const filters = schema.schemaFilters;
            if (!filters || !filters.filters) {
                return gridSearch;
            }

            // builds search and sort structure
            angular.forEach(filters.filters, filter => {
                if (!gridSearch.searchValues[filter.attribute]) {
                    gridSearch.searchValues[filter.attribute] = {};
                }
                this.createSearchable(gridSearch.searchFields, filter, schema);
                this.setPreSelectedValue(gridSearch, filter);

                const asc = this.createSortable(filter, true);
                gridSearch.sortables[asc.value] = asc;
                gridSearch.sortableFields.push(asc);
                const desc = this.createSortable(filter, false);
                gridSearch.sortables[desc.value] = desc;
                gridSearch.sortableFields.push(desc);
            });

            return gridSearch;
        }

        // clears the search and sort values and sets the preselected values
        clearGridSearchValues() {
            this.crudContextHolderService.clearGridSearchValues();
            this.setPreSelectedValues();
        }
    }

    crudSearchService["$inject"] = ["crudContextHolderService", "dispatcherService", "offlineAssociationService", "fieldService"];

    mobileServices.service("crudSearchService", crudSearchService);

})(mobileServices)
