(function (angular) {
    "use strict";

    //var PRINTMODAL_$_KEY = '[data-class="printModal"]';

    angular.module('sw_layout')
        .service('printService', [
            "$rootScope", "$http", "$timeout", "$log", "$q", "tabsService", "fieldService", "fixHeaderService", "redirectService", "searchService", "alertService", "printAwaitableService", "schemaCacheService", "crudContextHolderService", "compositionService",
            function ($rootScope, $http, $timeout, $log, $q, tabsService, fieldService, fixHeaderService, redirectService, searchService, alertService, printAwaitableService, schemaCacheService, crudContextHolderService, compositionService) {

                var mergeCompositionData = function (datamap, nonExpansibleData, expansibleData) {
                    var resultObj = {};
                    if (expansibleData != null) {
                        resultObj = expansibleData;
                    }
                    if (nonExpansibleData == undefined) {
                        return resultObj;
                    }
                    for (let i = 0; i < nonExpansibleData.length; i++) {
                        const value = nonExpansibleData[i];
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

                    if (!sessionStorage.mockprint) {
                        window.print();
                    }
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
                        var log = $log.getInstance("print_service#printDetail", ["print"]);
                        if (schema.properties["detail.print.schemaid"]) {
                            printOptions = printOptions || {};
                            printOptions.printSchemaId = schema.properties["detail.print.schemaid"];
                            printOptions.shouldPrintMain = true;
                        }

                        if (schema.hasNonInlineComposition && printOptions === undefined) {
                            //this case, we have to choose which extra compositions to choose, so we will open the print modal
                            //open print modal...
                            $rootScope.$broadcast(JavascriptEventConstants.PrintShowModal, schema, false);
                            return;
                        }


                        printOptions = printOptions || {};
                        return this.locatePrintSchema(schema, "detail", printOptions.printSchemaId)
                            .then(printSchema => {

                                const isSameSchema = printSchema.schemaId === schema.schemaId;
                                var params = {
                                    application: printSchema.applicationName,
                                    key: {
                                        schemaId: printSchema.schemaId,
                                        mode: printSchema.mode,
                                        platform: platform()
                                    },
                                    options: {
                                        printMode: true
                                    }
                                };


                                var notExpansibleCompositions = [];
                                var expansibleCompositions = {};
                                if (printOptions != null) {
                                    params.options.compositionsToExpand = tabsService.buildCompositionsToExpand(
                                        printOptions.compositionsToExpand,
                                        printSchema,
                                        datamap,
                                        'print',
                                        notExpansibleCompositions);
                                }

                                var shouldPageBreak = printOptions == undefined ? true : printOptions.shouldPageBreak;
                                var shouldPrintMain = printOptions == undefined ? true : printOptions.shouldPrintMain;
                                var printCallback = printOptions == undefined ? null : printOptions.printCallback;

                                var emptyCompositions = {};
                                //TODO: check whether printOptions might or not be null
                                if (printOptions != null && printOptions.compositionsToExpand) {
                                    $.each(printOptions.compositionsToExpand,
                                        function (key, obj) {
                                            if (obj.value == true) {
                                                const compositionData = datamap[key];
                                                //TODO: CompositionData might be undefined or not
                                                if (compositionData == undefined || compositionData.length == 0) {
                                                    emptyCompositions[key] = [];
                                                }
                                            }
                                        });
                                }
                                if (isSameSchema && (params.options.compositionsToExpand == undefined || params.options.compositionsToExpand === "")) {
                                    //no need to hit the server, just print the main detail
                                    log.debug('sw_readytoprintevent dispatched');
                                    return $rootScope.$broadcast(JavascriptEventConstants.ReadyToPrint,
                                        mergeCompositionData(datamap, notExpansibleCompositions, emptyCompositions),
                                        shouldPageBreak,
                                        shouldPrintMain,
                                        printCallback,
                                        printSchema,

                                    );
                                }

                                if (!isSameSchema) {
                                    //bringing datamap from another schema

                                    const parameters = {
                                        "id": datamap[schema.idFieldName],
                                        printmode: true
                                    }

                                    const getDetailsUrl = redirectService.getApplicationUrl(schema.applicationName, printSchema.schemaId, '', '', parameters);
                                    return $http.get(getDetailsUrl).then(function (response) {
                                        const data = response.data;
                                        const datamap = data.resultObject;
                                        fieldService.fillDefaultValues(printSchema.displayables, datamap);

                                        return $rootScope.$broadcast(JavascriptEventConstants.ReadyToPrint,
                                            mergeCompositionData(datamap, notExpansibleCompositions, emptyCompositions),
                                            shouldPageBreak,
                                            shouldPrintMain,
                                            printCallback,
                                            printSchema,
                                            datamap
                                        );
                                    });


                                } else {
                                    log.info('calling expanding compositions on service; params: {0}'.format(params));
                                    const urlToInvoke = removeEncoding(url("/api/generic/Composition/ExpandCompositions?" + $.param(params)));
                                    return $http.get(urlToInvoke).then(function (response) {
                                        const result = response.data;
                                        var compositions = result.resultObject;
                                        $.each(emptyCompositions,
                                            function (key, obj) {
                                                compositions[key] = obj;
                                            });

                                        log.debug('sw_readytoprintevent dispatched after server return');
                                        const compositionsToPrint = mergeCompositionData(datamap, notExpansibleCompositions, compositions);
                                        $rootScope.$broadcast(JavascriptEventConstants.ReadyToPrint,
                                            compositionsToPrint,
                                            shouldPageBreak,
                                            shouldPrintMain,
                                            printCallback,
                                            printSchema);
                                    });
                                }


                            });
                    },

                    locatePrintSchema: function (schema, printMode, printSchemaId) {

                        if (printSchemaId) {
                            return schemaCacheService.fetchSchema(schema.applicationName, printSchemaId);
                        }

                        //list, detailedList, detail
                        var schemaId = schema.schemaId;

                        if (printMode === 'detailedList') {
                            schemaId = schema.printDetailedListSchemaId;
                        } else if (printMode === 'list' && schema.printListSchemaId) {
                            schemaId = schema.printListSchemaId;
                        } else if (printMode === 'detail' && schema.printDetailSchemaId) {
                            schemaId = schema.printDetailSchemaId;
                        }

                        return schemaCacheService.fetchSchema(schema.applicationName, schemaId);
                    },

                    buildDetailedSearchParameter: function (printSchema) {
                        var ids = [];

                        const selectionModel = crudContextHolderService.getSelectionModel();
                        const selectionBuffer = selectionModel.selectionBuffer;

                        const countSelected = Object.keys(selectionBuffer).length;
                        if (countSelected > 30) {
                            alertService.alert("Maximum allowed to detailed print is 30. You requested " + countSelected);
                            return null;
                        }


                        angular.forEach(selectionBuffer, (entry, id) => {
                            if (!selectionBuffer.hasOwnProperty(id)) {
                                return;
                            }
                            ids.push(id);
                        });

                        const parameters = {
                            printmode: true
                        };

                        const searchData = {
                            [printSchema.idFieldName]: ids.join(',')
                        };
                        const searchOperator = {
                            [printSchema.idFieldName]: searchService.getSearchOperationById('EQ')
                        };


                        parameters.searchDTO = searchService.buildSearchDTO(searchData, {}, searchOperator, {});
                        parameters.searchDTO.compositionsToFetch = compositionService.getInlineCompositions(printSchema.cachedCompositions);



                        return parameters;
                    },

                    printDetailedList: function (schema, datamap, printOptions) {

                        return this.locatePrintSchema(schema, 'detailedList').then(printSchema => {
                            if (printSchema.hasNonInlineComposition && printOptions === undefined) {
                                //this case, we have to choose which extra compositions to choose, so we will open the print modal
                                //open print modal...
                                $rootScope.$broadcast(JavascriptEventConstants.PrintShowModal, printSchema);
                                return;
                            }

                            var parameters = this.buildDetailedSearchParameter(printSchema);
                            if (parameters === null) {
                                return;
                            }

                            var getDetailsUrl =
                                redirectService.getApplicationUrl(schema.applicationName, printSchema.schemaId, '', '', parameters);

                            var shouldPageBreak = true;
                            var shouldPrintMain = printOptions == undefined ? true : printOptions.shouldPrintMain;

                            $http.get(getDetailsUrl).then(function (response) {
                                const data = response.data;
                                $rootScope.$broadcast(JavascriptEventConstants.PrintReadyForDetailedList,
                                    data.resultObject,
                                    {},
                                    shouldPageBreak,
                                    shouldPrintMain,
                                    printSchema);
                            });

                            // to remove the crud_body grid from printing page
                            $('#listgrid').addClass('hiddenonprint');
                        });
                    },


                    hidePrintModal: function () {
                        $rootScope.$broadcast(JavascriptEventConstants.PrintHideModal);
                    },

                    awaitToPrint: function () {
                        var awaitables = printAwaitableService.getAwaitables();
                        //only not already resolved promises
                        awaitables = awaitables.filter(a => a.$$state.status === 0);
                        return $q.all(awaitables).finally(() => {
                            printAwaitableService.dispose();
                            awaitables.length = 0;
                        });
                    }
                };

            }]);

})(angular);