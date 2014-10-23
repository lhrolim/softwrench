var app = angular.module('sw_layout');

app.directive('advancedFilterToogle', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/advanced_filter_toogle.html')
    };
});


app.directive('crudList', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_list.html'),
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
            checked: '='
        },

        controller: function ($scope, $http, $rootScope, $filter, $injector, $log, $timeout,
            formatService, fixHeaderService,
            searchService, tabsService,
            fieldService, commandService, i18NService,
            validationService, submitService, redirectService,
            associationService, contextService, statuscolorService, eventdispatcherService) {

            $scope.$name = 'crudlist';
            

            fixHeaderService.activateResizeHandler();

            $scope.getFormattedValue = function (value, column) {
                var formattedValue = formatService.format(value, column);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return null;
                }
                return formattedValue;
            };

            this.test = function(val) {
                $log.warn(val);
            }

            $scope.hasTabs = function (schema) {
                return tabsService.hasTabs(schema);
            };
            $scope.isCommand = function (schema) {
                if ($scope.schema.properties['command.select'] == "true") {
                    return true;
                }
            };
            $scope.isNotHapagTest = function () {
                return $rootScope.clientName != 'hapag';
            }
            $scope.tabsDisplayables = function (schema) {
                return tabsService.tabsDisplayables(schema);
            };

            $scope.$on('filterRowRenderedEvent', function (filterRowRenderedEvent) {
                if ($scope.datamap.length == 0) {
                    // only update filter visibility if there are no results to shown on grid... else the filter visibility will be updated on "listTableRenderedEvent"
                    fixHeaderService.updateFilterZeroOrOneEntries();
                    // update table heigth (for ie9)
                }
            });

            $scope.$on('listTableRenderedEvent', function (listTableRenderedEvent) {
                var log = $log.getInstance('sw4.crud_list_dir#on#listTableRenderedEvent');
                log.debug('init table rendered listener');

                var parameters = {
                    fullKey: "/Global/Grids/ScanBar",
                    searchData: $scope.searchData
                };
                eventdispatcherService.onload($scope.schema, null, parameters);

                if ($scope.ismodal == 'true' && !(true === $scope.$parent.showingModal)) {
                    return;
                }

                var params = {};
                fixHeaderService.fixThead($scope.schema, params);
                if ($rootScope.showSuccessMessage) {
                    fixHeaderService.fixSuccessMessageTop(true);
                }

                // fix status column height
                $('.statuscolumncolor').each(function (key, value) {
                    if (contextService.isClient('hapag')) {
                        $(value).height($(value).parent().parent().parent().parent().parent().height());
                    }
                });

                $('.no-touch [rel=tooltip]').tooltip({ container: 'body' });
                log.debug('finish table rendered listener');
            });

            $scope.$on('sw_togglefiltermode', function (event) {
                $scope.advancedfiltermode = !$scope.advancedfiltermode;
                fixHeaderService.callWindowResize();
            });

            $scope.$on('sw_refreshgrid', function (event, searchData, extraparameters) {
                /// <summary>
                ///  implementation of searchService#refreshgrid see there for details
                /// </summary>
                if (extraparameters == null) {
                    extraparameters = {};
                }
                var pagetogo = extraparameters.pageNumber ? extraparameters.pageNumber : $scope.paginationData.pageNumber;
                var pageSize = extraparameters.pageSize ? extraparameters.pageSize : $scope.paginationData.pageSize;
                var printmode = extraparameters.printMode;
                var keepfilterparameters = extraparameters.keepfilterparameters;
                // if search data is present, we should go back to first page, as we wont know exactly the new number of pages available
                if (searchData) {
                    if (keepfilterparameters) {
                        for (var key in searchData) {
                            $scope.searchData[key] = searchData[key];
                        }
                    } else {
                        $scope.searchData = searchData;
                    }
                    pagetogo = 1;
                }
                $scope.selectPage(pagetogo, pageSize, printmode);
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
                searchService.advancedSearch($scope.datamap,$scope.schema, filterdata);
            }


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


            $scope.showDetail = function (rowdm, column) {

                var mode = $scope.schema.properties['list.click.mode'];
                var popupmode = $scope.schema.properties['list.click.popupmode'];
                var schemaid = $scope.schema.properties['list.click.schema'];
                var fullServiceName = $scope.schema.properties['list.click.service'];

                if (popupmode == "report") {
                    return;
                }

                if (fullServiceName != null) {
                    commandService.executeClickCustomCommand(fullServiceName, rowdm.fields, column);
                    return;
                };

                var id = rowdm.fields[$scope.schema.idFieldName];
                if (id == null || id == "-666") {
                    window.alert('error id is null');
                    return;
                }

                var applicationname = $scope.schema.applicationName;
                if (schemaid == '') {
                    return;
                }
                if (schemaid == null) {
                    schemaid = detailSchema();
                }
                $scope.$emit("sw_renderview", applicationname, schemaid, mode, $scope.title, { id: id, popupmode: popupmode });
            };

            $scope.renderListView = function (parameters) {
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                var listSchema = 'list';
                if ($scope.schema != null && $scope.schema.stereotype.isEqual('list', true)) {
                    //if we have a list schema already declared, keep it
                    listSchema = $scope.schema.schemaId;
                }
                $scope.$emit("sw_renderview", $scope.schema.applicationName, listSchema, 'none', $scope.title, parameters);
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
                    searchDTO = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause);
                }

                searchDTO.pageNumber = pageNumber;
                searchDTO.totalCount = totalCount;
                searchDTO.pageSize = pageSize;
                searchDTO.paginationOptions = $scope.paginationData.paginationOptions;

                //avoids table flickering
                fixHeaderService.unfix();

                $scope.renderListView({ SearchDTO: searchDTO, printMode: printMode });
                if ($scope.advancedfiltermode) {
                    //this workaround is used to clear the data after the advanced search has reached, because the code has lots of comes and goes...
                    //TODO: refatcor search
                    $scope.searchData = {};
                }
            };

            $scope.getSearchIcon = function (columnName) {
                var showSearchIcon = $scope.schema.properties["list.advancedfilter.showsearchicon"] != "false";
                var operator = $scope.getOperator(columnName);
                return showSearchIcon ? operator.symbol : "";
            }

            $scope.searchOperations = function () {
                return searchService.searchOperations();
            }

            $scope.getDefaultOperator = function () {
                return searchService.defaultSearchOperation();
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

            $scope.getOperator = function (columnName) {
                var searchOperator = $scope.searchOperator;
                if (searchOperator != null && searchOperator[columnName] != null) {
                    return searchOperator[columnName];
                }
                return searchService.getSearchOperation(0);
            };

            $scope.filterSearch = function (columnName, event) {

                if ($scope.searchOperator[columnName] == null) {
                    $scope.searchOperator[columnName] = searchService.defaultSearchOperation();
                }

                var searchString = $scope.searchData[columnName];
                if (searchString != null && searchString != '') {
                    $scope.selectPage(1);
                }

            };

            $scope.shouldShowHeaderLabel = function (column) {
                return column.type == "ApplicationFieldDefinition" && column.rendererType != "color";
            }

            $scope.shouldShowHeaderFilter = function (column) {
                return $scope.shouldShowHeaderLabel(column) && "false" != column.rendererParameters["filterenabled"];
            }

            $scope.statusColor = function (status, gridname) {
                return statuscolorService.getColor(status, $scope.schema.applicationName);
            }



            $scope.sort = function (column) {
                if (!$scope.shouldShowFilter(column)) {
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



            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });

        }
    };
});

