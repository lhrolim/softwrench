(function (angular) {
    "use strict";
    const app = angular.module('sw_layout');
    app.directive('advancedFiltertoggle', ["contextService", function (contextService) {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/Templates/crud/advanced_filter_toggle.html')
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
                panelid: "@",
                metadataid: "@",
                forprint: "@"
            },

            controller: ["$scope",  "$q", "$rootScope", "$filter", "$injector", "$log",
                "formatService", "fixHeaderService", "alertService", "gridPreferenceService",
                "searchService", "tabsService", "userPreferencesService", "printService", 
                "fieldService", "commandService", "i18NService", "modalService", "multisortService",
                "validationService", "submitService", "redirectService", "crudContextHolderService", "gridSelectionService",
                "associationService", "statuscolorService", "contextService", "eventService", "iconService", "expressionService",
                "checkpointService", "schemaCacheService", "dispatcherService", "schemaService",
                function ($scope, $q, $rootScope, $filter, $injector, $log,
                    formatService, fixHeaderService, alertService, gridPreferenceService,
                    searchService, tabsService, userPreferencesService, printService,
                    fieldService, commandService, i18NService, modalService, multisortService,
                    validationService, submitService, redirectService, crudContextHolderService, gridSelectionService,
                    associationService, statuscolorService, contextService, eventService, iconService, expressionService,
                    checkpointService, schemaCacheService, dispatcherService, schemaService) {

                    $scope.$name = "crudlist";

                    var multiSortVisibleKey = "multiSortVisible";
                    var sortModel = function() {
                        return crudContextHolderService.getSortModel($scope.panelid);
                    }

                    sortModel().multiSortVisible = !!userPreferencesService.getPreference(multiSortVisibleKey);
                    $scope.multiSortVisible = sortModel().multiSortVisible;

                    $scope.toggleMultiSortPanel = function () {
                        $scope.multiSortVisible = !$scope.multiSortVisible;
                        sortModel().multiSortVisible = $scope.multiSortVisible;
                        userPreferencesService.setPreference(multiSortVisibleKey, $scope.multiSortVisible);
                        fixHeaderService.callWindowResize();
                    };

                    $scope.vm = {
                        quickSearchDTO: {
                            compositionsToInclude: []
                        }
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

                    $scope.isCompositionSelectedForSearching = function (composition) {
                        $scope.vm.quickSearchDTO.compositionsToInclude = $scope.vm.quickSearchDTO.compositionsToInclude || [];
                        return $scope.vm.quickSearchDTO.compositionsToInclude.indexOf(composition) !== -1;
                    }

                    $scope.toggleCompositionForSearching = function (composition) {
                        var compositionsToInclude = $scope.vm.quickSearchDTO.compositionsToInclude;
                        if (compositionsToInclude == null) {
                            $scope.vm.quickSearchDTO.compositionsToInclude = [];
                            compositionsToInclude = [];
                        }
                        const idx = compositionsToInclude.indexOf(composition);
                        if (idx === -1) {
                            compositionsToInclude.push(composition);
                        } else {
                            compositionsToInclude.splice(idx, 1);
                        }

                    }

                    $scope.getGridColumnStyle = function (column, propertyName) {
                        const property = column.rendererParameters[propertyName];
                        if (property != null) {
                            return property;
                        }

                        if (propertyName === "maxwidth") {
                            const high = $(window).width() > 1199;
                            if (high) {
                                return "135px";
                            }
                            return "100px";
                        }
                        return null;
                    }

                    $scope.shouldShowSort = function (column, orientation) {
                        const defaultCondition = !!column.attribute && ($scope.searchSort.field === column.attribute || $scope.searchSort.field === column.rendererParameters["sortattribute"]) && $scope.searchSort.order === orientation;
                        return $scope.shouldShowGridNavigation() && defaultCondition;
                    };

                    this.shouldshowtoggleselected = function () {
                        if (!$scope.schema || !$scope.schema.properties) {
                            return false;
                        }
                        return "multiple" === $scope.schema.properties["list.selectionstyle"];
                    }

                    this.save = function () {
                        const saveFn = modalService.getSaveFn();
                        if (saveFn) {
                            const result = saveFn($scope.datamap, $scope.schema);
                            if (result && result.then) {
                                result.then(function () {
                                    modalService.hide();
                                });
                            }
                        }

                    };

                    this.cancel = function () {
                        //TODO: improve this solution, using this as a workaround for cancell calls from modals with list schemas
                        modalService.hide();
                    }

                    // Initial state of selection mode toggle button
                    // depends on "list.selectionmodebydefault" schema property
                    this.selectionModeToggleInitState = function () {
                        return crudContextHolderService.getSelectionModel($scope.panelid).selectionMode;
                    }

                    $scope.gridRefreshed = function (data, panelId, initialLoad) {
                        if ($scope.panelid != panelId) {
                            //none of my business --> another dashboard event 
                            //IMPORTANT: do not make it !==, since null is coming and comparing to undefined
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

                            userPreferencesService.syncCrudPreference($scope.paginationData, "pageSize", "pageSize", $scope.panelid);

                            $scope.searchData = {};
                            $scope.searchOperator = {};
                            $scope.searchSort = {};
                            $scope.vm.quickSearchDTO = {};
                            sortModel().sortColumns = [];

                            if (data.pageResultDto) {
                                if (data.pageResultDto.quickSearchDTO) {
                                    $scope.vm.quickSearchDTO = data.pageResultDto.quickSearchDTO;
                                }

                                if (data.pageResultDto.searchParams) {
                                    var result = searchService.buildSearchDataAndOperations(data.pageResultDto.searchParams, data.pageResultDto.searchValues);
                                    $scope.searchData = result.searchData;
                                    $scope.searchOperator = result.searchOperator;
                                }

                                if (data.pageResultDto["searchSort"]) {
                                    $scope.searchSort.field = data.pageResultDto["searchSort"];
                                    $scope.searchSort.order = data.pageResultDto["searchAscending"] === false ? "desc" : "asc";
                                }

                                const multiSort = data.pageResultDto["multiSearchSort"];
                                if (multiSort) {
                                    sortModel().sortColumns = multiSort;
                                    $scope.multiSortVisible = $scope.multiSortVisible || multisortService.hasMultisort($scope.panelid);
                                }
                            }

                        }

                        contextService.deleteFromContext("grid_refreshdata");
                        fixHeaderService.FixHeader();
                        //usually this next call won´t do anything, but for lists with optionfields, this is needed
                        associationService.updateFromServerSchemaLoadResult(data.associationOptions, null, true);

                        if (!initialLoad) {
                            checkpointService.createGridCheckpointFromGridData($scope.schema, $scope);    
                        }

                        $scope.gridDataChanged($scope.datamap);

                        var elements = $scope.datamap.map(function (item) {
                            const applicationField = schemaService.locateDisplayableByQualifier($scope.schema, "application.name");
                            const detailShemaIdField = schemaService.locateDisplayableByQualifier($scope.schema, "schema.detail.id");
                            const listitem = { id: item[$scope.schema.idFieldName] };
                            if (!!applicationField) listitem.application = item[applicationField.attribute];
                            if (!!detailShemaIdField) listitem.detailSchemaId = item[detailShemaIdField.attribute];

                            return listitem;
                        });
                        var crudContext = {
                            list_elements: elements,
                            detail_next: { id : "0" },
                            detail_previous: { id: "-1" },
                            paginationData: $scope.paginationData,
                            previousData: $scope.datamap,
                            panelid: $scope.panelid,
                            applicationName: $scope.schema.applicationName
                        };
                        contextService.insertIntoContext("crud_context", crudContext);
                    }

                    $scope.gridDataChanged = function (datamap) {
                        if ($scope.schema.properties["list.selectionstyle"] === "multiple") {
                            // if show only selected recovers the selected buffer to show
                            // also update the total count of pagination
                            if (!$scope.selectionModel.showOnlySelected) {
                                const paginationData = crudContextHolderService.getOriginalPaginationData($scope.panelid);
                                if (paginationData && $scope.paginationData) {
                                    $scope.paginationData.totalCount = paginationData.totalCount;
                                }
                            } else {
                                const selectionBuffer = $scope.selectionModel.selectionBuffer;
                                datamap = [];
                                for (let o in selectionBuffer) {
                                    if (selectionBuffer.hasOwnProperty(o)) {
                                        datamap.push(selectionBuffer[o]);
                                    }
                                }
                                crudContextHolderService.setOriginalPaginationData($scope.paginationData, $scope.panelid);
                                $scope.paginationData.totalCount = datamap.length;
                            }

                            gridSelectionService.gridDataChanged(datamap, $scope.schema, $scope.panelid);
                        }

                        // dispatching event for crud_tbody.js render the grid
                        $scope.$broadcast(JavascriptEventConstants.GridDataChanged, datamap, $scope.schema, $scope.panelid);
                    }


                    $scope.cleanup = function() {
                        $scope.paginationData.filterFixedWhereClause = null;
                        $scope.searchData = {};
                        $scope.searchSort = {};
                        sortModel().sortColumns = [];
                        $scope.searchOperator = {};
                        $scope.searchValues = "";
                        $scope.vm.quickSearchDTO = { compositionsToInclude: [] };
                        crudContextHolderService.setSelectedFilter({}, $scope.panelid);
                    }

                    /**
                     * This method might get called during the initialization of the controller itself (i.e page F5), or during a call to searchService#refreshgrid
                     * 
                     * @param {} searchData 
                     * @param {} searchOperator 
                     * @param {} panelid 
                     * @param {} searchSort 
                     * @param {} quickSearchDTO 
                     * @param {} metadataid 
                     * @param {} pageNumber 
                     * @param {} pageSize 
                     * @param {} printMode 
                     * @param {} forcecleanup 
                     * @param {} avoidspin 
                     * @param {} addPreSelectedFilters 
                     * @param {} numberOfPages 
                     * @param {Array} multiSort 
                     * @returns {} 
                     */
                    $scope.refreshGridRequested = function (searchData, searchOperator, 
                    {panelid,searchSort,quickSearchDTO,metadataid,pageNumber,pageSize,printMode,forcecleanup,avoidspin,addPreSelectedFilters,numberOfPages, multiSort,schemaFilterId} = {}) {

                        if (panelid && panelid.toString() !== $scope.panelid) {
                            //this is none of my business --> another dashboard will handle it
                            return;
                        }

                        contextService.deleteFromContext("poll_refreshgridaction" + ($scope.panelid ? $scope.panelid : ""));

                        $scope.paginationData = $scope.paginationData || {};
                        $scope.searchData = searchData || $scope.searchData || {};
                        $scope.searchOperator = searchOperator || $scope.searchOperator || {};

                        $scope.searchSort = searchSort || $scope.searchSort || {};
                        sortModel().sortColumns = multiSort || sortModel().sortColumns;
                        $scope.vm.quickSearchDTO = quickSearchDTO || $scope.vm.quickSearchDTO || { compositionsToInclude: [] };

                        $scope.metadataid = metadataid;
                        

                        var pagetogo = pageNumber ? pageNumber : $scope.paginationData.pageNumber;
                        const newPageSize = pageSize ? pageSize : $scope.paginationData.pageSize;

                        // $scope.searchTemplate = extraparameters.searchTemplate;

                        // if search data is present, we should go back to first page, as we wont know exactly the new number of pages available
                        if (searchData || searchOperator) {
                            pagetogo = 1;
                        }

                        if (avoidspin) {
                            contextService.set("avoidspin", true, true);
                        }
                        
                        if (forcecleanup) {
                            $scope.cleanup();
                        }

                        $scope.selectPage(pagetogo, newPageSize, printMode, {addPreSelectedFilters,numberOfPages,schemaFilterId});
                    };

                    $scope.getGridCommandPosition = function (propertyName, defaultProperty) {
                        if (!$scope.schema.properties || !$scope.schema.properties[propertyName]) {
                            return defaultProperty;
                        }
                        return $scope.schema.properties[propertyName];
                    }

                    $scope.showActionSeparator = function () {
                        const commands = commandService.getBarCommands($scope.schema, 'actions');
                        const buttons = $('.toolbar-secondary.actions button');
                        if (commands === null) {
                            return false;
                        }

                        if (buttons === null || buttons.length === 0) {
                            return false;
                        }

                        return commands.length > 0;
                    }

                    $scope.quickSearch = function (filterdata) {
                        // have a selected saved filter - applies filter considering the quicksearch also
                        const filter = crudContextHolderService.getSelectedFilter($scope.panelid);
                        if (filter) {
                            filterdata = filterdata && filterdata.quickSearchData ? filterdata : { compositionsToInclude: [] };
                            gridPreferenceService.applyFilter(filter, $scope.searchOperator, filterdata, $scope.panelid);
                            return;
                        }

                        // no saved filter selected just do the quick search considering existing search data (filters)
                        searchService.refreshGrid($scope.searchData, $scope.searchOperator, { quickSearchDTO: filterdata, panelid: $scope.panelid });
                    };

                    $scope.cursortype = function () {
                        const editDisabled = $scope.schema.properties["list.disabledetails"];
                        return "true" !== editDisabled ? "pointer" : "default";
                    };

                    $scope.isEditing = function (schema) {
                        const idFieldName = schema.idFieldName;
                        const id = $scope.datamap[idFieldName];
                        return id != null;
                    };


                    $scope.shouldShowField = function (expression) {
                        if (expression === "true") {
                            return true;
                        }
                        const stringExpression = "$scope.datamap." + expression;
                        const ret = eval(stringExpression);
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

                        $log.getInstance("crudlist#renderListView", ["list", "search"]).debug("calling search with data on the server")

                        searchService.searchWithData($scope.schema.applicationName, $scope.searchData, listSchema, {
                            searchDTO: parameters.search,
                            metadataid: $scope.metadataid
                        });

                    };

                    $scope.handleCustomParamProvider = function(searchDTO) {
                        if ($scope.schema.properties && $scope.schema.properties['schema.customparamprovider']) {
                            const customParamProviderString = $scope.schema.properties['schema.customparamprovider'];
                            const customParamProvider = dispatcherService.loadServiceByString(customParamProviderString);
                            if (customParamProvider != null) {
                                const customParams = customParamProvider();
                                if (customParams != null) {
                                    const customParameters = {};
                                    for (let param in customParams) {
                                        if (!customParams.hasOwnProperty(param)) {
                                            continue;
                                        }
                                        customParameters[param] = {};
                                        customParameters[param]["key"] = customParams[param]["key"];
                                        customParameters[param]["value"] = customParams[param]["value"];
                                    }
                                    searchDTO.CustomParameters = customParameters;
                                }
                            }
                        }
                    }


                    $scope.selectPage = function (pageNumber, pageSize, printMode, extraparameters ={}) {
                        
                        if (pageNumber === undefined || pageNumber <= 0 || pageNumber > $scope.paginationData.pageCount) {
                            $scope.paginationData.pageNumber = pageNumber;
                            return $q.when();
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

                        if (extraparameters instanceof SearchDTO) {
                            searchDTO = extraparameters;
                        } else {
                            searchDTO = 
                                 searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause, null, $scope.searchTemplate, null, sortModel().sortColumns);    
                            searchDTO.pageNumber = pageNumber;
                            searchDTO.totalCount = totalCount;
                            searchDTO.pageSize = pageSize;
                            searchDTO.numberOfPages = extraparameters.numberOfPages;
                            searchDTO.schemaFilterId = extraparameters.schemaFilterId;
                            searchDTO.paginationOptions = $scope.paginationData.paginationOptions;
                            searchDTO.quickSearchDTO = $scope.vm.quickSearchDTO;
                            searchDTO.addPreSelectedFilters = extraparameters.addPreSelectedFilters ? true : false;
                        }
                   
                        // Check for custom param provider
                        $scope.handleCustomParamProvider(searchDTO);

                        //avoids table flickering
                        fixHeaderService.unfix();
                        let listSchemaId = "list";

                        if ($scope.schema != null && $scope.schema.stereotype.isEqual("list", true)) {
                            //if we have a list schema already declared, keep it
                            listSchemaId = $scope.schema.schemaId;
                        }

                        $rootScope.printRequested = printMode;
                        return searchService.searchWithData($scope.schema.applicationName, $scope.searchData, listSchemaId,{
                            searchDTO: searchDTO,
                            printMode: printMode,
                            schemaFieldsToDisplay: $scope.fieldstodisplay,
                            metadataid: $scope.metadataid,
                            saveSwGlobalRedirectURL: typeof $scope.panelid === "undefined" && $scope.ismodal !== "true",
                            addToHistory: typeof $scope.panelid === "undefined" && !printMode
                        }).then(response => {
                            var data = response.data;
                            // Set the scroll position to the top of the new page
                            contextService.insertIntoContext("scrollto", { 'applicationName': $scope.applicationName, 'scrollTop': 0 });
                            if (!printMode) {
                                $scope.gridRefreshed(data, $scope.panelid);
                            } else {
                                printService.readyToPrintList(data.resultObject);
                            }
                        });

                    };

                    $scope.sort = function (column) {
                        if (!$scope.shouldShowHeaderLabel(column) || "none" === $scope.schema.properties["list.sortmode"] || !$scope.shouldShowGridNavigation() || column.rendererParameters.showsort === "false") {
                            return;
                        }
                        const columnName = column.attribute;
                        const sorting = $scope.searchSort;
                        if (sorting.field != null && sorting.field === columnName) {
                            if (sorting.order === "asc") {
                                sorting.order = "desc";
                            } else {
                                $scope.searchSort = {};
                            }
                        } else {
                            sorting.field = columnName;
                            sorting.order = "asc";
                        }
                        sortModel().sortColumns = [];
                        $scope.selectPage(1);
                    };

                    $scope.multisort = function (addPreSelectedFilters) {
                        $scope.searchSort = {};
                        $scope.selectPage(1, $scope.paginationData, false, { addPreSelectedFilters: addPreSelectedFilters});
                    };

                    $scope.sortLabel = function (column) {
                        if (!$scope.shouldShowGridNavigation() || column.rendererParameters.showsort === "false") {
                            return "";
                        }
                        return $scope.i18N("_grid.filter.clicksort", "{0}, Click to sort".format(column.toolTip ? column.toolTip : column.label));
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
                        $scope.selectPage(1);
                    };

                    $scope.filterForColumn = function (column) {
                        const filter = $scope.schema.schemaFilters.filters.find(filter => filter.attribute === column.attribute);

                        if (!!column && !!column.rendererType) {
                            if (!!filter) {
                                filter.rendererType = column.rendererType;
                            }
                        }

                        return filter;
                    }

                    // called when the state of select all checkbox changes from user action
                    $scope.selectAllChanged = function () {
                        gridSelectionService.selectAllChanged($scope.datamap, $scope.schema, $scope.panelid);
                    }

                    $scope.shouldShowGridNavigation = function () {
                        return !$scope.selectionModel.showOnlySelected && "true" !== $scope.schema.properties["list.disablepagination"];
                    }

                    $scope.shouldShowSearchOptions = function () {
                        if (!$scope.schema.relatedCompositions) {
                            return true;
                        }

                        return !$scope.schema.relatedCompositions.length > 0;
                    }

                    $scope.noRecordsNewButtonLabel = function () {
                        const create = i18NService.get18nValue("_grid.filter.noresultWithNewCreate", "Create");
                        return create + " " + $scope.schema.applicationTitle;
                    }

                    $scope.noRecordsNewClick = function () {
                        // calls the preaction function if needed and pass the created datamap to the next schema
                        const noResultsPreAction = $scope.schema.properties["list.noresultspreaction"];
                        var datamap = null;
                        if (noResultsPreAction) {
                            const preAction = dispatcherService.loadServiceByString(noResultsPreAction);
                            if (preAction) {
                                datamap = preAction();
                                if (datamap.then) {
                                    return datamap.then(function(datamap) {
                                        return redirectService.goToApplication($scope.schema.applicationName, $scope.schema.noResultsNewSchema, null, datamap);
                                    });
                                }
                            }
                        }
                        redirectService.goToApplication($scope.schema.applicationName, $scope.schema.noResultsNewSchema, null, datamap);
                    }


                    //#region eventlisteners



                    $scope.$on(JavascriptEventConstants.FilterRowRendered, function (filterRowRenderedEvent) {
                        if ($scope.datamap && $scope.datamap.length <= 0) {
                            // only update filter visibility if there are no results to shown on grid... else the filter visibility will be updated on "listTableRenderedEvent"
                            fixHeaderService.updateFilterZeroOrOneEntries();
                            // update table heigth (for ie9)
                        }
                    });

                    $scope.$on(JavascriptEventConstants.ListTableRendered, function (listTableRenderedEvent) {
                        var log = $log.getInstance("sw4.crud_list_dir#on#listTableRenderedEvent");
                        log.debug("init table rendered listener");
                        const parameters = {
                            fullKey: $scope.schema.properties["config.fullKey"],
                            searchData: $scope.searchData
                        };
                        eventService.onload($scope, $scope.schema, $scope.datamap, parameters);

                        if ($scope.ismodal === "true" && !(true === $scope.$parent.showingModal)) {
                            return;
                        }
                        const params = {};
                        fixHeaderService.fixThead($scope.schema, params);
                        const onLoadMessage = contextService.fetchFromContext("onloadMessage", false, false, true);
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

                    $scope.$on(JavascriptEventConstants.GRID_REFRESHED, function (event, data, panelId) {
                        $scope.gridRefreshed(data, panelId,true);
                    });

                    $scope.$on(JavascriptEventConstants.ToggleSelected, function (event, args) {
                        const panelid = args[0];
                        if ($scope.panelid !== panelid) {
                            return;
                        }
                        crudContextHolderService.toggleShowOnlySelected($scope.panelid);
                        $scope.gridDataChanged(crudContextHolderService.rootDataMap($scope.panelid));
                    });


                    $scope.$on(JavascriptEventConstants.ToggleSelectionMode, function (event, args) {
                        const panelid = args[0];
                        if ($scope.panelid === panelid) {
                            crudContextHolderService.toggleSelectionMode($scope.panelid);
                        }
                    });

                    $scope.$on(JavascriptEventConstants.ClearQuickSearch, function (event, args) {
                        if ($scope.panelid === args[0]) {
                            $scope.vm.quickSearchDTO.quickSearchData = null;
                        }
                    });

                    $scope.$on(JavascriptEventConstants.RefreshGrid, function (event, searchData, searchOperator, extraparameters) {
                        if ($scope.panelid != extraparameters.panelid) {
                            //DO NOT use !== here to avoid undefined and null comparisons to fail
                            return;
                        }
                        //enforcing correct schema
                        $scope.schema = crudContextHolderService.currentSchema($scope.panelid);
                        $scope.refreshGridRequested(searchData, searchOperator, extraparameters);
                    });
                    //#endregion

                    $scope.$watch("selectionModel.selectionMode", function (newValue) {
                        const toggleCommand = crudContextHolderService.getToggleCommand("toggleselectionmode", $scope.panelid);
                        if (toggleCommand) {
                            toggleCommand.state = newValue;
                        }
                    });


                    this.hasNewSchema = function() {
                        return $scope.schema.newSchemaRepresentation != null;
                    }

                    this.newShemaLabel = function () {
                        return $scope.schema.newSchemaRepresentation && $scope.schema.newSchemaRepresentation.label;
                    }

                    this.newEntry = function () {
                        return redirectService.goToApplication($scope.schema.applicationName, $scope.schema.newSchemaRepresentation.schemaId);
                    }


                    this.toggleSelectionModeInitialValue = function () {
                        return crudContextHolderService.getSelectionModel($scope.panelid).selectionMode;
                    }

                    function initController() {
                        const log = $log.getInstance("crudlist#init", ["grid","init","route","navigation"]);
                        log.debug("crudlist controller init...")
                        $injector.invoke(BaseController, this, {
                            $scope,
                            i18NService,
                            fieldService,
                            commandService,
                            formatService
                        });

                        $injector.invoke(BaseList, this, {
                            $scope,
                            formatService,
                            expressionService,
                            searchService,
                            commandService,
                            gridSelectionService
                        });

                        const dataRefreshed = contextService.fetchFromContext("grid_refreshdata", true, true, true);
                        if ($scope.ismodal === "true") {
                            $scope.panelid = modalService.panelid;
                        }

                        $scope.selectionModel = crudContextHolderService.getSelectionModel($scope.panelid);

                        if (dataRefreshed) {
                            log.debug("data was already fetched from server... directive was compiled after the response");
                            $scope.gridRefreshed(dataRefreshed.data, dataRefreshed.panelid,true);
                        }
                        const pageSize = userPreferencesService.getSchemaPreference("pageSize", $scope.schema.applicationName, $scope.schema.schemaId, $scope.panelid);
                        const dataToRefresh = contextService.fetchFromContext("poll_refreshgridaction" + ($scope.panelid ? $scope.panelid : ""), true, true, true);
                        if (dataToRefresh) {
                            log.debug("there was already a scheduled call to refresh data from the server");
                            if (pageSize) {
                                dataToRefresh.extraparameters = dataToRefresh.extraparameters || {};
                                dataToRefresh.extraparameters.pageSize = pageSize;
                            }
                            $scope.refreshGridRequested(dataToRefresh.searchData, dataToRefresh.searchOperator, dataToRefresh.extraparameters);
                        }

                        if (!dataRefreshed && !dataToRefresh) {
                            $scope.searchSort = $scope.searchOperator = $scope.searchData = {};
                            const fixedWhereClause = crudContextHolderService.getFixedWhereClause($scope.panelid);
                            const searchDTO = {};
                            if (fixedWhereClause) {
                                searchDTO.filterFixedWhereClause = fixedWhereClause;
                            }

                            if (pageSize) {
                                searchDTO.pageSize = pageSize;
                            }

                            if ($scope.forprint === "true") {
                                return;
                            }

                            const searchPromise = searchService.searchWithData($scope.schema.applicationName, $scope.searchData, $scope.schema.schemaId, {
                                searchDTO: searchDTO,
                                printMode: false,
                                metadataid: $scope.metadataid
                            });

                            searchPromise.then(function (data) {
                                // Set the scroll position to the top of the new page
                                contextService.insertIntoContext("scrollto", { 'applicationName': $scope.applicationName, 'scrollTop': 0 });
                                $scope.gridRefreshed(data.data, $scope.panelid);
                            }).catch(err=> console.log(err));
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