﻿var app = angular.module('sw_layout');

app.directive('advancedFilterToogle', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/advanced_filter_toogle.html')
    };
});

app.directive('crudListWrapper', function (contextService, $compile) {
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            schema: '=',
            datamap: '=',
            previousschema: '=',
            previousdata: '=',
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            ismodal: '@',
            checked: '=',
            isList: "=",
            timestamp: '=',
        },
        link: function (scope, element, attrs) {
            if (scope.isList) {
                element.append(
                    "<crud-list datamap='datamap' schema='schema' pagination-data='paginationData' " +
                    "search-data='searchData' " +
                    "search-operator='searchOperator' " +
                    "search-sort='searchSort'" +
                     "timestamp='{{timestamp}}' />"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});



app.directive('crudList', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_list.html'),
        scope: {
            schema: '=',
            datamap: '=',
            fieldstodisplay: '=',
            previousschema: '=',
            previousdata: '=',
            ismodal: '@',
            hidebars: '@',
            checked: '=',
            timestamp: '@',
            panelid: "@"
        },

        controller: function ($scope, $http, $rootScope, $filter, $injector, $log, $timeout,
            formatService, fixHeaderService,
            searchService, tabsService,
            fieldService, commandService, i18NService,
            validationService, submitService, redirectService,
            associationService, statuscolorService, contextService, eventService, iconService, expressionService, checkpointService) {



            $scope.$name = 'crudlist';

            fixHeaderService.activateResizeHandler();

            $scope.loadIcon = function (value, metadata) {
                return iconService.loadIcon(value, metadata);
            };

            $scope.hasTabs = function (schema) {
                return tabsService.hasTabs(schema);
            };
            $scope.isCommand = function (schema) {
                return $scope.schema && $scope.schema.properties['command.select'] == "true";
            };
            $scope.isNotHapagTest = function () {
                return $rootScope.clientName != 'hapag';
            };

            $scope.tabsDisplayables = function (schema) {
                return tabsService.tabsDisplayables(schema);
            };

            $scope.getGridColumnStyle = function (column, propertyName) {
                var property = column.rendererParameters[propertyName];

                if (property != null) {
                    return property;
                }

                if (propertyName == 'maxwidth') {
                    var high = $(window).width() > 1199;
                    if (high) {
                        return '135px';
                    }
                    return '100px';
                }
                return null;
            }

            $scope.shouldShowSort = function (column, orientation) {
                return column.attribute != null && ($scope.searchSort.field == column.attribute || $scope.searchSort.field == column.rendererParameters['sortattribute']) && $scope.searchSort.order == orientation;
            };


            $scope.$on('filterRowRenderedEvent', function (filterRowRenderedEvent) {
                if ($scope.datamap && $scope.datamap.length == 0) {
                    // only update filter visibility if there are no results to shown on grid... else the filter visibility will be updated on "listTableRenderedEvent"
                    fixHeaderService.updateFilterZeroOrOneEntries();
                    // update table heigth (for ie9)
                }
            });

            $scope.$on('listTableRenderedEvent', function (listTableRenderedEvent) {
                var log = $log.getInstance('sw4.crud_list_dir#on#listTableRenderedEvent');
                log.debug('init table rendered listener');

                var parameters = {
                    fullKey: $scope.schema.properties['config.fullKey'],
                    searchData: $scope.searchData
                };
                eventService.onload($scope, $scope.schema, $scope.datamap, parameters);

                if ($scope.ismodal == 'true' && !(true === $scope.$parent.showingModal)) {
                    return;
                }

                var params = {};
                fixHeaderService.fixThead($scope.schema, params);
                var onLoadMessage = contextService.fetchFromContext("onloadMessage", false, false, true);
                if (onLoadMessage) {
                    //if we have a message to display upon page load
                    var data = {
                        successMessage: onLoadMessage
                    };
                    $rootScope.$broadcast('sw_successmessage', data);
                    $timeout(function () {
                        $rootScope.$broadcast('sw_successmessagetimeout', { successMessage: null });
                    }, contextService.retrieveFromContext('successMessageTimeOut'));
                }
                //                if ($rootScope.showSuccessMessage) {
                //                    fixHeaderService.fixSuccessMessageTop(true);
                //                }

                // fix status column height
                $('.statuscolumncolor').each(function (key, value) {
                    if (contextService.isClient('hapag')) {
                        $(value).height($(value).parent().parent().parent().parent().parent().height());
                    }
                });

                //restore the last scroll position, else scroll to the top of the page
                var scrollObject = contextService.fetchFromContext('scrollto', true);

                if (typeof scrollObject === 'undefined') {
                    scrollPosition = 0;
                } else {
                    if ($scope.schema.applicationName === scrollObject.applicationName) {
                        scrollPosition = scrollObject.scrollTop;
                    } else {
                        scrollPosition = 0;
                    }
                }

                $timeout(
                    function () {
                        window.scrollTo(0, scrollPosition);
                        log.info('Scroll To', scrollPosition, scrollObject);
                    }, 100, false);

                $('.no-touch [rel=tooltip]').tooltip({container: 'body'});
                log.debug('finish table rendered listener');
            });


            $scope.$on('sw_togglefiltermode', function (event, setToBasicMode) {
                $scope.advancedsearchdata = "";
                $scope.searchData = {};
                $scope.$broadcast("sw_clearAdvancedFilter");
                $scope.advancedfiltermode = !setToBasicMode;
                fixHeaderService.callWindowResize();

                var first = true;
                for (var data in $scope.searchData) {
                    if ($scope.searchTemplate && first) {
                        //TODO: right now if a template is present, the only possibility is that the advanced filter is present.
                        //Refactor this in the future
                        $scope.advancedsearchdata = replaceAll($scope.searchData[data], '%', '');
                    }
                    $scope.searchData[data] = "";
                    first = false;
                }
                var operator = searchService.getSearchOperationBySymbol("");
                for (var key in $scope.searchOperator) {
                    $scope.searchOperator[key] = operator;
                }
            });

            $scope.$on('sw_gridrefreshed', function (event, data, panelId) {
                $scope.gridRefreshed(data, panelId);
            });

            // When changing grids the search sort should be cleared
            $scope.$on('sw_gridchanged', function(event) {
                $scope.searchSort = {};
            });

            $scope.gridRefreshed = function (data, panelId) {
                if ($scope.panelid != panelId) {
                    //none of my business --> another dashboard event
                    return;
                }

                $scope.schema = data.schema;
                $scope.datamap = data.resultObject;
                $scope.selectAllChecked = false;
                if ($rootScope.printRequested !== true) {
                    //if its a printing operation, then leave the pagination data intact
                    if (data.paginationOptions) {
                        $scope.paginationData = {};
                        $scope.searchValues = data.searchValues;
                        $scope.paginationData.pagesToShow = data.pagesToShow;
                        $scope.paginationData.pageNumber = data.pageNumber;
                        $scope.paginationData.selectedPage = data.pageNumber;
                        $scope.paginationData.pageCount = data.pageCount;
                        $scope.paginationData.pageSize = data.pageSize;
                        $scope.paginationData.paginationOptions = data.paginationOptions;
                        $scope.paginationData.totalCount = data.totalCount;
                        $scope.paginationData.hasPrevious = data.hasPrevious;
                        $scope.paginationData.hasNext = data.hasNext;
                        $scope.paginationData.filterFixedWhereClause = data.filterFixedWhereClause;
                    } else {
                        //if the data is not coming from the server, let´s restore the latest, this is due to a cancel click
                        $scope.paginationData = contextService.fetchFromContext("crud_context", true).paginationData;
                    }

                    $scope.searchData = {};
                    $scope.searchOperator = {};
                    $scope.searchSort = $scope.searchSort || {};

                    if (data.pageResultDto && data.pageResultDto.searchParams) {
                        //TODO: make sure searchSort follows the same logic of building from the server response, then clear the sw_gridchanged event
                        var result = searchService.buildSearchDataAndOperations(data.pageResultDto.searchParams, data.pageResultDto.searchValues);
                        $scope.searchData = result.searchData;
                        $scope.searchOperator = result.searchOperator;
                    }
                }
                contextService.deleteFromContext('grid_refreshdata');
                fixHeaderService.FixHeader();
                //usually this next call won´t do anything, but for lists with optionfields, this is needed
                associationService.updateAssociationOptionsRetrievedFromServer($scope, data.associationOptions, null);
                $scope.$broadcast('sw_griddatachanged', $scope.datamap, $scope.schema, $scope.panelid);

                var elements = [];
                for (var i = 0; i < $scope.datamap.length; i++) {
                    elements.push($scope.datamap[i].fields[$scope.schema.idFieldName]);
                }
                var crudContext = {
                    list_elements: elements,
                    detail_next: "0",
                    detail_previous: "-1",
                    paginationData: $scope.paginationData,
                    previousData: $scope.datamap
                };
                contextService.insertIntoContext("crud_context", crudContext);
            }


            $scope.refreshGridRequested = function (searchData, extraparameters) {


                /// <summary>
                ///  implementation of searchService#refreshgrid see there for details
                /// </summary>
                extraparameters = extraparameters || {};
                if (extraparameters.panelid && extraparameters.panelid != $scope.panelid) {
                    //this is none of my business --> another dashboard will handle it
                    return;
                }

                contextService.deleteFromContext("poll_refreshgridaction" + ($scope.panelid ? $scope.panelid : ""));
                $scope.paginationData = $scope.paginationData || {};
                $scope.searchData = $scope.searchData || {};
                $scope.metadataid = extraparameters.metadataid;

                var pagetogo = extraparameters.pageNumber ? extraparameters.pageNumber : $scope.paginationData.pageNumber;
                var pageSize = extraparameters.pageSize ? extraparameters.pageSize : $scope.paginationData.pageSize;
                var printmode = extraparameters.printMode;
                var keepfilterparameters = extraparameters.keepfilterparameters;
                $scope.searchTemplate = extraparameters.searchTemplate;
                // if search data is present, we should go back to first page, as we wont know exactly the new number of pages available
                if (searchData) {
                    if (keepfilterparameters) {
                        for (var key in searchData) {
                            $scope.searchData[key] = searchData[key];
                        }
                    } else {
                        $scope.searchData = searchData;
                        $scope.advancedsearchdata = null;
                    }
                    pagetogo = 1;
                } else {
                    //let´s clean the advanced filter just in case
                    $scope.advancedsearchdata = null;
                }
                if (extraparameters.avoidspin) {
                    contextService.set("avoidspin", true, true);
                }
                $scope.selectPage(pagetogo, pageSize, printmode);
            };

            $scope.$on('sw_refreshgrid', function (event, searchData, extraparameters) {
                $scope.refreshGridRequested(searchData, extraparameters);
            });

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                if (!$rootScope.showSuccessMessage) {
                    fixHeaderService.resetTableConfig($scope.schema);
                }
            });

            $scope.$on('sw_errormessage', function (event, show) {
                fixHeaderService.topErrorMessageHandler(show, $scope.$parent.isDetail, $scope.schema);
            });

            $scope.doAdvancedSearch = function (filterdata) {
                searchService.advancedSearch($scope.datamap, $scope.schema, filterdata);
            };

            $scope.cursortype = function () {
                var editDisabled = $scope.schema.properties['list.disabledetails'];
                return "true" != editDisabled ? "pointer" : "default";
            };

            $scope.isEditing = function (schema) {
                var idFieldName = schema.idFieldName;
                var id = $scope.datamap.fields[idFieldName];
                return id != null;
            };


            $scope.shouldShowField = function (expression) {
                if (expression == "true") {
                    return true;
                }
                var stringExpression = '$scope.datamap.' + expression;
                var ret = eval(stringExpression);
                return ret;
            };


            $scope.isHapag = function () {
                return $rootScope.clientName == "hapag";
            };

            $scope.safeCSSselector = function (name) {
                return safeCSSselector(name);
            };

            $scope.renderListView = function (parameters) {
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                var listSchema = 'list';
                if ($scope.schema != null && $scope.schema.stereotype.isEqual('list', true)) {
                    //if we have a list schema already declared, keep it
                    listSchema = $scope.schema.schemaId;
                }

                searchService.searchWithData($scope.schema.applicationName, $scope.searchData, listSchema, {
                    searchDTO: parameters.search,
                    metadataid: $scope.metadataid
                });

            };

            $scope.selectPage = function (pageNumber, pageSize, printMode) {
                if (pageNumber === undefined || pageNumber <= 0 || pageNumber > $scope.paginationData.pageCount) {
                    $scope.paginationData.pageNumber = pageNumber;
                    return;
                }
                var totalCount = 0;
                var filterFixedWhereClause = null;
                if ($scope.paginationData != null) {
                    totalCount = $scope.paginationData.totalCount;
                    //if pageSize is specified, use it... this is used for printing function
                    //TODO: refactor calls to always pass pageSize
                    if (pageSize == undefined) {
                        pageSize = $scope.paginationData.pageSize;
                    }
                    filterFixedWhereClause = $scope.paginationData.filterFixedWhereClause;
                }
                if (pageSize === undefined) {
                    //if it remains undefined, use 100
                    pageSize = 100;
                }

                var searchDTO;

                //TODO Improve this solution
                var reportDto = contextService.retrieveReportSearchDTO($scope.schema.schemaId);
                if (reportDto != null) {
                    searchDTO = searchService.buildReportSearchDTO(reportDto, $scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause);
                } else {
                    searchDTO = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause, null, $scope.searchTemplate);
                }

                searchDTO.pageNumber = pageNumber;
                searchDTO.totalCount = totalCount;
                searchDTO.pageSize = pageSize;
                searchDTO.paginationOptions = $scope.paginationData.paginationOptions;

                //avoids table flickering
                fixHeaderService.unfix();

                checkpointService.createGridCheckpoint($scope.schema, searchDTO);

                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                var listSchema = 'list';
                if ($scope.schema != null && $scope.schema.stereotype.isEqual('list', true)) {
                    //if we have a list schema already declared, keep it
                    listSchema = $scope.schema.schemaId;
                }

                $rootScope.printRequested = printMode;

                var searchPromise = searchService.searchWithData($scope.schema.applicationName, $scope.searchData, listSchema,
                {
                    searchDTO: searchDTO,
                    printMode: printMode,
                    schemaFieldsToDisplay: $scope.fieldstodisplay,
                    metadataid: $scope.metadataid
                });

                searchPromise.success(function (data) {
                    $scope.gridRefreshed(data, $scope.panelid);
                });

            };

            $scope.toggleSelectAll = function (checked) {
                $.each($scope.datamap, function (key, value) {
                    value.fields["_#selected"] = checked;
                });
            };

            $scope.selectOperator = function (columnName, operator) {
                var searchOperator = $scope.searchOperator;
                var searchData = $scope.searchData;

                searchOperator[columnName] = operator;

                if (operator.id == "") {
                    searchData[columnName] = '';
                    $scope.selectPage(1);
                } else if (searchData[columnName] != null && searchData[columnName] != '') {
                    $scope.selectPage(1);
                } else if (operator.id == "BLANK") {
                    searchData[columnName] = '';
                    $scope.selectPage(1);
                }
            };

            $scope.filterSearch = function (columnName, event) {

                if ($scope.searchOperator[columnName] == null || $scope.searchOperator[columnName].symbol == "") {
                    $scope.searchOperator[columnName] = searchService.defaultSearchOperation();
                }

                var searchString = $scope.searchData[columnName];
                if (searchString == "" || searchString == null) {
                    $scope.searchOperator[columnName] = searchService.getSearchOperationById("BLANK");
                    $scope.searchData[columnName] = " ";
                }
                $scope.selectPage(1);

            };



            $scope.sort = function (column) {
                if (!$scope.shouldShowHeaderLabel(column) || "none" == $scope.schema.properties["list.sortmode"]) {
                    return;
                }
                var columnName = column.attribute;

                var sorting = $scope.searchSort;
                if (sorting.field != null && sorting.field == columnName) {
                    sorting.order = sorting.order == 'desc' ? 'asc' : 'desc';
                } else {
                    sorting.field = columnName;
                    sorting.order = 'asc';
                }
                $scope.selectPage(1);
            };

            $scope.collapse = function (selector) {
                if ($(selector).is(':visible')) {
                    $(selector).hide();
                } else {
                    $(selector).show();
                }
                fixHeaderService.fixTableTop($(".fixedtable"));
            };


            function initController() {
                $injector.invoke(BaseController, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService,
                    commandService: commandService,
                    formatService: formatService
                });

                $injector.invoke(BaseList, this, {
                    $scope: $scope,
                    formatService: formatService,
                    expressionService: expressionService,
                    searchService: searchService,
                    commandService: commandService
                });

                var dataRefreshed = contextService.fetchFromContext('grid_refreshdata', true, true, true);



                if (dataRefreshed) {
                    $scope.gridRefreshed(dataRefreshed.data, dataRefreshed.panelid);
                }

                var dataToRefresh = contextService.fetchFromContext('poll_refreshgridaction' + ($scope.panelid ? $scope.panelid : ""), true, true, true);
                if (dataToRefresh) {
                    $scope.refreshGridRequested(dataToRefresh.searchData, dataToRefresh.extraparameters);
                }

            }

            //by the end of the controller, so that all the scope functions are already declared
            initController();

        },

        link: function (scope, element, attrs) {
            scope.isDashboard = function (element, panelid) {
                if (panelid != null) {
                    return "width: 100%;";
                } else {
                    return "width: 97.5%; width: -moz-calc(100% - 40px); width: -webkit-calc(100% - 40px); width: calc(100% - 40px);";
                }
            }
        }
    };
});

