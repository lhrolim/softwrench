var app = angular.module('sw_layout');

//var PRINTMODAL_$_KEY = '[data-class="printModal"]';

app.factory('printService', function ($rootScope, $http, $timeout,$log, tabsService, fixHeaderService, redirectService, searchService) {

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
                resultObj[value.key] = datamap.fields[value.key];
            }
            
        }
        return resultObj;
   };

    var doPrintGrid = function(schema) {
        fixHeaderService.unfix();
        innerDoPrint();
        fixHeaderService.fixThead(schema,{empty:false});
    };

    var innerDoPrint = function() {
        window.print();
    };

    return {
        doPrint: function() {
            innerDoPrint();
        },

        printList: function (totalCount, printPageSize, gridRefreshFunction, schema) {
            var log = $log.getInstance('printService#printList');
            $rootScope.printRequested = false;
            if ($rootScope.$$listeners['listTableRenderedEvent'] != undefined) {
                //avoids multiple listener registration
                $rootScope.$$listeners['listTableRenderedEvent'] = null;
            }

            // to add the crud_body grid in printing page
            var listgrid = $('#listgrid');
            listgrid.removeClass('hiddenonprint');

            $rootScope.$on('listTableRenderedEvent', function () {
                if (!$rootScope.printRequested) {
                    return;
                }
                $rootScope.printRequested = false;
                var rows = $('[rel=hideRow]');
                var index;
                if (rows.length <= printPageSize) {
                    return;
                }
                for (index = 1; index < rows.length; ++index) {
                    if (index > printPageSize) {
                        rows[index].className += ' hideRows';
                    }
                }                
                if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
                    listgrid.addClass('listgrid-firefox');
                }
                doPrintGrid(schema);
            });

            if (totalCount <= printPageSize) {
                //If all the data is on the screen, just print it
                doPrintGrid(schema);
                return;
            }

            //otherwise, hit the server asking for the full grid to be print
            var invokeObj = {
                pageNumber: 1,
                pageSize: totalCount,
                printMode: true,
            };

            log.info('fetching print data on server');
            gridRefreshFunction(invokeObj);
            
        },

        printDetail: function (schema, datamap, printOptions) {
            var log = $log.getInstance("print_service#printDetail");
            if (schema.hasNonInlineComposition && printOptions === undefined) {
                //this case, we have to choose which extra compositions to choose, so we will open the print modal
                //open print modal...
                $rootScope.$broadcast("sw_showprintmodal", schema);
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
                params.options.compositionsToExpand = tabsService.buildCompositionsToExpand(printOptions.compositionsToExpand, schema, datamap, 'print', notExpansibleCompositions,true);
            }
            params.options.printMode = true;
            var shouldPageBreak = printOptions == undefined ? true : printOptions.shouldPageBreak;
            var shouldPrintMain = printOptions == undefined ? true : printOptions.shouldPrintMain;

                var emptyCompositions = {};
            //TODO: check whether printOptions might or not be null
            if (printOptions != null) {
                $.each(printOptions.compositionsToExpand, function (key, obj) {
                    if (obj.value == true) {
                        var compositionData = datamap.fields[key];
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
                $rootScope.$broadcast("sw_readytoprintevent", mergeCompositionData(datamap, notExpansibleCompositions, emptyCompositions), shouldPageBreak, shouldPrintMain);
                return;
            }

            log.info('calling expanding compositions on service; params: {0}'.format(params));
            var urlToInvoke = removeEncoding(url("/api/generic/ExtendedData/ExpandCompositions?" + $.param(params)));
            $http.get(urlToInvoke, { printMode: true }).success(function (result) {

                var compositions = result.resultObject;
                $.each(emptyCompositions, function (key, obj) {
                    compositions[key] = obj;
                });
                
                log.debug('sw_readytoprintevent dispatched after server return');
                var compositionsToPrint = mergeCompositionData(datamap, notExpansibleCompositions, compositions);
                $rootScope.$broadcast("sw_readytoprintevent", compositionsToPrint, shouldPageBreak, shouldPrintMain);
            });
        },
        
        printDetailedList: function (schema, datamap, printOptions) {

            var printSchema = schema.printSchema != null ? schema.printSchema : schema;

            if (printSchema.hasNonInlineComposition && printOptions === undefined) {
                //this case, we have to choose which extra compositions to choose, so we will open the print modal
                //open print modal...
                $rootScope.$broadcast("sw_showprintmodal", printSchema);
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

            searchData[printSchema.idFieldName] =  ids.join(',');
            searchOperator[printSchema.idFieldName] = searchService.getSearchOperationById('EQ');

            parameters.searchDTO = searchService.buildSearchDTO(searchData, {}, searchOperator, {});
            parameters.searchDTO.pageSize = ids.length;

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

            $http.get(getDetailsUrl).success(function (data) {
                $rootScope.$broadcast("sw_readytoprintdetailedlistevent", data.resultObject, compositionsToFetch, shouldPageBreak, shouldPrintMain);
            });

            // to remove the crud_body grid from printing page
            $('#listgrid').addClass('hiddenonprint');
        },


        hidePrintModal: function () {
            $rootScope.$broadcast("sw_hideprintmodal");
        }
    };

});


