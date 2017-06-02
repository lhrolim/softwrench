(function (angular, app) {
    "use strict";

    function CompositionListController($scope, $q, $log, $timeout, $filter, $injector, $http, $element, $rootScope, i18NService, tabsService, alertService,
        formatService, fieldService, commandService, compositionService, validationService, dispatcherService, cmpAutocompleteClient, userPreferencesService, associationService,
        expressionService, modalService, redirectService, eventService, iconService, cmpfacade, crud_inputcommons, spinService, crudContextHolderService, gridSelectionService,
        schemaService, contextService, fixHeaderService, applicationService, searchService, compositionCommons, compositionListViewModel) {


        $scope.searchData = {};
        $scope.searchOperator = {};
        $scope.searchSort = {};
        $scope.filtersDatamap = {};
        $scope.panelId = null;
        $scope.selectAllChecked = false;

        const blankArr = [];

        const batchExpandReadOnly = fieldService.isPropertyTrue($scope.compositionschemadefinition, "composition.inline.expandreadonly");
        const batchEditFunction = $scope.compositionschemadefinition.rendererParameters && $scope.compositionschemadefinition.rendererParameters["composition.inline.editfunction"];
        const batchForceShowRemove = fieldService.isPropertyTrue($scope.compositionschemadefinition, "composition.inline.forceshowremove");
        const batchForceHideRemove = fieldService.isPropertyTrue($scope.compositionschemadefinition, "composition.inline.forcehideremove");
        $scope.batchAddTitle = ($scope.compositionschemadefinition.rendererParameters && $scope.compositionschemadefinition.rendererParameters["composition.inline.addTitle"]) || "Add Item";

        $scope.filterForColumn = function (column) {
            return compositionListViewModel.filterForColumn($scope.compositionlistschema, column);
        }

        $scope.isShowingComposition = function () {
            return expressionService.evaluate($scope.metadatadeclaration.showExpression, $scope.parentdata);
        }

        $scope.nonHiddenDisplayables = function () {
            
            if (!$scope.compositionlistschema) {
                return blankArr;
            }

            return $scope.compositionlistschema.displayables.filter(f => !f.isHidden);
        }

        $scope.filterApplied = function () {
            $scope.paginationData.pageNumber = 1;
            const searchDTO = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, null, $scope.paginationData);
            compositionService.searchCompositionList($scope.relationship, $scope.parentschema, $scope.parentdata, searchDTO).then(
                result => $scope.refreshList(result[$scope.relationship])
            ).finally(() => crudContextHolderService.setDetailDataResolved());
        };

        $scope.shouldShowSort = function (column, orientation) {
            if ($scope.hideFilter()) {
                return false;
            }
            return !!column.attribute && ($scope.searchSort.field === column.attribute || $scope.searchSort.field === column.rendererParameters["sortattribute"]) && $scope.searchSort.order === orientation;
        };


        $scope.showInlineHeader = function () {
            if (!$scope.inline || !$scope.isNoRecords()) {
                //not inline, or having at least one entry --> show header
                return true;
            }
            return !$scope.compositionschemadefinition.rendererParameters["composition.inline.avoidheader"];
        };

        $scope.sort = function (column) {
            const columnName = column.attribute;
            const sorting = $scope.searchSort;
            if (sorting.field && sorting.field === columnName) {
                sorting.order = sorting.order === "desc" ? "asc" : "desc";
            } else {
                sorting.field = columnName;
                sorting.order = "asc";
            }
            $scope.filterApplied();
        };

        $scope.sortLabel = function (column) {
            return $scope.i18N("_grid.filter.clicksort", "{0}, Click to sort".format(column.toolTip ? column.toolTip : column.label));
        }

        $scope.hideFilter = function () {
            if ($scope.isBatch()) {
                return true;
            }
            if (!$scope.compositionlistschema) {
                return false;
            }
            return schemaService.isPropertyTrue($scope.compositionlistschema, "compositions.disable.filter");
        }


        $scope.showTableHover = function () {
            return (($scope.compositionlistschema != undefined && $scope.compositionlistschema.properties['list.click.event'] != undefined) || $scope.compositiondetailschema != undefined) && $scope.compositiondata.length > 0;
        };

        $scope.clearCompositionData = function () {
            const arr = $scope.compositionData();
            const previousCompositionData = $scope.compositionData();
            arr.splice(0, arr.length);
            $scope.paginationData = null;
            const parameters = {
                parentdata: $scope.parentdata,
                parentschema: $scope.parentschema,
                relationship: $scope.relationship,
                clonedCompositionData: arr,
                previousData: previousCompositionData
            };
            eventService.onload($scope, $scope.compositionlistschema, $scope.parentdata, parameters);
        }

        $scope.compositionData = function () {
            if ($scope.isBatch() || !$scope.hasDetailSchema()) {
                return $scope.compositiondata;
            }
            return $scope.clonedCompositionData;
        };

        $scope.isFieldRequired = function (item, requiredExpression) {
            return expressionService.evaluate(requiredExpression, item);
        };

        function isCompositionRequired() {
            return expressionService.evaluate($scope.compositionschemadefinition.requiredRelationshipExpression, null, $scope);
        }

        $scope.hideRemoveBatchItem = function (compositionitem, rowindex) {
            return (compositionitem[$scope.compositionlistschema.idFieldName] > 0 && !batchForceShowRemove) || ($scope.compositionData().length === 1 && isCompositionRequired());
        }

        $scope.isCompositionItemFieldHidden = function (application, fieldMetadata, item) {
            const datamap = item == null ? $scope.parentdata : compositionService.buildMergedDatamap(item, $scope.parentdata);
            return fieldService.isFieldHidden(datamap, application, fieldMetadata);
        };


        $scope.initField = function (fieldMetadata, item) {
            const idx = $scope.compositionData().indexOf(item);
            crud_inputcommons.initField($scope, fieldMetadata, "compositiondata[{0}]".format(idx), idx);
        };

        $scope.uploadAcceptedFiles = function (fieldMetadata) {
            if (!fieldMetadata || !fieldMetadata.rendererParameters || !fieldMetadata.rendererParameters.acceptedfiles) {
                return ".jpg,.bmp,.png,.pdf,.zip,text/plain,.doc,.docx,.dwg,.csv,.xls,.xlsx,.ppt,application/xml,.xsl,text/html";
            }
            return fieldMetadata.rendererParameters.acceptedfiles;
        }

        function watchForDirty(index) {
            return $scope.$watch("compositiondata[{0}]".format(index), function (newValue, oldValue) {
                //make sure any change on the composition marks it as dirty
                if (oldValue !== newValue && $scope.compositiondata[index]) {
                    $scope.compositiondata[index][CompositionConstants.IsDirty] = true;
                }
            }, true);
        }

        $scope.init = function (previousCompositionData, isPaginationRefresh) {
            //$scope.compositionschemadefinition matches ApplicationCompositionSchema
            if (!$scope.compositionschemadefinition.schemas) {
                angular.extend($scope.compositionschemadefinition, compositionCommons.buildInlineDefinition($scope.compositionschemadefinition));
            }

            //Extra scope variables
            $scope.compositionlistschema = $scope.compositionschemadefinition.schemas.list;
            $scope.compositiondetailschema = $scope.compositionschemadefinition.schemas.detail;

            $scope.fetchfromserver = $scope.compositionschemadefinition.fetchFromServer;
            $scope.collectionproperties = $scope.compositionschemadefinition.collectionProperties;
            $scope.inline = $scope.compositionschemadefinition.inline;

            $scope.detailData = $scope.clonedData = {};
            $scope.parentdata = $scope.parentdata || crudContextHolderService.rootDataMap();

            $scope.noupdateallowed = !$scope.isBatch() && !expressionService.evaluate($scope.collectionproperties.allowUpdate, $scope.parentdata);
            $scope.nodeleteallowed = !expressionService.evaluate($scope.collectionproperties.allowRemoval, $scope.parentdata);
            $scope.expanded = $scope.wasExpandedBefore = false;

            $scope.isReadonly = !expressionService.evaluate($scope.collectionproperties.allowUpdate, $scope.parentdata);


            $injector.invoke(BaseList, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService,
                formatService: formatService,
                gridSelectionService: gridSelectionService
            });

            contextService.insertIntoContext('clonedCompositionData', $scope.compositionData(), true);
            const parameters = {
                parentdata: $scope.parentdata,
                parentschema: $scope.parentschema,
                previousData: previousCompositionData,
                element: $element,
                relationship: $scope.relationship,
                clonedCompositionData: $scope.compositionData(),
                paginationApplied: isPaginationRefresh
            };
            eventService.onload($scope, $scope.compositionlistschema, $scope.parentdata, parameters);


            if (!$scope.paginationData) {
                //case the tab is loaded after the event result, the event would not be present on the screen
                $scope.paginationData = contextService.get("compositionpagination_{0}".format($scope.relationship), true, true);
            }

            if ($scope.paginationData) {
                userPreferencesService.syncSchemaPreference($scope.paginationData, "pageSize", "compositionPageSize", $scope.parentschema.applicationName, $scope.parentschema.schemaId);
            }

            if ($scope.isBatch()) {
                initBatches();
            } else {
                initNonBatches();
            }
            const eventData = compositionService.pollCompositionEvent($scope.relationship);
            if (eventData) {
                $scope.onAfterCompositionResolved(null, eventData);
            }

        }

        function initNonBatches() {
            if ($scope.hasDetailSchema()) {
                //we shall just clone the composition array if we're dealing with a non batch operation, 
                //because then the original datamap shall only be updated on server return
                $scope.clonedCompositionData = [];
                $scope.clonedCompositionData = JSON.parse(JSON.stringify($scope.compositiondata));
            }
            if (compositionService.hasEditableProperty($scope.compositionlistschema)) {
                $.each($scope.compositionData(), function (key, value) {
                    fieldService.fillDefaultValues($scope.compositionlistschema.displayables, value, $scope);
                });
            }
        }

        function initBatches(initialDeclaration = true) {

            if ($scope.unWatcherArray && ($scope.unWatcherArray instanceof Array)) {
                $scope.unWatcherArray.forEach(function (unwatcher) {
                    unwatcher();
                });
            }

            $scope.unWatcherArray = [];

            let watches = [];

            $scope.compositionData().forEach(function (value, index, array) {
                //for eventually already existing items
                const id = schemaService.getId(value, $scope.compositionlistschema);
                watches = watches.concat(crud_inputcommons.configureAssociationChangeEvents($scope,
                    "compositiondata[{0}]".format(index), $scope.compositionlistschema.displayables, id));
                watches.push(watchForDirty(index));
            });

            $scope.unWatcherArray = watches;

            if (initialDeclaration && fieldService.isPropertyTrue($scope.compositionschemadefinition, "composition.inline.startwithentry")) {
                //inline composition should apear with an initial item
                //add BatchItem already configure the listeners
                $scope.addBatchItem();
            }
        }

        $scope.getApplicationPath = function (datamap, fieldMetadata) {
            const path = fieldMetadata.applicationPath + schemaService.getId(datamap, $scope.compositionlistschema);
            return replaceAll(path, "\\.", "_");
        }

        $scope.isNoRecords = function () {
            return $scope.compositiondata.length <= 0;
        }

        $scope.showPagination = function () {
            return !$scope.isNoRecords() && // has items to show
                !!$scope.paginationData &&
                $scope.paginationData.paginationOptions.some((option) => {
                    // totalCount is bigger than at least one option
                    return option !== 0 && $scope.paginationData.totalCount > option;
                });
        }


        $scope.onAfterCompositionResolved = function (event, compositiondata) {

            if (!compositiondata || !compositiondata.hasOwnProperty($scope.relationship)) {
                //this is not the data this tab is interested
                return;
            }

            compositionService.pollCompositionEvent();
            const log = $log.get("compositionlist#resolved", ["composition"]);
            spinService.stop({ compositionSpin: true });
            const thisCompData = compositiondata[$scope.relationship];
            if (thisCompData == null) {
                log.debug("cleaning up composition data (but keeping same array)");
                $scope.clearCompositionData();
                return;
            }
            const list = thisCompData.list || thisCompData.resultList;
            log.debug("composition data refreshed for {0} | entries: {1}".format($scope.relationship, list.length));

            $scope.paginationData = thisCompData.paginationData;
            $scope.compositiondata = list;
            $scope.parentdata[$scope.relationship] = list;

            $scope.init();


            try {
                $scope.$digest();
            } catch (e) {
                //digest already in progress...
            }

        };

        $scope.$on(JavascriptEventConstants.CrudSaved, () => {
            $scope.clearNewCompositionDataForBatches();
        });

        $scope.$on(JavascriptEventConstants.COMPOSITION_RESOLVED, $scope.onAfterCompositionResolved);

        $scope.getBooleanClass = function (compositionitem, attribute) {
            if (formatService.isChecked(compositionitem[attribute])) {
                return 'fa-check-square-o';
            }
            return 'fa-square-o';
        }

        $scope.safeCSSselector = function (name) {
            return safeCSSselector(name);
        };

        $scope.haslookupModal = function (schema) {
            return fieldService.getDisplayablesOfRendererTypes(schema.displayables, ['lookup']).length > 0;
        }

        $scope.isRowHidden = function (compositionlistschema, collectionproperties, compositionitem) {
            if (collectionproperties.hideExistingData == true) {
                const idFieldName = compositionlistschema.idFieldName;
                return compositionitem[idFieldName] != null;
            }
            return false;
        }

        $scope.isChecked = function (value) {
            return formatService.isChecked(value);
        }

        $scope.determineCheckBoxValueType = function (fieldMetadata, datamap, isTrueValue) {
            switch (fieldMetadata.dataType) {
                case "smallint":
                    return isTrueValue ? 1 : 0;
                    break;

                case "boolean":
                    return isTrueValue ? 'true' : 'false';
                    break;
            }
        };

        $scope.invokeCustomCheckBoxService = function (fieldMetadata, datamap, $event) {
            if (fieldMetadata.rendererParameters["clickservice"] == null) {
                return;
            }
            const customfn = dispatcherService.loadServiceByString(fieldMetadata.rendererParameters["clickservice"]);
            $q.when(customfn(fieldMetadata, $scope.parentdata, datamap));
            $event.stopImmediatePropagation();
        }


        $scope.isModifiableEnabled = function (fieldMetadata, item) {
            const result = expressionService.evaluate(fieldMetadata.enableExpression, compositionService.buildMergedDatamap(item, $scope.parentdata), $scope);
            return result;
        };

        this.shouldShowAdd = function () {
            return expressionService.evaluate($scope.collectionproperties.allowInsertion, $scope.parentdata) && $scope.collectionproperties.autoCommit;
        }

        this.shouldShowSave = function () {
            return $scope.mode !== "output";
        }

        $scope.shouldShowSave = function () {
            return $scope.mode !== "output";
        }

        $scope.shouldShowAdd = function () {
            return expressionService.evaluate($scope.collectionproperties.allowInsertion, $scope.parentdata) && $scope.collectionproperties.autoCommit;
        }

        this.getAddIcon = function () {
            var iconCompositionAddbutton = $scope.compositionschemadefinition.schemas.list.properties['icon.composition.addbutton'];
            if (!iconCompositionAddbutton) {
                //use the same as the tab by default
                iconCompositionAddbutton = $scope.compositionschemadefinition.schemas.list.properties['icon.composition.tab'];
            }
            return iconCompositionAddbutton;
        }

        this.getAddLabel = function () {
            const labelOverride = $scope.compositiondetailschema.properties['add.button.label'];
            if (labelOverride) {
                return $scope.i18N($scope.relationship + '.add', labelOverride);
            } else {
                return $scope.i18N($scope.relationship + '.add', 'Add ' + $scope.title);
            }
        }

        this.newDetailFn = function () {
            $scope.isUpdate = true;
            const datamap = {
                _iscreation: true
            };
            return $scope.edit(datamap);
        };

        this.save = function () {
            return $scope.save();
        }

        this.cancel = function () {
            $('#crudmodal').modal('hide');
            if (GetPopUpMode() === 'browser') {
                close();
            }

            $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
            $scope.$emit('sw_cancelclicked');
        };

        $scope.draggableValue = function () {
            return $scope.relationship.contains("attachment") ? "true" : "false";
        };





        //#region attachments

        function onAttachmentFileDropped(event, file) {
            // remove preview element if a new file is added
            getFileUploadFields().forEach(function (field) {
                if (file.field !== field.attribute) {
                    // this field was handled by the event emitter: no need to clean it
                    field.rendererParameters["showImagePreview"] = false;
                }
                $scope.$emit("sw.attachment.file.changed", file.name);
            });

        }


        // drag-out file download
        (function () {
            // drag-out download only works in Chrome
            if (!isChrome()) return;
            if (!$scope.relationship.contains("attachment")) return;

            function dragStartListener(event) {
                // resolve composition item from the event
                const evt = (event.originalEvent || event);
                const src = evt.srcElement;
                const item = angular.element(src).scope().compositionitem;
                if (!item) return;

                // add extension to fileName if it does't have
                var fileName = item["document"];
                if (fileName.lastIndexOf(".") < 0) {
                    const fileUrl = item["docinfo_.urlname"];
                    const extension = fileUrl.substring(fileUrl.lastIndexOf(".") + 1);
                    fileName += ("." + extension);


                    // download = '<mime_type>:<file_name_on_save>:<file_download_url>'
                    const downloadData = "application/octet-stream:" + fileName + ":" + item["download_url"];
                    evt.dataTransfer.setData("DownloadURL", downloadData);
                }
                // adding the listener to the parent element so it still triggers on pagination
                var list = $element[0].querySelector("#compositionlistgrid");
                angular.element(list).on("dragstart", dragStartListener);
                $scope.$on("$destroy", function () {
                    angular.element(list).off("dragstart", dragStartListener);
                });

            }
        })();


        function getFileUploadFields() {
            return $scope.compositiondetailschema.displayables
                .filter(function (field) { // file upload fields
                    const renderer = field.renderer.rendererType;
                    return !!renderer && renderer.endsWith("upload");
                });
        }

        function onAttachmentFileLoaded(event, file) {
            if (!$scope.relationship.contains("attachment")) return;

            var datamap = crudContextHolderService.rootDataMap("#modal") || {};
            datamap["newattachment_path"] = file.name;

            // set file on the datamap
            getFileUploadFields()
                .forEach(function (field) { // set file
                    datamap[field.attribute] = file.file;
                    field.rendererParameters["showImagePreview"] = true;
                });
            // open create form
            $timeout(function () {
                if (crudContextHolderService.isShowingModal()) {
                    crudContextHolderService.rootDataMap("#modal", datamap);
                } else {
                    $scope.edit(datamap, "New Attachment", true);
                }
                $scope.$emit("sw.attachment.file.changed", file.name);
            });
        }

        $scope.$on("file-dropzone-drop-event", onAttachmentFileDropped);

        $scope.$on("sw.attachment.file.load", onAttachmentFileLoaded);

        //#endregion

        $scope.$on(JavascriptEventConstants.CompositionEdit, function (event, applicationName, datamap, actionTitle, forceModal) {
            if (applicationName !== $scope.compositionlistschema.applicationName) {
                return;
            }
            $scope.edit(datamap, actionTitle, forceModal);
        });


        //#region edition
        $scope.edit = function (datamap, actionTitle, forceModal) {
            return $scope.toggleDetailsAfterDataFetched(false, datamap, null, actionTitle, forceModal);
        };

        $scope.hasDetailSchema = function () {
            return !!$scope.compositiondetailschema;
        }

        $scope.expansionAllowed = function (item) {
            const compositionId = item[$scope.compositionlistschema.idFieldName]; //we cannot expand an item that doesn´t have an id
            return compositionId != null;
        }


        /// <summary>
        ///  Method called when an entry of the composition is clicked
        /// </summary>
        /// <param name="item">the row entry, datamap</param>
        /// <param name="column">the specific column clicked,might be used by different implementations</param>
        $scope.toggleDetails = function (item, column, columnMode, $event, rowIndex) {
            $scope.isUpdate = columnMode === "edit";
            const initialPromise = columnMode === "edit" ? eventService.onedit_validation(item, $scope.compositionlistschema) : $q.when();
            return initialPromise.then(() => {
                $scope.doToggleDetails(item, column, columnMode, $event, rowIndex);
            });
        };

        $scope.editClick = function (item, column, $event, rowIndex) {
            if (!batchEditFunction) {
                $scope.toggleDetails(item, column, "edit", $event, rowIndex);
                return;
            }

            const originalItem = $scope.compositionData()[rowIndex];
            const originalData = $scope.detailData[originalItem.id] ? $scope.detailData[originalItem.id] : null;

            const editCallback = (editedItem) => {
                $scope.compositionData()[rowIndex] = editedItem;
                if ($scope.detailData[editedItem.id]) {
                    $scope.detailData[editedItem.id].data = formatService.doContentStringConversion(jQuery.extend(true, {}, editedItem));
                }
            }
            
            const editRollback = () => {
                $scope.compositionData()[rowIndex] = originalItem;
                if ($scope.detailData[originalItem.id]) {
                    $scope.detailData[originalItem.id].data = originalData;
                }
            }

            dispatcherService.invokeServiceByString(batchEditFunction, [angular.copy(item), editCallback, editRollback, $scope.onAfterSave, $scope.relationship]);
        }

        $scope.executeCompositionCustomClickService = (fullServiceName, column, compositionlistschema, item, clonedItem) => {
            // TODO: watch for siteid changes to recalculate the whole composition list

            if (clonedItem.hasOwnProperty("siteid") && !clonedItem["siteid"]) {
                clonedItem.siteid = $scope.parentdata["siteid"];
            }
            const shouldToggle = commandService.executeClickCustomCommand(fullServiceName, clonedItem, column, compositionlistschema);
            if (shouldToggle && $scope.hasDetailSchema()) {
                compositionListViewModel.doToggle($scope, clonedItem, item);
            }
            return $q.when();
        }


        $scope.doToggleDetails = function (item, column, columnMode, $event, rowIndex) {
            // if there is a custom list click action, do it
            const compositionlistschema = $scope.compositionlistschema;

            const customAction = compositionlistschema.properties["list.click.event"];
            if (customAction) {
                return dispatcherService.invokeServiceByString(customAction, [item]);
            }

            if (columnMode === "arrow" || columnMode === "singleselection") {
                //to avoid second call
                $event.stopImmediatePropagation();
            }
            compositionListViewModel.handleSingleSelectionClick(compositionlistschema, this.compositionData(), $scope.compositionData, item, rowIndex);
            const log = $log.get("compositionlist#toggleDetails", ["composition", "detail"]);
            log.debug("dotoggleDetails init");

            if (column != null && column.attribute == null) {
                //for sections inside compositionlist, ex: reply/replyall of commlogs
                return $q.reject();
            }

            if (($scope.isBatch() && columnMode !== "arrow")) {
                //For batch mode, as the items will be edited on the lines, 
                //we cannot allow the details to be expanded unless the button is clicked on the left side of the table.
                return $q.reject();
            }
            const compositionId = item[compositionlistschema.idFieldName];
            const updating = parseBooleanValue($scope.collectionproperties.allowUpdate);
            const fullServiceName = compositionlistschema.properties['list.click.service'];
            const clonedItem = angular.copy(item);

            if (fullServiceName != null) {
                return $scope.executeCompositionCustomClickService(fullServiceName, column, compositionlistschema, item, clonedItem);
            };

            if (!$scope.hasDetailSchema()) {
                return $q.reject("no detail schema found");
            }

            $scope.isReadOnly = !updating;

            //update header/footer layout
            $timeout(function () {
                $(window).trigger('resize');
            }, false);

            if ($scope.isBatch()) {
                //batches should always pick details locally, therefore make sure to adjust extraprojectionfields on list schema
                return compositionListViewModel.doToggle($scope, clonedItem, item);
            }
            const needServerFetching = $scope.fetchfromserver || $scope.detailData[compositionId] == undefined;
            if (!needServerFetching) {
                //opening it using already existing cached instance
                return $scope.toggleDetailsAfterDataFetched(false, $scope.detailData[compositionId].data, item);
            }

            if (!compositionId) {
                // we can´t hit the server if there´s no id.
                //this should not be happening
                log.warn("trying to fetch details on a compostion with no id selected");
                return $q.reject();
            }

            const customParams = $scope.getCustomParameters(compositionlistschema, item);

            return compositionService.getCompositionDetailItem(compositionId, $scope.compositiondetailschema, customParams).then(result => {
                const datamap = result.resultObject;
                if ($scope.isUpdate) {
                    datamap[CompositionConstants.Edited] = 1;
                }
                return $scope.toggleDetailsAfterDataFetched(true, datamap, item);
            });
        };


        $scope.toggleDetailsAfterDataFetched = function (fromServer, item, originalListItem, title, forceModal) {
            if (!shouldEditInModal() && !forceModal) {
                $scope.collapseAll(item[$scope.compositionlistschema.idFieldName]);
                compositionListViewModel.doToggle($scope, item, originalListItem);
                if (fromServer) {
                    $timeout(function () {
                        $rootScope.$broadcast(JavascriptEventConstants.BodyRendered, $element.parents('.tab-pane').attr('id'));
                    }, 0, false);
                }
                return $q.when();
            }

            // Check that main tab has all required fields filled before opening modal
            const parentDatamap = crudContextHolderService.rootDataMap();
            const parentSchema = crudContextHolderService.currentSchema();


            return validationService.validatePromise(parentSchema, parentDatamap).catch(() => {
                redirectService.redirectToTab('main');
                return $q.reject();
            }).then(() => {
                return modalService.showPromise($scope.compositiondetailschema, angular.copy(item), { title }, $scope.parentdata, $scope.parentschema);
            }).then(modaldatamap => {
                return $scope.save(modaldatamap, null);
            });
        }

        //#endregion

        $scope.delete = function (item, column, $event, rowIndex) {
            return alertService.confirm("Are you sure you want to delete this entry").then(() => {
                const compositionId = item[$scope.compositionlistschema.idFieldName];
                return compositionService.getCompositionDetailItem(compositionId, $scope.compositiondetailschema)
                    .then((result) => {
                        //TODO: generate composition deletion method
                        var compositionItem = result.resultObject;
                        return eventService.onremoval_validation(compositionItem, $scope.compositionlistschema).then(() => {
                            compositionItem[CompositionConstants.Deleted] = 1;
                            return $scope.save(compositionItem, "crud_delete");
                        });
                    });
            });



        }

        $scope.isUpdate = false;

        // TODO: decide what is better for batch
        $scope.isExpandReadOnly = function () {
            return ($scope.noupdateallowed || !$scope.isUpdate) && (!$scope.isBatch() || batchExpandReadOnly);
        }

        $scope.showEditButton = function () {
            return $scope.hasDetailSchema() && !$scope.noupdateallowed && (!$scope.isBatch() || batchEditFunction);
        }

        $scope.showDeleteButton = function () {
            return !$scope.nodeleteallowed || ($scope.isBatch() && !batchForceHideRemove);
        }

        //#region ***************Batch functions **************************************/

        $scope.addBatchItem = function () {
            const idx = $scope.compositionData().length;

            // validates the last row
            const listSchema = $scope.compositionlistschema;

            if (idx !== 0) {
                const itemMap = $scope.compositionData()[idx - 1];
                const mergedDataMap = compositionService.buildMergedDatamap(itemMap, $scope.parentdata);
                const arr = validationService.validate(listSchema, listSchema.displayables, mergedDataMap);
                if (!!arr && arr.length > 0) {
                    return;
                }
            }

            const newItem = compositionService.generateBatchItemDatamap(idx, listSchema);

            if ($scope.compositionschemadefinition.rendererParameters && $scope.compositionschemadefinition.rendererParameters["composition.inline.addfunction"]) {
                const addFunction = $scope.compositionschemadefinition.rendererParameters["composition.inline.addfunction"];

                const addCallback = (customNewtem) => {
                    addBatchItemContinue(idx, customNewtem, listSchema);
                }

                const addRollback = () => {
                    doRemoveBatchItem(idx);
                }

                dispatcherService.invokeServiceByString(addFunction, [newItem, addCallback, addRollback, $scope.onAfterSave, $scope.relationship]);
                return;
            }

            addBatchItemContinue(idx, newItem, listSchema);
        }

        function addBatchItemContinue(idx, newItem, listSchema) {
            // if inside a scroll pane - to update pane size
            fixHeaderService.callWindowResize();

            const fakeNegativeId = newItem[listSchema.idFieldName];

            $scope.compositionData().push(newItem);


            const watches = crud_inputcommons.configureAssociationChangeEvents($scope, "compositiondata[{0}]".format(idx), listSchema.displayables, fakeNegativeId);
            watches.push(watchForDirty(idx));

            $scope.unWatcherArray = $scope.unWatcherArray.concat(watches);

            // to allow watchers to be triggered by setting default values
            $timeout(function () {
                fieldService.fillDefaultValues(listSchema.displayables, newItem, $scope);
                if (idx === 0) {
                    associationService.loadSchemaAssociations(newItem, listSchema);
                }
            }, 0, false);

            //time for the components to be rendered
            $timeout(function () {
                // inits autocomplete clients if needed
                const bodyElement = $("[composition-list-key='{0}'][composition-list-id='{1}']".format($scope.getCompositionListKey(), fakeNegativeId));
                if (bodyElement.length > 0) {
                    cmpAutocompleteClient.init(bodyElement, null, listSchema);
                }

                // if inside a scroll pane - to update pane size
                fixHeaderService.callWindowResize();
                $(window).trigger('resize');
            }, 400, false);

        }

        $scope.isItemExpanded = function (item) {
            const compositionId = item[$scope.compositionlistschema.idFieldName];
            return $scope.detailData[compositionId] && $scope.detailData[compositionId].expanded;
        }


        $scope.firstItem = function (item) {
            return $scope.compositionData().indexOf(item) === 0;
        }

        function doRemoveBatchItem(rowindex) {
            const compositionData = $scope.compositionData();
            compositionData.splice(rowindex, 1);
            //since the watchers were created using the composition index, we need to regenerate them once the item is fully removed
            //otherwise the index would be out of sync with the data (ex: add 2 items, delete item[0] , add item[1] again --> there would be two watchers at item 1)
            initBatches(false);
        }

        $scope.removeBatchItem = function (rowindex) {
            const removeFunction = $scope.compositionschemadefinition.rendererParameters && $scope.compositionschemadefinition.rendererParameters["composition.inline.deletefunction"];
            if (!removeFunction) {
                doRemoveBatchItem(rowindex);
                return;
            }

            const compositionData = $scope.compositionData();
            const item = compositionData[rowindex];

            const removeCallback = () => {
                doRemoveBatchItem(rowindex);
            };

            const removeRollback = () => {
                addBatchItemContinue($scope.compositionData().length, item, $scope.compositionlistschema);
            }

            dispatcherService.invokeServiceByString(removeFunction, [item, removeCallback, removeRollback]);
        }

        //#endregion ***************END Batch functions **************************************/

        $scope.newDetailFn = function () {
            $scope.isUpdate = true;
            const datamap = {
                _iscreation: true
            };
            return $scope.edit(datamap);
        };


        $scope.cancelComposition = function () {
            $scope.newDetail = false;
            $scope.selecteditem = null;
            //                $scope.isReadonly = true;
        };

        $scope.allowButton = function (value) {
            return expressionService.evaluate(value, $scope.parentdata) && $scope.inline && !$scope.isBatch();
        };

        $scope.isBatch = function () {
            return compositionListViewModel.isBatch($scope.compositionschemadefinition);
        }


        //#region saving

        $scope.save = function (selecteditem, action) {
            if (!selecteditem) {
                //if the function is called inside a modal, then we would receive the item as a parameter, otherwise it would be on the scope of the page itself
                selecteditem = $scope.selecteditem;
            }

            if (!action) {
                action = selecteditem[CompositionConstants.IsCreation] ? "crud_create" : "crud_update";
            }

            //enforcing the dirtyness of the item
            selecteditem[CompositionConstants.IsDirty] = true;
            const relationship = $scope.relationship;

            const log = $log.getInstance("compositionlist#save", ["composition", "save", "submit"]);

            // Validation should happen before adding items to the composition list to allow invalid data to pass into the system.
            const detailSchema = $scope.compositionschemadefinition.schemas.detail;

            //ensure new item is captured as well
            safePush($scope.parentdata, relationship, selecteditem);

            if ($scope.collectionproperties.autoCommit != undefined && !$scope.collectionproperties.autoCommit) {
                log.warn('autocommit=false is yet to be implemented for compositions');
                return $q.reject();
            }

            var alwaysrefresh = schemaService.isPropertyTrue(detailSchema, 'compositions.alwaysrefresh');
            if (alwaysrefresh) {
                //this will disable success message, since we know we´ll need to refresh the screen
                contextService.insertIntoContext("refreshscreen", true, true);
            }

            log.debug("calling applicationService save with composition data");
            //calling submit on root application, informing extra composition data
            return applicationService.save({
                nextSchemaObj: { schemaId: crudContextHolderService.currentSchema().schemaId },
                refresh: alwaysrefresh,
                dispatchedByModal: false,
                compositionData: new CompositionOperation(action, relationship, selecteditem, schemaService.getId(selecteditem, $scope.compositionlistschema))
            }).then(function (data) {
                log.debug("applying composition save cbk");
                $scope.onAfterSave(data, alwaysrefresh);
            }).catch(data => {
                return $scope.onSaveError(data, selecteditem);
            });
        };

        $scope.onSaveError = function (data, selecteditem) {
            $scope.clearNewCompositionDataForBatches();

            //TODO: investigate whether this first call is really necessary...
            //$scope.compositiondata and $scope.parentdata[$scope.relationship] should be pointing to the exact same array, and angular should have been dealing with them
            //keeping here just to play safe, while there are few tests for the batches scenario
            const idx = $scope.compositiondata.indexOf(selecteditem);
            if (idx !== -1) {
                $scope.compositiondata.splice(idx, 1);
            }

            const parentidx = $scope.parentdata[$scope.relationship].indexOf(selecteditem);
            if (parentidx !== -1) {
                $scope.parentdata[$scope.relationship].splice(idx, 1);
            }
            $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
            return $q.reject(data);
        };

        $scope.onAfterSave = function (data, alwaysrefresh, forceReloadFirstPage) {
            if (alwaysrefresh) {
                window.location.href = window.location.href;
            }
            //legacy composition mode, no modals
            $scope.newDetail = false;
            $scope.selecteditem = null;
            $scope.collapseAll();

            if (forceReloadFirstPage && !$scope.paginationData) {
                $scope.paginationData = {
                    pageCount: 1,
                    pageNumber: 1,
                    pageSize: 10
                }
            }

            if (!$scope.paginationData) {
                $scope.clearNewCompositionDataForBatches();
                return $q.when(null);
            }
            const keepfilters = schemaService.isPropertyTrue($scope.parentschema, "compositions.keepfilters");
            const destinationPage = keepfilters ? $scope.paginationData.pageNumber : 1;
            if (!keepfilters) {
                $scope.searchData = {};
                $scope.searchOperator = {};
                $scope.searchSort = {};
            }

            // select first page
            return $scope.selectPage(destinationPage).then(() => {
                $scope.clearNewCompositionDataForBatches();
                crudContextHolderService.setTabRecordCount($scope.relationship, null, $scope.paginationData.totalCount);
            });
        };

        //#endregion


        $scope.clearNewCompositionDataForBatches = function () {

            if (!$scope.isBatch()) {
                return;
            }

            //removing watchers first
            if ($scope.unWatcherArray) {
                $scope.unWatcherArray.forEach(unwatcher => {
                    unwatcher();
                });
            }
            //removing dirty flag
            $scope.compositionData().forEach((item, index) => {
                delete item[CompositionConstants.IsDirty];
            });

            initBatches(false);


        };


        /*API Methods*/
        this.showExpansionCommands = function () {
            if ($scope.ismodal === "true") {
                return false;
            }

            // if schema is not present, then it should default back normal expansion commands
            if ($scope.compositionlistschema.properties != null) {
                //this is fix for GRIC-98. Don't remove it
                const isExpansible = $scope.compositionlistschema.properties.expansible;
                if (isExpansible != undefined && isExpansible == "false") {
                    return false;
                }
            }

            return $scope.compositionData().length > 1;
        }


        $scope.collapseAll = function (except) {
            Object.keys($scope.detailData).forEach(key => {
                if (except && String(key) === String(except)) {
                    return;
                }
                $scope.detailData[key].expanded = false;

            });
        }

        this.collapseAll = function () {
            $scope.collapseAll();
        };

        this.refresh = function () {
            //TODO: make a composition refresh only --> now it will be samething as F5
            window.location.href = window.location.href;
        };


        $scope.isCheckboxSelected = function (compositionitem) {
            return true === compositionitem["_#selected"];
        }

        $scope.showListCommands = function () {
            return !$scope.detail || $scope.expanded;
        };

        $scope.loadIcon = function (value, metadata) {
            return iconService.loadIcon(value, metadata);
        };


        $scope.expandAll = function () {
            return compositionListViewModel.expandAll($scope).then(() => {
                $scope.wasExpandedBefore = true;
            });
        }

        this.expandAll = function () {
            return $scope.expandAll();
        };


        $scope.shouldDisplayCommand = function (commandSchema, id) {
            return commandService.shouldDisplayCommand(commandSchema, id);
        };

        $scope.commandLabel = function (schema, id, defaultValue) {
            return commandService.commandLabel(schema, id, defaultValue);
        };

        $scope.isEnabledToExpand = function () {
            return !this.isBatch() && $scope.isReadonly && $scope.compositiondetailschema != null &&
                ($scope.compositionlistschema.properties.expansible == undefined ||
                    $scope.compositionlistschema.properties.expansible == 'true');
        };

        $scope.getCompositionListKey = function () {
            return $scope.compositionlistschema ? $scope.compositionlistschema.applicationName + "." + $scope.compositionlistschema.schemaId : "";
        }

        //overriden function
        $scope.i18NLabel = function (fieldMetadata) {
            return i18NService.getI18nLabel(fieldMetadata, $scope.compositionlistschema);
        };

        $scope.i18NInputLabel = function (fieldMetadata) {
            return i18NService.getI18nInputLabel(fieldMetadata, $scope.compositiondetailschema);
        };


        //#region pagination
        $scope.selectPage = function (pageNumber, pageSize, printMode) {
            if (pageNumber === undefined || pageNumber <= 0 || pageNumber > $scope.paginationData.pageCount) {
                $scope.paginationData.pageNumber = pageNumber;
                return $q.when();
            }
            const fields = $scope.parentdata;
            crudContextHolderService.clearCompositionsLoaded();
            const searchDTO = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, null, $scope.paginationData);
            searchDTO.pageNumber = pageNumber;
            searchDTO.pageSize = pageSize;
            return compositionService
                .searchCompositionList($scope.relationship, $scope.parentschema, fields, searchDTO)
                .then(result => $scope.refreshList(result[$scope.relationship]))
                .finally((result) => crudContextHolderService.compositionsLoaded(result));
        };

        $scope.refreshList = function (compositionData) {
            $scope.clonedCompositionData = [];

            //in order to use on onloadevent
            const previousData = $scope.compositiondata;
            $scope.compositiondata = compositionData.list;
            $scope.paginationData = compositionData.paginationData;

            $scope.init(previousData, true);
        }

        //#endregion

        //#region modal helpers
        function shouldEditInModal() {
            const openInLine = $scope.compositionlistschema.properties && "true" === $scope.compositionlistschema.properties["list.click.openinline"];
            return !openInLine && $scope.isUpdate;
        }


        //#endregion

        /**
         *  this is needed for cases where we have an inline composition hidden under a showexpression, and we don´t want it even to be initialized;
         *  metatadatadeclaration check is used for the commlog composition "subclass" implementation, whereas it doesn´t contain that property at all;
         *  Usually, the source datamap for the composition would be resolved on server side and on the response of the event it would init the composition. 
         *  However if the composition is marked as a batch composition to start with an entry, it would need to get started right away, even though it´s still hidden, 
         *  otherwise it would never be initialized 
         * 
         *  https://controltechnologysolutions.atlassian.net/browse/SWWEB-2703
         * 
         * @returns {true if we need to initialize this composition right now, false otherwise} 
         */
        const shouldEagerInit = () => {
            const isVisible = !$scope.metadatadeclaration || expressionService.evaluate($scope.metadatadeclaration.showExpression, $scope.parentdata, $scope);
            var startsWithEntry = $scope.metadatadeclaration && fieldService.isPropertyTrue($scope.metadatadeclaration.schema, "composition.inline.startwithentry");
            return isVisible || startsWithEntry;
        }


//        if (shouldEagerInit()) {
            $scope.init();
//        }

    };

    CompositionListController.$inject = ["$scope", "$q", "$log", "$timeout", "$filter", "$injector", "$http", "$element", "$rootScope", "i18NService", "tabsService", "alertService",
        "formatService", "fieldService", "commandService", "compositionService", "validationService", "dispatcherService", "cmpAutocompleteClient", "userPreferencesService", "associationService",
        "expressionService", "modalService", "redirectService", "eventService", "iconService", "cmpfacade", "crud_inputcommons", "spinService", "crudContextHolderService", "gridSelectionService",
        "schemaService", "contextService", "fixHeaderService", "applicationService", "searchService", "compositionCommons", "compositionListViewModel"];

    window.CompositionListController = CompositionListController;

    app.controller("ExtractedCompositionListController", CompositionListController);

    app.directive("compositionList", ["contextService", function (contextService) {

        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/Templates/crud/composition_list.html'),
            scope: {
                compositionschemadefinition: '=',
                compositiondata: '=',
                parentdata: '=',
                relationship: '@',
                title: '@',
                cancelfn: '&',
                previousschema: '=',
                previousdata: '=',
                parentschema: '=',
                //the composition declaration tag, of the parent schema
                metadatadeclaration: '=',
                mode: '@',
                ismodal: '@'
            },

            controller: "ExtractedCompositionListController"
        };
    }]);
})(angular, app);
