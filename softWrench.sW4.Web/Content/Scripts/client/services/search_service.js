var app = angular.module('sw_layout');

app.factory('searchService', function (i18NService, $rootScope, contextService) {

    var objCache = {};

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

        var searchOperations = [
        { id: "", symbol: "", title: filtertype.nofilter, tooltip: filtertype.filter, begin: "", end: "", renderType: ["default", "datetime"] },
        { id: "CONTAINS", symbol: "C", title: filtertype.contains, tooltip: filtertype.contains, begin: "%", end: "%", renderType: ["default"] },
        { id: "NCONTAINS", symbol: "!C", title: filtertype.ncontains, tooltip: filtertype.ncontains, begin: "!%", end: "%", renderType: ["default"] },
        { id: "STARTWITH", symbol: "ST", title: filtertype.startwith, tooltip: filtertype.startwith, begin: "", end: "%", renderType: ["default"] },
        { id: "ENDWITH", symbol: "END", title: filtertype.endwith, tooltip: filtertype.endwith, begin: "%", end: "", renderType: ["default"] },
        { id: "EQ", symbol: "=", title: filtertype.eq, tooltip: filtertype.eq, begin: "=", end: "", renderType: ["default", "datetime"] },
        { id: "BTW", symbol: "-", title: filtertype.btw, tooltip: filtertype.btw, begin: ">=", end: "<=", renderType: [] },
        { id: "NOTEQ", symbol: "!=", title: filtertype.noteq, tooltip: filtertype.noteq, begin: "!=", end: "", renderType: ["default", "datetime"] },
        { id: "BLANK", symbol: "BLANK", title: filtertype.blank, tooltip: filtertype.blank, begin: "", end: "", renderType: ["default", "datetime"] },
        { id: "GT", symbol: ">", title: filtertype.gt, tooltip: filtertype.gt, begin: ">", end: "", renderType: ["default", "datetime"] },
        { id: "LT", symbol: "<", title: filtertype.lt, lt: filtertype.lt, begin: "<", end: "", renderType: ["default", "datetime"] },
        { id: "GTE", symbol: ">=", title: filtertype.gte, tooltip: filtertype.gte, begin: ">=", end: "", renderType: ["default", "datetime"] },
        { id: "LTE", symbol: "<=", title: filtertype.lte, tooltip: filtertype.lte, begin: "<=", end: "", renderType: ["default", "datetime"] }
        ];
        return searchOperations;
    };



    var buildSearchParamsString = function (searchData, searchOperator) {
        var resultString = "";
        for (var data in searchData) {
            if (data == "lastSearchedValues") {
                //exclude this field which is used only to control the  needsCountUpdate flag
                continue;
            }

            if ((searchData[data] != null && searchData[data] != '') ||
                (searchOperator[data] != null && searchOperator[data].id == "BLANK")) {

                if (data.indexOf('___') != -1) {
                    // this case is only for "BETWEEN" operator
                    data = data.substring(0, data.indexOf('___'));
                    if (resultString.indexOf(data) != -1) {
                        resultString += data + "&&";
                    } else {
                        resultString += data + "___";
                    }
                    continue;
                }

                resultString += data + "&&";
            }
        }
        return resultString.substring(0, resultString.lastIndexOf("&&"));
    };

    var buildSearchSortString = function (searchSort) {
        //            var searchSort = scope.searchSort;
        var resultString = "";

        if (searchSort.field != null && searchSort.field != '') {
            resultString = searchSort.field;
        }
        return resultString;
    };



    return {


        buildSearchValuesString: function (searchData, searchOperator) {
            var resultString = "";
            var value = "";
            var beginAlreadySet = false;
            for (var data in searchData) {
                if ((searchData[data] == null || searchData[data] == '' || data == "lastSearchedValues") &&
                    (searchOperator[data] == null || searchOperator[data].id != "BLANK")) {
                    continue;
                }

                value = searchData[data];
                if (data.indexOf('___') != -1) {
                    data = data.substring(0, data.indexOf('___'));
                }
                if (searchOperator[data] == null) {
                    searchOperator[data] = this.defaultSearchOperation();
                }
                if (searchOperator[data].begin != '' && !beginAlreadySet) {
                    value = searchOperator[data].begin + value;
                    if (searchOperator[data].id == 'BTW') {
                        beginAlreadySet = true;
                        resultString += value + "___";
                        continue;
                    }
                }
                if (searchOperator[data].end != '') {
                    if (searchOperator[data].id == 'BTW') {
                        value = searchOperator[data].end + value;
                        beginAlreadySet = false;
                    }
                    else {
                        value = value + searchOperator[data].end;
                    }
                }
                if (searchOperator[data] != null && searchOperator[data].id == 'BLANK') {
                    value = 'IS NULL';
                }

                resultString += value + ",,,";
            }
            resultString = resultString.substring(0, resultString.lastIndexOf(",,,"));

            return resultString;
        },

        buildSearchDTO: function (searchData, searchSort, searchOperator, filterFixedWhereClause, unionFilterFixedWhereClause) {



            var btwFlag = false;
            var searchDto = {};
            searchData = searchData || {};
            searchSort = searchSort || {};
            searchOperator = searchOperator || {};


            searchDto.searchParams = buildSearchParamsString(searchData, searchOperator);
            searchDto.searchValues = this.buildSearchValuesString(searchData, searchOperator);
            searchDto.searchSort = buildSearchSortString(searchSort);
            searchDto.SearchAscending = searchSort.order == "asc";
            searchDto.filterFixedWhereClause = filterFixedWhereClause;
            searchDto.unionFilterFixedWhereClause = unionFilterFixedWhereClause;
            searchDto.needsCountUpdate = true;
            searchData.lastSearchedValues = searchDto.searchValues;
            return searchDto;

        },

        buildReportSearchDTO: function (searchDto, searchData, searchSort, searchOperator, filterFixedWhereClause, unionFilterFixedWhereClause) {
            if (searchDto == null || searchDto == undefined) {
                var searchDto = {};
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
            searchDto.unionFilterFixedWhereClause = unionFilterFixedWhereClause;
            searchDto.needsCountUpdate = searchDto.searchValues != searchData.lastSearchedValues;
            searchData.lastSearchedValues = searchDto.searchValues;
            return searchDto;

        },

        getSearchOperation: function (idx) {
            return this.searchOperations()[idx];
        },

        getSearchOperationById: function (id) {
            var op = $.grep(this.searchOperations(), function (e) { return e.id.toUpperCase() == id.toUpperCase() });
            if (op.length > 0) {
                return op[0];
            }
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

        noFilter: function () {
            return this.searchOperations()[0];
        }

    };


});