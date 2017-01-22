(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('gridPreferenceService', function (contextService, restService, $log, searchService, checkpointService, configurationService) {
            "ngInject";

            function compareFilters(a, b) {
                if (a.alias < b.alias) {
                    return -1;
                }
                if (a.alias > b.alias) {
                    return 1;
                }
                return 0;
            }

            function doLoadFilter(shared, application, schema) {
                const schemaId = schema.schemaId;

             

                var user = contextService.getUserData();
                const gridpreferences = user.gridPreferences;
                var result = [];

                const declaredQuickFilters = schema.schemaFilters.quickSearchFilters;
                if (declaredQuickFilters) {
                    declaredQuickFilters.forEach(d => {
                        d.deletable = false;
                        result.push(d);
                    });
                }

                if (!gridpreferences) {
                    return result;
                }
                const filters = gridpreferences.gridFilters;
                var log = $log.getInstance("#gridpreferenceservice#doLoadFilter",["filter"]);

                filters.forEach(association => {
                    if (association.filter.application.equalIc(application)
                        && association.filter.schema.equalIc(schemaId)
                        && ((association.filter.creatorId === user.dbId) ^ shared)) {
                        log.debug("loading filter {0}; shared?".format(association.filter.alias, shared));
                        result.push(association.filter);
                    }
                });

                result = result.sort(compareFilters);
                const previousFilter = checkpointService.getCheckPointAsFilter(application,schemaId);
                if (previousFilter != null && previousFilter !== {}) {
                    log.debug(`Adding previous filter for application ${application} and schema ${schemaId}: ${previousFilter}`);
                    result.push(previousFilter);
                }
                return result;
            }

            return {

                loadUserSharedFilters: function (application, schema) {
                    return doLoadFilter(true, application, schema);
                },

                loadUserNonSharedFilters: function (application, schema) {
                    return doLoadFilter(false, application, schema);
                },

                /**
                 * Called when the a grid renders, deciding whether to apply the previous unsaved filter to it, depending of the server side configuration
                 * @param {} application 
                 * @returns {} 
                 */
                getPreviousFilterDto: function(applicationName,schemaId,panelId) {
                    const log = $log.get("gridPreferenceService#applyPreviousFilterIfNeeded", ["filter"]);
                    const applyDefault = configurationService.getConfigurationValue("/Global/Grid/Filter/ApplyDefaultPrevious");
                    if (applyDefault === "true") {
                        const previousFilter = checkpointService.getCheckPointAsFilter(applicationName, schemaId);
                        if (previousFilter) {
                            log.debug(`applying default filter for ${applicationName} ${previousFilter}`);
                            return previousFilter;
                        }
                    }
                    return null;
                },

                hasFilter: function (application, schema) {
                    const user = contextService.getUserData();
                    const preferences = user.gridPreferences;
                    const associations = preferences.gridFilters;
                    const filters = $.grep(associations, function (e) {
                        return e.filter.application == application && e.filter.schema == schema;
                    });
                    return filters.length > 0;
                },

                saveFilter: function (schema, searchData, template, searchOperators, searchSort, advancedSearch, alias, id, filterowner, successCbk) {
                    var fields = "";
                    var operators = "";
                    var values = "";
                    var user = contextService.getUserData();
                    const log = $log.getInstance("#gridpreferenceservice#savefilter");
                    for (let data in searchData) {
                        if (data == "lastSearchedValues") {
                            continue;
                        }
                        fields += data + ",";
                        values += searchData[data] + ",,,";
                        const operator = searchOperators[data];
                        // the operators are only setted if the user specific select them, but they should be 'C' as default --> avoiding inconsistency
                        operators += (operator == null ? 'C' : operator.symbol) + ",";
                    }
                    const parameters = {
                        application: schema.applicationName,
                        schema: schema.schemaId,
                        fields: fields.substr(0, fields.length - 1),
                        operators: operators.substr(0, operators.length - 1),
                        values: values.substr(0, values.length - 3),
                        template: template,
                        alias: alias,
                        advancedSearch: advancedSearch,
                        id: id
                    };
                    if (id == null || filterowner != user.dbId) {
                        log.debug("creating new filter {0}".format(JSON.stringify(parameters)));
                        restService.invokePost("GridFilter", "CreateNewFilter", parameters, null, function (resultdata) {
                            const filterassociation = resultdata.resultObject;
                            const preferences = user.gridPreferences;
                            const filters = preferences.gridFilters;
                            filters.push(filterassociation);
                            successCbk(filterassociation.filter);
                        });
                    } else {
                        log.debug("updating filter {0}".format(JSON.stringify(parameters)));
                        restService.invokePost("GridFilter", "UpdateFilter", parameters, null, function (resultdata) {
                            const preferences = user.gridPreferences;
                            const associations = preferences.gridFilters;
                            const filter = resultdata.resultObject;
                            for (let i = 0; i < associations.length; i++) {
                                if (associations[i].filter.id == filter.id) {
                                    associations[i].filter = filter;
                                }
                            }
                            successCbk(filter);
                        });
                    }
                },

                deleteFilter: function (filterId, creatorId, cbk) {
                    const parameters = {
                        filterId: filterId,
                        creatorId: creatorId,
                    };
                    var log = $log.getInstance("#gridpreferenceservice#deletefilter");
                    var user = contextService.getUserData();
                    restService.invokePost("GridFilter", "DeleteFilter", parameters, null, function (resultdata) {
                        const gridpreferences = user.gridPreferences;
                        var association = resultdata.resultObject;
                        log.debug("removing filter {0}".format(association.filter.alias));
                        gridpreferences.gridFilters = $.grep(gridpreferences.gridFilters, function (value) {
                            return value.id != association.id;
                        });
                        cbk();
                    });
                },

        applyFilter: function (filter, searchOperator = {}, quickSearchDTO = {}, panelid = null) {
            let searchDto = filter.searchDTO;
            if (!searchDto) {
                searchDto = this.buildSearchDTOFromFilter(filter, searchOperator, quickSearchDTO, panelid);
            }
            let wrappedDTO = searchDto;
            if (!(searchDto instanceof SearchDTO)) {
                wrappedDTO = new SearchDTO(searchDto);
            }
            const log = $log.getInstance("#gridpreferenceservice#applyFilter",["filter"]);
            log.debug(`applying filter  ${wrappedDTO}`);
            return searchService.refreshGrid(searchDto.searchData, searchDto.searchOperator, wrappedDTO);
        },

        buildSearchDTOFromFilter: function (filter, searchOperator = {}, quickSearchDTO = {}, panelid = null) {
            const searchData = {};
            if (filter.fields) {
                const fieldsArray = filter.fields.split(",");
                const operatorsArray = filter.operators.split(",");
                const valuesArray = filter.values.split(",,,");
                for (let i = 0; i < fieldsArray.length; i++) {
                    const field = fieldsArray[i];
                    searchData[field] = valuesArray[i];
                    searchOperator[field] = searchService.getSearchOperationBySymbol(operatorsArray[i]);
                }
            } else {
                searchOperator = {};
            }
            const template = filter.template;
            const searchSort = filter.searchSort || {};

            const schemaFilterId = filter.type === "QuickSearchFilter"  ? filter.id : null;

            return {searchData,searchOperator, searchTemplate: template, quickSearchDTO, panelid, searchSort,schemaFilterId};
        }

};

});

})(angular);