var app = angular.module('sw_layout');

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
            previousschema: '=',
            previousdata: '=',
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            ismodal: '@',
            checked: '=',
            timestamp: '@',
        },

        controller: function ($scope, $http, $rootScope, $filter, $injector, $log, $timeout,
            formatService, fixHeaderService,
            searchService, tabsService,
            fieldService, commandService, i18NService,
            validationService, submitService, redirectService,
            associationService, contextService, schemaCacheService) {

            $scope.$name = 'crudlist';

            fixHeaderService.activateResizeHandler();

            $scope.getFormattedValue = function (value, column) {
                var formattedValue = formatService.format(value, column);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return "";
                }
                return formattedValue == null ? "" : formattedValue;
            };

            $scope.hasTabs = function (schema) {
                return tabsService.hasTabs(schema);
            };
            $scope.isCommand = function (schema) {
                if ($scope.schema && $scope.schema.properties['command.select'] == "true") {
                    return true;
                }
            };

            $scope.getOpenCalendarTooltip = function (attribute) {
                var currentData = $scope.searchData[attribute];
                if (currentData) {
                    return currentData;
                }
                return this.i18N('calendar.date_tooltip', 'Open the calendar popup');
            }

            $scope.isNotHapagTest = function () {
                return $rootScope.clientName != 'hapag';
            }
            $scope.tabsDisplayables = function (schema) {
                return tabsService.tabsDisplayables(schema);
            };

            $scope.shouldShowSort = function(column,orientation) {
                return column.attribute != null && ($scope.searchSort.field == column.attribute || $scope.searchSort.field == column.rendererParameters['sortattribute']) && $scope.searchSort.order == orientation;
            };

            $scope.getGridHeaderStyle = function (column, propertyName) {
                if (!isIe9()) {
                    return null;
                }

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
                if ($scope.ismodal == 'true' && !(true === $scope.$parent.showingModal)) {
                    return;
                }

                var params = {};

                if (!$rootScope.printRequested) {

                    fixHeaderService.fixThead($scope.schema, params);
                    if ($rootScope.showSuccessMessage) {
                        fixHeaderService.fixSuccessMessageTop(true);
                    }
                    // fix status column height
                    $('.statuscolumncolor').each(function (key, value) {
                        $(value).height($(value).parent().parent().height());
                    });

                    $('[rel=tooltip]').tooltip({ container: 'body' });
                    log.debug('finish table rendered listener');

                    //make sure we are seeing the top of the grid 
                    window.scrollTo(0, 0);

                    //add class to last visible th in the filter row
                    $('#listgrid .filter-row').find('th').filter(':visible:last').addClass('last');
                }
            });

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                if (!$rootScope.showSuccessMessage) {
                    fixHeaderService.resetTableConfig($scope.schema);
                }
            });

            $scope.$on('sw_errormessage', function (event, show) {
                fixHeaderService.topErrorMessageHandler(show, $scope.$parent.isDetail, $scope.schema);
            });

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

                if (column.rendererType == "checkbox") {
                    return;
                }

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
                var unionFilterFixedWhereClause = null;
                if ($scope.paginationData != null) {
                    totalCount = $scope.paginationData.totalCount;
                    //if pageSize is specified, use it... this is used for printing function
                    //TODO: refactor calls to always pass pageSize
                    if (pageSize == undefined) {
                        pageSize = $scope.paginationData.pageSize;
                    }
                    filterFixedWhereClause = $scope.paginationData.filterFixedWhereClause;
                    unionFilterFixedWhereClause = $scope.paginationData.unionFilterFixedWhereClause;
                }
                if (pageSize === undefined) {
                    //if it remains undefined, use 100
                    pageSize = 100;
                }

                var searchDTO;

                //TODO Improve this solution
                var reportDto = contextService.retrieveReportSearchDTO($scope.schema.schemaId);
                if (reportDto != null) {
                    reportDto = $.parseJSON(reportDto);
                    searchDTO = searchService.buildReportSearchDTO(reportDto, $scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause);
                } else {
                    searchDTO = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause, unionFilterFixedWhereClause);
                }

                searchDTO.pageNumber = pageNumber;
                searchDTO.totalCount = totalCount;
                searchDTO.pageSize = pageSize;
                searchDTO.paginationOptions = $scope.paginationData.paginationOptions;

                //avoids table flickering
                fixHeaderService.unfix();

                $scope.renderListView({ SearchDTO: searchDTO, printMode: printMode });
            };

            $scope.getSearchIcon = function (columnName) {
                var showSearchIcon = $scope.schema.properties["list.advancedfilter.showsearchicon"] == "true";
                var operator = $scope.getOperator(columnName);

                if (showSearchIcon && operator.symbol != "") {
                    return operator.symbol;
                } else {
                    return "";
                }
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

                // workaround to remove the focus from the filter textbox
                // on ie9, if we dont took the focus out of the textbox, the page breaks something on the rendering 
                // that prevents the click on the grid to show the details
                $('#listgrid').focus();
                window.scrollTo(0, 0);
            };

            $scope.shouldShowFilter = function (column) {
                return column.type == "ApplicationFieldDefinition" && column.rendererType != "color";
            }



            $scope.sort = function (column) {
                if (!$scope.shouldShowFilter(column)) {
                    return;
                }
                var columnName = column.attribute;
                if (column.rendererParameters && column.rendererParameters.sortattribute) {
                    columnName = column.rendererParameters.sortattribute;
                }
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

