(function (angular) {
    "use strict";

//var PRINTMODAL_$_KEY = '[data-class="printModal"]';

angular.module('sw_layout')
    .service('printService', [
        "$rootScope", "$http", "$timeout", "$log", "$q", "tabsService", "fixHeaderService", "redirectService", "searchService", "alertService", "printAwaitableService",
        function ($rootScope, $http, $timeout, $log, $q, tabsService, fixHeaderService, redirectService, searchService, alertService, printAwaitableService) {

     var mergeCompositionData = function (datamap, nonExpansibleData, expansibleData) {
        var resultObj = {};
        if (expansibleData != null) {
            resultObj = expansibleData;
        }
        if (nonExpansibleData == undefined) {
            return resultObj;
        }
        for (var i = 0; i < nonExpansibleData.length; i++) {
            var value = nonExpansibleData[i];
            if (value.tabObject != undefined) {
                //this would happen in case of a tab being printed wherease we don´t have the fields on datamap
                resultObj[value.key] = value.tabObject;
            } else {
                resultObj[value.key] = datamap[value.key];
            }

        }
        return resultObj;
    };

    var doPrintGrid = function (schema) {
        fixHeaderService.unfix();
        innerDoPrint();
        fixHeaderService.fixThead(schema);
    };

    const searchPrintGrid = function (pageNumber, pageSize, numberOfPages) {
        searchService.refreshGrid(null, null, {
            pageNumber: pageNumber,
            pageSize: pageSize,
            numberOfPages: numberOfPages,
            printMode: true
        });
    }

    var innerDoPrint = function () {
        window.print();
    };

    return {
        doPrint: function (isList, schema) {
            if (isList) {
                doPrintGrid(schema);
            } else {
                innerDoPrint();
            }
            $rootScope.printRequested = null; 
        },

        printList: function (paginationData, schema, printOptions) {
            if (!printOptions) {
                $rootScope.$broadcast(JavascriptEventConstants.PrintShowModal, schema, true, paginationData);
                return;
            }

            $.each($(".tooltip"), function () {
                $(this).remove();
            });

            if (printOptions.pageOption === "current") {
                searchPrintGrid(1, paginationData.pageSize);
                return;
            }

            if (printOptions.pageOption === "all") {
                searchPrintGrid(1, paginationData.totalCount);
                return;
            }

            
            if (printOptions.startPage > printOptions.endPage || 
                printOptions.startPage < 1 || printOptions.endPage < 1 || 
                printOptions.startPage > paginationData.pageCount || printOptions.endPage > paginationData.pageCount) {
                alertService.alert("Invalid page range.");
                return;
            }

            searchPrintGrid(printOptions.startPage, paginationData.pageSize, printOptions.endPage - printOptions.startPage + 1);
        },

        readyToPrintList: function (datamap) {
            $rootScope.$broadcast(JavascriptEventConstants.PrintReadyForList, datamap);
        },

        printDetail: function (schema, datamap, printOptions) {
            var log = $log.getInstance("print_service#printDetail");
            if (schema.hasNonInlineComposition && printOptions === undefined) {
                //this case, we have to choose which extra compositions to choose, so we will open the print modal
                //open print modal...
                $rootScope.$broadcast(JavascriptEventConstants.PrintShowModal, schema, false);
                return;
            }
            var params = {};
            params.key = {};

            params.options = {};

            params.application = schema.applicationName;
            params.key.schemaId = schema.schemaId;
            params.key.mode = schema.mode;
            params.key.platform = platform();
            var notExpansibleCompositions = [];
            var expansibleCompositions = {};
            if (printOptions != null) {
                params.options.compositionsToExpand = tabsService.buildCompositionsToExpand(printOptions.compositionsToExpand, schema, datamap, 'print', notExpansibleCompositions);
            }
            params.options.printMode = true;
            var shouldPageBreak = printOptions == undefined ? true : printOptions.shouldPageBreak;
            var shouldPrintMain = printOptions == undefined ? true : printOptions.shouldPrintMain;
            var printCallback = printOptions == undefined ? null : printOptions.printCallback;

            var emptyCompositions = {};
            //TODO: check whether printOptions might or not be null
            if (printOptions != null && printOptions.compositionsToExpand) {
                $.each(printOptions.compositionsToExpand, function (key, obj) {
                    if (obj.value == true) {
                        var compositionData = datamap[key];
                        //TODO: CompositionData might be undefined or not
                        if (compositionData == undefined || compositionData.length == 0) {
                            emptyCompositions[key] = [];
                        }
                    }
                });
            }
            if (params.options.compositionsToExpand == undefined || params.options.compositionsToExpand == "") {
                //no need to hit the server, just print the main detail
                log.debug('sw_readytoprintevent dispatched');
                $rootScope.$broadcast(JavascriptEventConstants.ReadyToPrint, mergeCompositionData(datamap, notExpansibleCompositions, emptyCompositions), shouldPageBreak, shouldPrintMain, printCallback);
                return;
            }

            log.info('calling expanding compositions on service; params: {0}'.format(params));
            var urlToInvoke = removeEncoding(url("/api/generic/Composition/ExpandCompositions?" + $.param(params)));
            $http.get(urlToInvoke).then(function (response) {
                const result = response.data;
                var compositions = result.resultObject;
                $.each(emptyCompositions, function (key, obj) {
                    compositions[key] = obj;
                });

                log.debug('sw_readytoprintevent dispatched after server return');
                var compositionsToPrint = mergeCompositionData(datamap, notExpansibleCompositions, compositions);
                $rootScope.$broadcast(JavascriptEventConstants.ReadyToPrint, compositionsToPrint, shouldPageBreak, shouldPrintMain, printCallback);
            });
        },

        printDetailedList: function (schema, datamap, printOptions) {

            var printSchema = schema.printSchema != null ? schema.printSchema : schema;

            if (printSchema.hasNonInlineComposition && printOptions === undefined) {
                //this case, we have to choose which extra compositions to choose, so we will open the print modal
                //open print modal...
                $rootScope.$broadcast(JavascriptEventConstants.PrintShowModal, printSchema);
                return;
            }


            var ids = [];

            $.grep(datamap, function (element) {
                if (element.fields['checked'] == true) {
                    ids.push(element.fields[printSchema.idFieldName]);
                }
            });

            var applicationName = printSchema.name;
            var printSchemaId = printSchema.schemaId;
            var parameters = {};
            var searchData = {};
            var searchOperator = {};

            searchData[printSchema.idFieldName] = ids.join(',');
            searchOperator[printSchema.idFieldName] = searchService.getSearchOperationById('EQ');

            parameters.searchDTO = searchService.buildSearchDTO(searchData, {}, searchOperator, {});


            parameters.searchDTO.compositionsToFetch = [];
            var compositionsToFetch = {};
            $.each(printOptions.compositionsToExpand, function (key, obj) {
                if (obj.value == true) {
                    parameters.searchDTO.compositionsToFetch.push(key);
                    compositionsToFetch[key] = obj;
                }
            });

            parameters.printmode = true;

            var getDetailsUrl = redirectService.getApplicationUrl(applicationName, printSchemaId, '', '', parameters);

            var shouldPageBreak = printOptions == undefined ? true : printOptions.shouldPageBreak;
            var shouldPrintMain = printOptions == undefined ? true : printOptions.shouldPrintMain;

            $http.get(getDetailsUrl).then(function (response) {
                const data = response.data;
                $rootScope.$broadcast(JavascriptEventConstants.PrintReadyForDetailedList, data.resultObject, compositionsToFetch, shouldPageBreak, shouldPrintMain);
            });

            // to remove the crud_body grid from printing page
            $('#listgrid').addClass('hiddenonprint');
        },


        hidePrintModal: function () {
            $rootScope.$broadcast(JavascriptEventConstants.PrintHideModal);
        },

        awaitToPrint: function () {
            var awaitables = printAwaitableService.getAwaitables();
            return $q.all(awaitables).then(() => {
                awaitables.length = 0;
            });
        }
    };

}]);

})(angular);