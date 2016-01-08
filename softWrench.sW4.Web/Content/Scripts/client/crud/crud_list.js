(function (angular) {
    "use strict";

    var app = angular.module('sw_layout');

    app.directive('advancedFilterToogle', ["contextService", function (contextService) {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/Templates/crud/advanced_filter_toogle.html')
        };
    }]);

    app.directive('crudList', ["contextService", "$timeout", function (contextService, $timeout) {

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

            controller: ["$scope", "$http", "$rootScope", "$filter", "$injector", "$log",
                "formatService", "fixHeaderService", "alertService",
                "searchService", "tabsService",
                "fieldService", "commandService", "i18NService",
                "validationService", "submitService", "redirectService", "crudContextHolderService",
                "associationService", "statuscolorService", "contextService", "eventService", "iconService", "expressionService", "checkpointService", "schemaCacheService",
                function ($scope, $http, $rootScope, $filter, $injector, $log,
                    formatService, fixHeaderService, alertService,
                    searchService, tabsService,
                    fieldService, commandService, i18NService,
                    validationService, submitService, redirectService, crudContextHolderService,
                    associationService, statuscolorService, contextService, eventService, iconService, expressionService, checkpointService, schemaCacheService) {

                    $scope.$name = "crudlist";

                    $scope.vm = {
                        quickSearchData: null
                    }

                    var scrollPosition = 0;

                    fixHeaderService.activateResizeHandler();

                    $scope.loadIcon = function (value, metadata) {
                        return iconService.loadIcon(value, metadata);
                    };

                    $scope.hasTabs = function (schema) {
                        return tabsService.hasTabs(schema);
                    };
                    $scope.isCommand = function (schema) {
                        return $scope.schema && $scope.schema.properties["command.select"] === "true";
                    };
                    $scope.isNotHapagTest = function () {
                        return $rootScope.clientName !== "hapag";
                    };

                    $scope.tabsDisplayables = function (schema) {
                        return tabsService.tabsDisplayables(schema);
                    };

                    $scope.getGridColumnStyle = function (column, propertyName) {
                        var property = column.rendererParameters[propertyName];

                        if (property != null) {
                            return property;
                        }

                        if (propertyName === "maxwidth") {
                            var high = $(window).width() > 1199;
                            if (high) {
                                return "135px";
                            }
                            return "100px";
                        }
                        return null;
                    }

                    $scope.shouldShowSort = function (column, orientation) {
                        var defaultCondition = !!column.attribute && ($scope.searchSort.field === column.attribute || $scope.searchSort.field === column.rendererParameters["sortattribute"]) && $scope.searchSort.order === orientation;
                        return $scope.shouldShowGridNavigation() && defaultCondition;
                    };

                    this.shouldshowtoogleselected = function () {
                        return $scope.schema.properties["list.selectionstyle"] === "multiple";
                    }

                    this.toogleSelectedIcon = function () {
                        var showOnlySelected = crudContextHolderService.getShowOnlySelected($scope.panelid);
                        return showOnlySelected ? "fa-toggle-on" : "fa-toggle-off";
                    }

                    //#region eventlisteners;




                    $scope.$on("filterRowRenderedEvent", function (filterRowRenderedEvent) {
                        if ($scope.datamap && $scope.datamap.length <= 0) {
                            // only update filter visibility if there are no results to shown on grid... else the filter visibility will be updated on "listTableRenderedEvent"
                            fixHeaderService.updateFilterZeroOrOneEntries();
                            // update table heigth (for ie9)
                        }
                    });

                    $scope.$on("listTableRenderedEvent", function (listTableRenderedEvent) {
                        var log = $log.getInstance("sw4.crud_list_dir#on#listTableRenderedEvent");
                        log.debug("init table rendered listener");

                        var parameters = {
                            fullKey: $scope.schema.properties["config.fullKey"],
                            searchData: $scope.searchData
                        };
                        eventService.onload($scope, $scope.schema, $scope.datamap, parameters);

                        if ($scope.ismodal === "true" && !(true === $scope.$parent.showingModal)) {
                            return;
                        }

                        var params = {};
                        fixHeaderService.fixThead($scope.schema, params);
                        var onLoadMessage = contextService.fetchFromContext("onloadMessage", false, false, true);
                        if (onLoadMessage) {
                            alertService.notifymessage("success", onLoadMessage);
                        }

                        // fix status column height
                        $(".statuscolumncolor").each(function (key, value) {
                            if (contextService.isClient("hapag")) {
                                $(value).height($(value).parent().parent().parent().parent().parent().height());
                            }
                        });

                        //restore the last scroll position, else scroll to the top of the page
                        var scrollObject = contextService.fetchFromContext("scrollto", true);

                        if (scrollObject && $scope.schema.applicationName === scrollObject.applicationName) {
                            scrollPosition = scrollObject.scrollTop;
                        } else {
                            scrollPosition = 0;
                        }

                        $timeout(
                            function () {
                                window.scrollTo(0, scrollPosition);
                                log.info("Scroll To", scrollPosition, scrollObject);
                            }, 100, false);

                        $(".no-touch [rel=tooltip]").tooltip({ container: "body", trigger: "hover" });
                        log.debug("finish table rendered listener");
                    });


                    $scope.$on("sw_togglefiltermode", function (event, setToBasicMode) {
                        $scope.vm.quickSearchData = "";
                        $scope.searchData = {};
                        $scope.$broadcast("sw_clearAdvancedFilter");
                        $scope.advancedfiltermode = !setToBasicMode;
                        fixHeaderService.callWindowResize();

                        angular.forEach($scope.searchData, function (data, key) {
                            $scope.searchData[key] = "";
                        });

                        var operator = searchService.getSearchOperationBySymbol("");
                        angular.forEach($scope.searchOperator, function (op, key) {
                            $scope.searchOperator[key] = operator;
                        });
                    });

                    $scope.$on("sw_gridrefreshed", function (event, data, panelId) {
                        $scope.gridRefreshed(data, panelId);
                    });

                    // When changing grids the search sort should be cleared
                    $scope.$on("sw_gridchanged", function (event) {
                        $scope.searchSort = {};
                    });


                    $scope.$on("sw_refreshgrid", function (event, searchData, extraparameters) {
                        $scope.refreshGridRequested(searchData, extraparameters);
                    });

                    $scope.$on('sw_toogleselected', function (event, args) {
                        var panelid = args[0];
                        if ($scope.panelid !== panelid) {
                            return;
                        }
                        crudContextHolderService.toogleShowOnlySelected($scope.panelid);
                        $scope.gridDataChanged(crudContextHolderService.rootDataMap($scope.panelid));
                    });

                    $scope.$on('sw.crud.grid.toggleselectionmode', function (event, args) {
                        var panelid = args[0];
                        if ($scope.panelid !== panelid) {
                            return;
                        }
                        crudContextHolderService.toggleSelectionMode($scope.panelid);

                    });


                    //#endregion

                    $scope.gridRefreshed = function (data, panelId) {
                        if (!!$scope.panelid && $scope.panelid !== panelId) {
                            //none of my business --> another dashboard event
                            return;
                        }
                        $scope.schema = schemaCacheService.getSchemaFromResult(data);
                        $scope.datamap = data.resultObject;
                        crudContextHolderService.updateCrudContext($scope.schema, $scope.datamap, panelId);

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

                            if (data.pageResultDto) {
                                if (data.pageResultDto.quickSearchData) {
                                    $scope.vm.quickSearchData = data.pageResultDto.quickSearchData;
                                    $scope.searchSort = {};

                                } else if (data.pageResultDto.searchParams) {
                                    //TODO: make sure searchSort follows the same logic of building from the server response, then clear the sw_gridchanged event
                                    var result = searchService.buildSearchDataAndOperations(data.pageResultDto.searchParams, data.pageResultDto.searchValues);
                                    $scope.searchData = result.searchData;
                                    $scope.searchOperator = result.searchOperator;
                                }
                            }

                        }

                        contextService.deleteFromContext("grid_refreshdata");
                        fixHeaderService.FixHeader();
                        //usually this next call won´t do anything, but for lists with optionfields, this is needed
                        associationService.updateFromServerSchemaLoadResult(data.associationOptions);
                        $scope.gridDataChanged($scope.datamap);

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

                    $scope.gridDataChanged = function (datamap) {
                        // if show only selected recovers the selected buffer to show
                        // also update the total count of pagination
                        var newDatamap;
                        var showOnlySelected = crudContextHolderService.getShowOnlySelected($scope.panelid);
                        if (!showOnlySelected) {
                            newDatamap = datamap;
                            var paginationData = crudContextHolderService.getOriginalPaginationData($scope.panelid);
                            if (paginationData) {
                                $scope.paginationData.totalCount = paginationData.totalCount;
                            }
                        } else {
                            var selectionBuffer = crudContextHolderService.getSelectionBuffer($scope.panelid);
                            newDatamap = [];
                            for (var o in selectionBuffer) {
                                newDatamap.push(selectionBuffer[o]);
                            }
                            crudContextHolderService.setOriginalPaginationData($scope.paginationData, $scope.panelid);
                            $scope.paginationData.totalCount = newDatamap.length;
                        }

                        var hasMultipleSelector = $scope.schema.properties["list.selectionstyle"] === "multiple";
                        if (hasMultipleSelector) {
                            // inits selectall checkbox with true and updates on loop
                            $scope.selectAllBuffer.selectAllValue = newDatamap.length > 0;

                            for (var i = 0; i < newDatamap.length; i++) {
                                // updates selector checkbox than updates the select all checkbox
                                var selected = $scope.recoverSelected(datamap[i]);
                                $scope.selectAllBuffer.selectAllValue = $scope.selectAllBuffer.selectAllValue && selected;
                            }
                        }

                        $scope.$broadcast("sw_griddatachanged", newDatamap, $scope.schema, $scope.panelid);
                    }

                    // recover the state of a selector checkbox from buffer
                    $scope.recoverSelected = function (row) {
                        var buffer = crudContextHolderService.getSelectionBuffer($scope.panelid);
                        var rowId = row.fields[$scope.schema.idFieldName];
                        var selected = Boolean(buffer[rowId]);
                        row.fields["_#selected"] = selected;
                        return selected;
                    }

                    $scope.refreshGridRequested = function (searchData, extraparameters) {
                        /// <summary>
                        ///  implementation of searchService#refreshgrid see there for details
                        /// </summary>
                        extraparameters = extraparameters || {};
                        if (extraparameters.panelid && extraparameters.panelid.toString() !== $scope.panelid) {
                            //this is none of my business --> another dashboard will handle it
                            return;
                        }
                        contextService.deleteFromContext("poll_refreshgridaction" + ($scope.panelid ? $scope.panelid : ""));

                        $scope.paginationData = $scope.paginationData || {};
                        $scope.searchData = $scope.searchData || {};
                        $scope.metadataid = extraparameters.metadataid;
                        $scope.vm.quickSearchData = extraparameters.quickSearchData;

                        var pagetogo = extraparameters.pageNumber ? extraparameters.pageNumber : $scope.paginationData.pageNumber;
                        var pageSize = extraparameters.pageSize ? extraparameters.pageSize : $scope.paginationData.pageSize;
                        var printmode = extraparameters.printMode;
                        var keepfilterparameters = extraparameters.keepfilterparameters;
                        // $scope.searchTemplate = extraparameters.searchTemplate;

                        // if search data is present, we should go back to first page, as we wont know exactly the new number of pages available
                        if (searchData) {
                            //if (keepfilterparameters) {
                            //    angular.forEach(searchData, function(data, key) {
                            //        $scope.searchData[key] = data;
                            //    });
                            //} else {
                            $scope.searchData = searchData;
                            //}
                            pagetogo = 1;
                        }
                        if (extraparameters.avoidspin) {
                            contextService.set("avoidspin", true, true);
                        }
                        $scope.selectPage(pagetogo, pageSize, printmode);
                    };

                   

                    $scope.getGridCommandPosition = function (propertyName, defaultProperty) {
                        if (!$scope.schema.properties || !$scope.schema.properties[propertyName]) {
                            return defaultProperty;
                        }
                        return $scope.schema.properties[propertyName];
                    }

                    $scope.quickSearch = function (filterdata) {
                        $scope.searchData = {};
                        $scope.searchSort = {};
                        if (!filterdata) {
                            searchService.refreshGrid({});
                            return;
                        }
                        searchService.quickSearch(filterdata);
                    };

                    $scope.cursortype = function () {
                        var editDisabled = $scope.schema.properties["list.disabledetails"];
                        return "true" !== editDisabled ? "pointer" : "default";
                    };

                    $scope.isEditing = function (schema) {
                        var idFieldName = schema.idFieldName;
                        var id = $scope.datamap.fields[idFieldName];
                        return id != null;
                    };


                    $scope.shouldShowField = function (expression) {
                        if (expression === "true") {
                            return true;
                        }
                        var stringExpression = "$scope.datamap." + expression;
                        var ret = eval(stringExpression);
                        return ret;
                    };


                    $scope.isHapag = function () {
                        return $rootScope.clientName === "hapag";
                    };

                    $scope.safeCSSselector = function (name) {
                        return safeCSSselector(name);
                    };

                    $scope.renderListView = function (parameters) {
                        $scope.$parent.multipleSchema = false;
                        $scope.$parent.schemas = null;
                        var listSchema = "list";
                        if ($scope.schema != null && $scope.schema.stereotype.isEqual("list", true)) {
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


                        //TODO Improve this solution
                        var reportDto = contextService.retrieveReportSearchDTO($scope.schema.schemaId);
                        var searchDTO = !!reportDto
                            ? searchService.buildReportSearchDTO(reportDto, $scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause)
                            : searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause, null, $scope.searchTemplate);

                        searchDTO.pageNumber = pageNumber;
                        searchDTO.totalCount = totalCount;
                        searchDTO.pageSize = pageSize;
                        searchDTO.paginationOptions = $scope.paginationData.paginationOptions;
                        searchDTO.quickSearchData = $scope.vm.quickSearchData;

                        //avoids table flickering
                        fixHeaderService.unfix();

                        checkpointService.createGridCheckpoint($scope.schema, searchDTO);

                        $scope.$parent.multipleSchema = false;
                        $scope.$parent.schemas = null;
                        var listSchema = "list";
                        if ($scope.schema != null && $scope.schema.stereotype.isEqual("list", true)) {
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
                            // Set the scroll position to the top of the new page
                            contextService.insertIntoContext("scrollto", { 'applicationName': $scope.applicationName, 'scrollTop': 0 });
                            $scope.gridRefreshed(data, $scope.panelid);
                        });

                    };


                    $scope.sort = function (column) {
                        if (!$scope.shouldShowHeaderLabel(column) || "none" === $scope.schema.properties["list.sortmode"] || !$scope.shouldShowGridNavigation()) {
                            return;
                        }
                        var columnName = column.attribute;

                        var sorting = $scope.searchSort;
                        if (sorting.field != null && sorting.field === columnName) {
                            sorting.order = sorting.order === "desc" ? "asc" : "desc";
                        } else {
                            sorting.field = columnName;
                            sorting.order = "asc";
                        }
                        $scope.selectPage(1);
                    };

                    $scope.sortLabel = function () {
                        if (!$scope.shouldShowGridNavigation()) {
                            return "";
                        }
                        return $scope.i18N("_grid.filter.clicksort", "Click here to sort");
                    }

                    $scope.collapse = function (selector) {
                        if ($(selector).is(":visible")) {
                            $(selector).hide();
                        } else {
                            $(selector).show();
                        }
                        fixHeaderService.fixTableTop($(".fixedtable"));
                    };

                    $scope.filterApplied = function () {
                        $scope.vm.quickSearchData = null;
                        $scope.selectPage(1);
                    };

                    $scope.filterForColumn = function (column) {
                        return $scope.schema.schemaFilters.filters.find(function (filter) {
                            return filter.attribute === column.attribute;
                        });
                    }

                    $scope.selectAllChanged = function () {
                        $scope.$broadcast("sw_selectallchanged", $scope.datamap, $scope.selectAllBuffer.selectAllValue, $scope.panelid);
                    }

                    $scope.shouldShowGridNavigation = function () {
                        return !crudContextHolderService.getShowOnlySelected($scope.panelid);
                    }

                  

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

                        $scope.selectAllBuffer = crudContextHolderService.getSelectAllBuffer();
                        $scope.selectionModel = crudContextHolderService.getSelectionModel();
                        

                        var dataRefreshed = contextService.fetchFromContext("grid_refreshdata", true, true, true);
                        if (dataRefreshed) {
                            $scope.gridRefreshed(dataRefreshed.data, dataRefreshed.panelid);
                        }

                        var dataToRefresh = contextService.fetchFromContext("poll_refreshgridaction" + ($scope.panelid ? $scope.panelid : ""), true, true, true);
                        if (dataToRefresh) {
                            $scope.refreshGridRequested(dataToRefresh.searchData, dataToRefresh.extraparameters);
                        }

                        
                    }

                    //by the end of the controller, so that all the scope functions are already declared
                    initController();

                }],

            link: function (scope, element, attrs) {
                scope.isDashboard = function (el, panelid) {
                    return panelid != null
                        ? "width: 100%;"
                        : "width: 97.5%; width: -moz-calc(100% - 40px); width: -webkit-calc(100% - 40px); width: calc(100% - 40px);";
                };

                //first call when the directive is linked (listener was not yet in place)
                if (scope.schema.displayables) {
                    scope.gridDataChanged(scope.datamap);
                }

                

            }
        };
    }]);

})(angular);