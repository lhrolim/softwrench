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
            hidebars: '@',
            checked: '=',
            timestamp: '@',
            associationOptions: "="
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

                $('.no-touch [rel=tooltip]').tooltip({ container: 'body' });
                log.debug('finish table rendered listener');
            });

            $scope.$on('sw_togglefiltermode', function (event) {
                $scope.advancedfiltermode = !$scope.advancedfiltermode;
                fixHeaderService.callWindowResize();
                if (!$scope.advancedfiltermode) {
                    $scope.advancedsearchdata = null;
                }
                for (var data in $scope.searchData) {
                    $scope.searchData[data] = "";
                }
                var operator = searchService.getSearchOperationBySymbol("");
                for (var key in $scope.searchOperator) {
                    $scope.searchOperator[key] = operator;
                }
            });

            $scope.$on('sw_gridrefreshed', function (event, data, printmode) {
                $scope.selectAllChecked = false;
            });
            $scope.refreshGrid = function () {
                $scope.selectPage($scope.paginationData.pageNumber, $scope.paginationData.pageSize, false);
            };

            $scope.$on('sw_refreshgrid', function (event, searchData, extraparameters) {
                /// <summary>
                ///  implementation of searchService#refreshgrid see there for details
                /// </summary>
                if (extraparameters == null) {
                    extraparameters = {
                    };
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
                        $scope.advancedsearchdata = null;
                    }
                    pagetogo = 1;
                }
                if (extraparameters.avoidspin) {
                    contextService.set("avoidspin", true, true);
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

                checkpointService.createGridCheckpoint($scope.schema, searchDTO);

                $scope.renderListView({
                    SearchDTO: searchDTO, printMode: printMode
                });
                if ($scope.advancedfiltermode) {
                    //this workaround is used to clear the data after the advanced search has reached, because the code has lots of comes and goes...
                    //TODO: refatcor search
                    $scope.searchData = {
                    };
                }
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

        }
    };
});

