(function (angular) {
    "use strict";
    var module = angular.module('sw_layout');

  
    module.factory('searchService', function (i18NService, $log, $rootScope, contextService, fieldService, $http, historyService) {
        "ngInject";


        var objCache = {};

        function isEmpty(value) {
            return value == null || value === "" || value.length === 0;
        }

        function getSearchValue(value) {
            if (!value) {
                return null;
            }
            //remove leading and trailing wildcards, but not all of them
            if (value[0] === "%") {
                value = value.substring(1);
            } else if (value[0] === "!" && value[1] === "%") {
                value = value.substring(2);
            }

            if (value[value.length - 1] === "%") {
                value = value.substring(0, value.length - 1);
            }
            value = replaceAll(value, '>', '');
            value = replaceAll(value, '=', '');
            value = replaceAll(value, '<', '');
            value = replaceAll(value, '!', '');
            value = replaceAll(value, '@BLANK', '');
            return value;
        };

        //Update the Filter lables upon change of language.
        function buildArray() {
            var filtertype = {};
            var filterkeyaux = '_grid.filter.filtertype.';
            filtertype.nofilter = i18NService.get18nValue(filterkeyaux + 'nofilter', 'No Filter');
            filtertype.filter = i18NService.get18nValue(filterkeyaux + 'filter', 'Filter');
            filtertype.contains = i18NService.get18nValue(filterkeyaux + 'contains', 'Contains');
            filtertype.ncontains = i18NService.get18nValue(filterkeyaux + 'ncontains', 'Does Not Contain');
            filtertype.startwith = i18NService.get18nValue(filterkeyaux + 'startwith', 'Starts With');
            filtertype.endwith = i18NService.get18nValue(filterkeyaux + 'endwith', 'Ends With');
            filtertype.eq = i18NService.get18nValue(filterkeyaux + 'eq', 'Equal To');
            filtertype.noteq = i18NService.get18nValue(filterkeyaux + 'noteq', 'Not Equal To');
            filtertype.blank = i18NService.get18nValue(filterkeyaux + 'blank', 'Blank');
            filtertype.gt = i18NService.get18nValue(filterkeyaux + 'gt', 'Greater Than');
            filtertype.lt = i18NService.get18nValue(filterkeyaux + 'lt', 'Less Than');
            filtertype.gte = i18NService.get18nValue(filterkeyaux + 'gte', 'Greater Than Or Equal To');
            filtertype.lte = i18NService.get18nValue(filterkeyaux + 'lte', 'Less Than Or Equal To');
            filtertype.btw = i18NService.get18nValue(filterkeyaux + 'btw', 'Between');
            filtertype.custom = i18NService.get18nValue(filterkeyaux + 'custom', 'Custom');

            var searchOperations = [
            { id: "", symbol: "", title: filtertype.nofilter, tooltip: filtertype.filter, begin: "", end: "", renderType: ["combo", "default", "datetime"], },
            { id: "CONTAINS", symbol: "C", title: filtertype.contains, tooltip: filtertype.contains, begin: "%", end: "%", renderType: ["combo", "default"], datatype: ['varchar'] },
            { id: "EQ", symbol: "=", title: filtertype.eq, tooltip: filtertype.eq, begin: "=", end: "", renderType: ["default", "datetime"] },
            { id: "NCONTAINS", symbol: "!C", title: filtertype.ncontains, tooltip: filtertype.ncontains, begin: "!%", end: "%", renderType: ["combo", "default"], datatype: ['varchar'] },
            { id: "STARTWITH", symbol: "ST", title: filtertype.startwith, tooltip: filtertype.startwith, begin: "", end: "%", renderType: ["default"], datatype: ['varchar'] },
            { id: "ENDWITH", symbol: "END", title: filtertype.endwith, tooltip: filtertype.endwith, begin: "%", end: "", renderType: ["default"], datatype: ['varchar'] },
    //        { id: "BTW", symbol: "-", title: filtertype.btw, tooltip: filtertype.btw, begin: ">=", end: "<=", renderType: [] },
            { id: "NOTEQ", symbol: "!=", title: filtertype.noteq, tooltip: filtertype.noteq, begin: "!=", end: "", renderType: ["default", "datetime"] },
            { id: "BLANK", symbol: "BLANK", title: filtertype.blank, tooltip: filtertype.blank, begin: "", end: "", renderType: ["default", "datetime", "combo"] },
            { id: "GT", symbol: ">", title: filtertype.gt, tooltip: filtertype.gt, begin: ">", end: "", renderType: ["default", "datetime"] },
            { id: "LT", symbol: "<", title: filtertype.lt, lt: filtertype.lt, begin: "<", end: "", renderType: ["default", "datetime"] },
            { id: "GTE", symbol: ">=", title: filtertype.gte, tooltip: filtertype.gte, begin: ">=", end: "", renderType: ["default", "datetime"] },
            { id: "LTE", symbol: "<=", title: filtertype.lte, tooltip: filtertype.lte, begin: "<=", end: "", renderType: ["default", "datetime"] },
            { id: "BTW", symbol: ">=", title: filtertype.btw, tooltip: filtertype.btw, begin: "", end: "" },
            { id: "CUSTOM", symbol: "??", title: filtertype.custom, tooltip: filtertype.custom, begin: "", end: "" },

            ];
            return searchOperations;
        };

        var buildSearchParamsString = function (searchData, searchOperator, useOrOperator) {
            var operatorToUse = useOrOperator ? "||" : "&&";
            var resultString = "";
            for (var data in searchData) {
                if (data === "lastSearchedValues") {
                    //exclude this field which is used only to control the  needsCountUpdate flag
                    continue;
                }

                if (!isEmpty(searchData[data]) || (searchOperator[data] != null && searchOperator[data].id === "BLANK")) {
                    if (!searchOperator[data]) {
                        continue;
                    }

                    if (data.indexOf('___') !== -1) {
                        // this case is only for "BETWEEN" operator
                        data = data.substring(0, data.indexOf('___'));
                        if (resultString.indexOf(data) !== -1) {
                            resultString += data + operatorToUse;
                        } else {
                            resultString += data + "___";
                        }
                        continue;
                    }

                    resultString += data + operatorToUse;
                }
            }
            return resultString.substring(0, resultString.lastIndexOf(operatorToUse));
        };

        var buildSearchSortString = function (searchSort) {
            //            var searchSort = scope.searchSort;
            var resultString = "";

            if (searchSort.field != null && searchSort.field != '') {
                resultString = searchSort.field;
            }
            return resultString;
        };

        var specialCharactersHandler = function (searchData, searchOperator) {
            var specialcharacter = "*";
            for (var data in searchData) {
                if (!searchData.hasOwnProperty(data)) continue;
                if (searchData[data] == null || searchData[data] == '' || data == "lastSearchedValues") {
                    continue;
                }
                var search = searchData[data];
                if (!angular.isString(search)) return; // -> numeric inputs
                if (search.indexOf(specialcharacter) > -1) {
                    var indexSearchOperator = null;
                    var searchreplaced = search.replace(/\*/g, '');
                    if (search.indexOf(specialcharacter) == 0 && search.lastIndexOf(specialcharacter) == search.length - specialcharacter.length) {
                        indexSearchOperator = 1; /* contains */
                    } else if (search.indexOf(specialcharacter) == 0) {
                        indexSearchOperator = 3; /* start with */
                    } else if (search.lastIndexOf(specialcharacter) == search.length - specialcharacter.length) {
                        indexSearchOperator = 4; /* end with */
                    }
                    searchData[data] = searchreplaced;
                    if (indexSearchOperator != null) {
                        searchOperator[data] = buildArray()[indexSearchOperator];
                    }
                }
            }
        };

        return {
            //TODO: dictionary?
            getSearchOperator: function (value) {
                if (value.startsWith('>')) {
                    if (value.startsWith('>=')) {
                        if (value.endsWith('<=')) {
                            return this.getSearchOperationById('BTW');
                        }
                        return this.getSearchOperationById('GTE');
                    }
                    return this.getSearchOperationById('GT');
                }
                if (value.startsWith('<=')) {
                    return this.getSearchOperationById('LTE');
                }
                if (value.startsWith('<')) {
                    return this.getSearchOperationById('LT');
                }
                if (value.startsWith("!") && value.endsWith("%")) {
                    return this.getSearchOperationById('NCONTAINS');
                }
                if (value.startsWith('%')) {
                    if (value.endsWith('%')) {
                        return this.getSearchOperationById('CONTAINS');
                    }
                    return this.getSearchOperationById('ENDWITH');
                }
                if (value.endsWith("%")) {
                    return this.getSearchOperationById('STARTWITH');
                }
                if (value.startsWith("=")) {
                    return this.getSearchOperationById('EQ');
                }
                if (value.startsWith("!=")) {
                    return this.getSearchOperationById('NOTEQ');
                }
                if (value === "!@BLANK") {
                    return this.getSearchOperationById('BLANK');
                }
                if (value.indexOf("__") !== -1) {
                    return this.getSearchOperationById('BTW');
                }

                return this.getSearchOperationById('CUSTOM');
            },


            buildSearchValuesString: function (searchData, searchOperator) {
                var resultString = "";
                var value = "";
                var beginAlreadySet = false;
                for (var data in searchData) {
                    if (searchOperator[data] == null) {
                        continue;
                    }


                    if ((isEmpty(searchData[data]) || data === "lastSearchedValues") &&
                        (searchOperator[data] == null || searchOperator[data].id !== "BLANK")) {
                        continue;
                    }


                    if (searchOperator[data].id === 'BTW') {
                        value = searchData[data] + "__" + searchData[data + "_end"];
                    } else {
                        value = searchData[data];
                        if (data.indexOf('___') != -1) {
                            data = data.substring(0, data.indexOf('___'));
                        }
                        if (searchOperator[data] == null) {
                            searchOperator[data] = this.defaultSearchOperation();
                        }
                        if (searchOperator[data].begin !== '' && !beginAlreadySet) {
                            value = searchOperator[data].begin + value;
                            if (searchOperator[data].id === 'BTW') {
                                beginAlreadySet = true;
                                resultString += value + "___";
                                continue;
                            }
                        }
                        if (searchOperator[data].end !== '') {
                            if (searchOperator[data].id === 'BTW') {
                                value = searchOperator[data].end + value;
                                beginAlreadySet = false;
                            }
                            else {
                                value = value + searchOperator[data].end;
                            }
                        }
                        if (searchOperator[data] != null && searchOperator[data].id === 'BLANK') {
                            value = '!@BLANK';
                        }
                    }
                    resultString += value + ",,,";
                }
                resultString = resultString.substring(0, resultString.lastIndexOf(",,,"));

                return resultString;
            },

            /// <summary>
            /// 
            /// Builds the server side PaginatedSearchDTO object gathering data from the screen arrays
            /// 
            /// </summary>
            /// <param name="searchData">the array of filter entries</param>
            /// <param name="searchSort">a sort property to apply</param>
            /// <param name="searchOperator">the array of filter operations, in the same order of searchData</param>
            /// <param name="filterFixedWhereClause">a fallback query applied.it could be null</param>
            /// <param name="paginationData">an object containing pageNumber and pageSize properties</param>
            /// <param name="searchTemplate">a string containing the query, with the parameters delimited by : (ex: :a || :b || :c || :d) </param>
            /// <returns type=""></returns>        
            buildSearchDTO: function (searchData, searchSort, searchOperator, filterFixedWhereClause, paginationData, searchTemplate, quicksearchDTO) {
                var searchDto = {};
                searchData = searchData || {};
                searchSort = searchSort || {};
                searchOperator = searchOperator || {};
                searchDto.searchParams = buildSearchParamsString(searchData, searchOperator);
                specialCharactersHandler(searchData, searchOperator);
                searchDto.searchValues = this.buildSearchValuesString(searchData, searchOperator);
                searchDto.searchSort = buildSearchSortString(searchSort);
                searchDto.SearchAscending = searchSort.order == "asc";
                searchDto.filterFixedWhereClause = filterFixedWhereClause;
                searchDto.needsCountUpdate = true;
                searchDto.quickSearchDTO = quicksearchDTO;
                //existing template pass too many variable, which some of them did not get translated and caused an SQL error
                //searchDto.searchTemplate = searchTemplate;
                searchDto.searchTemplate = searchTemplate;
                searchData.lastSearchedValues = searchDto.searchValues;

                if (paginationData) {
                    searchDto.pageNumber = paginationData.pageNumber;
                    searchDto.pageSize = paginationData.pageSize;
                }
                return searchDto;

            },

            buildReportSearchDTO: function (searchDto, searchData, searchSort, searchOperator, filterFixedWhereClause) {
                if (searchDto == null) {
                    searchDto = {};
                    searchDto.searchParams = buildSearchParamsString(searchData, searchOperator);
                    searchDto.searchValues = this.buildSearchValuesString(searchData, searchOperator);
                }
                else {
                    var extraParams = buildSearchParamsString(searchData, searchOperator);
                    var extraValues = this.buildSearchValuesString(searchData, searchOperator);

                    if (extraParams != null && extraParams != '' && extraValues != null && extraValues != '') {
                        searchDto.searchParams += "&&" + extraParams;
                        searchDto.searchValues += ",,," + extraValues;
                    }
                }
                searchDto.searchSort = buildSearchSortString(searchSort);
                searchDto.SearchAscending = searchSort.order == "asc";
                searchDto.filterFixedWhereClause = filterFixedWhereClause;
                searchDto.needsCountUpdate = searchDto.searchValues != searchData.lastSearchedValues;
                searchData.lastSearchedValues = searchDto.searchValues;
                return searchDto;

            },
            /**
             * Converts server result into searchData and searchOperations to be reapplied to the filters, so that the screen become consistent after an api call.
             * 
             * @param {} searchParams a string whose parameters are separated by either && or ||,
             * @param {} searchValues another string separated by ,,, containing the values that were used on the search
             * @returns {} an object containing both the searchData and the searchOperator that will be updated on the filters
             */
            buildSearchDataAndOperations: function (searchParams, searchValues) {
                var result = {};

                var searchData = {};
                var searchOperator = {};

                var params = searchParams.split("&&");
                var values = searchValues.split(",,,");
                if (values.length !== params.length) {
                    //this was a global search, so it uses || and not && 
                    params = searchParams.split("||,");
                }

                for (var i = 0; i < params.length; i++) {
                    var value = values[i];
                    var param = params[i];
                    searchOperator[param] = this.getSearchOperator(value);
                    searchData[param] = getSearchValue(value);
                    if (value.indexOf("__") !== -1) {
                        var splittedValues = value.split("__");
                        searchData[param] = splittedValues[0];
                        searchData[param+"_end"] = splittedValues[1];
                    }
                }
                result.searchData = searchData;
                result.searchOperator = searchOperator;
                return result;
            },

            getSearchOperation: function (idx) {
                return this.searchOperations()[idx];
            },

            getSearchOperationById: function (id) {
                var op = $.grep(this.searchOperations(), function (e) {
                    return e.id.toUpperCase() === id.toUpperCase();
                });
                if (op.length > 0) {
                    return op[0];
                }
                return null;
            },

            getSearchOperationBySymbol: function (symbol) {
                var arr = this.searchOperations();
                for (var i = 0; i < arr.length; i++) {
                    if (arr[i].symbol.equalIc(symbol)) {
                        return arr[i];
                    }
                }
                return null;
            },

            searchOperations: function () {
                var language = i18NService.getCurrentLanguage();
                var module = contextService.retrieveFromContext('currentmodule');
                if (!nullOrUndef(module)) {
                    //if inside a module language should be always english
                    language = 'EN';
                }
                if (objCache[language] != undefined) {
                    return objCache[language];
                }
                objCache[language] = buildArray();
                return objCache[language];
            },

            defaultSearchOperation: function () {
                return this.searchOperations()[1];
            },

            /**
             * @param {} searchData dictionary for modifying the grid query that is present on the screen e.g. { 'param1' : 'value1', 'param2' : 'value2' }
             * @param {} extraparameters accepts:
             *              pageNumber: the page to go
             *              pageSize: a different page size than the scope one
             *              printMode: if we need to refresh the grid for printmode
             *              avoidspin: if true, we wont show the busy indicator on the screen
             *              keepfilterparams: if true, we should keep the filter parameters on the grid
             *              searchTemplate: the search template string to apply on the seach
             *              panelid: the panel id to refresh, used to allow multiple data on screen
             *              fieldstodisplay: if present, the schema will be sliced for showing only these fields
             *              forcecleanup: used to mark that the any stateful scope data needs to be clean, including filterwhereclauses
             *              quickSearchDTO: 
             */
            refreshGrid: function (searchData, searchOperator, extraparameters) {
                extraparameters = extraparameters || {};

                var key = "poll_refreshgridaction" + (extraparameters.panelid ? extraparameters.panelid : "");

                //this is needed because the crud_list handler may not yet be in place when this method is called, 
                //we need to make sure that as soon as it gets available it consumes the message
                contextService.insertIntoContext(key, {
                    searchData: searchData,
                    searchOperator: searchOperator,
                    extraparameters: extraparameters,
                    panelid: extraparameters.panelid,
                    metadataid: extraparameters.metadataid,
                    forcecleanup: extraparameters.forcecleanup
                }, true);
                $rootScope.$broadcast("sw_refreshgrid", searchData, searchOperator, extraparameters);
            },

            /// <summary>
            /// invokes a search function on the specified application, returning a $http.get promise invocation built
            /// </summary>
            /// <param name="application"></param>
            /// <param name="searchData"></param>
            /// <param name="searchOperators"> </param>
            /// <param name="schema"></param>
            /// <param name="extraParameters">Accepts an object with the following:
            /// 
            /// pageNumber --> the page in which to perform the search, would be 1 by default
            /// pageSize --> the number of items to display per page, would be 30 by default
            /// mode --> the mode of the schema to use, defaults to none
            /// searchOperators --> the array of operators to apply to searchdata array, in the same order
            /// searchSort --> the sorting object
            /// searchTemplate --> the searchtemplate to use in the search operation
            /// printMode --> if true, means we´re doing a search for a print
            /// searchDTO --> the built in searchDTO, íf present, won´t be built inside here
            /// 
            /// </param>
            searchWithData: function (application, searchData, schema, extraParameters) {
                if (application == null) {
                    throw new Error("application cannot be null");
                }
                extraParameters = extraParameters || {};
                searchData = searchData || {};

                var log = $log.getInstance('searchService#searchWithData');
                var searchDTO = extraParameters.searchDTO;
                if (!searchDTO) {
                    searchDTO = this.buildSearchDTO(searchData, extraParameters.searchSort, extraParameters.searchOperators, null);
                    searchDTO.searchTemplate = extraParameters.searchTemplate;
                    searchDTO.pageNumber = extraParameters.pageNumber ? extraParameters.pageNumber : 1;
                    searchDTO.totalCount = 0;
                    searchDTO.pageSize = extraParameters.pageSize ? extraParameters.pageSize : 30;
                }

                var restParameters = {
                    key: {
                        schemaId: schema ? schema : "list",
                        mode: extraParameters.mode ? extraParameters.mode : 'none',
                        platform: "web"
                    },
                    schemaFieldsToDisplay: extraParameters.schemaFieldsToDisplay,
                    SearchDTO: searchDTO
                };
                var queryString = $.param(restParameters);
                var urlToUse = url("/api/Data/{0}?{1}".format(application, queryString));
                log.info("invoking url {0}".format(urlToUse));

                if (extraParameters.saveSwGlobalRedirectURL) {
                    contextService.insertIntoContext("swGlobalRedirectURL", urlToUse, false);
                }

                if (extraParameters.addToHistory) {
                    historyService.addToHistory(urlToUse);
                }
                if (extraParameters.metadataid != null) {
                    return $http.get(urlToUse, {
                        headers: {
                            currentmetadata: extraParameters.metadataid
                        }
                    });
                } else {
                    return $http.get(urlToUse);
                }
            }
        };

    });

})(angular);