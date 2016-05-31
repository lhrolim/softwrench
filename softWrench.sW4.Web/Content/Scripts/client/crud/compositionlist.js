(function (angular, app) {
    "use strict";

    app.directive('expandedItemOutput', function ($compile) {
        "ngInject";

        return {
            restrict: "E",
            replace: true,
            scope: {
                displayables: '=',
                schema: '=',
                datamap: '=',
                cancelfn: '&'
            },
            template: "<div></div>",
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.displayables)) {
                    element.append(
                        "<crud-output schema='schema'" +
                        "datamap='datamap'" +
                        "displayables='displayables'" +
                        "cancelfn='cancelfn()'></crud-output>"
                    );
                    $compile(element.contents())(scope);
                }
            }
        }
    });

    app.directive('expandedItemInput', function ($compile) {
        "ngInject";

        return {
            restrict: "E",
            replace: true,
            scope: {
                displayables: '=',
                schema: '=',
                datamap: '=',
                savefn: '&',
                cancelfn: '&'
            },
            template: "<div></div>",
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.displayables)) {
                    element.append(
                        "<crud-input schema='schema'" +
                        "datamap='datamap'" +
                        "displayables='displayables'" +
                        "savefn='savefn()'" +
                        "cancelfn='cancelfn()'></crud-input>"
                    );
                    $compile(element.contents())(scope);
                }
            }
        }
    });

    app.directive('newItemInput', function ($compile, fieldService) {
        "ngInject";

        return {
            restrict: "E",
            replace: true,
            scope: {
                displayables: '=',
                elementid: '=',
                schema: '=',
                datamap: '=',
                cancelfn: '&',
                savefn: '&',
                parentdata: '=',
                parentschema: '=',
                previousdata: '=',
                previousschema: '='

            },
            template: "<div></div>",
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.displayables)) {
                    fieldService.fillDefaultValues(scope.displayables, scope.datamap, scope);
                    element.append(
                        "<crud-input schema='schema' " +
                        "datamap='datamap' " +
                        "displayables='displayables' " +
                        "elementid='crudInputNewItemComposition' " +
                        "association-schemas='associationSchemas' " +
                        "blockedassociations='blockedassociations' " +
                        "parentdata='parentdata' " +
                        "parentschema='parentschema' " +
                        "previousdata='previousdata' " +
                        "previouschema='previousschema' " +
                        "savefn='savefn()' " +
                        "cancelfn='cancelfn()' " +
                        "composition='true'></crud-input>"
                    );
                    $compile(element.contents())(scope);
                }
            }
        }
    });

    app.directive('compositionListWrapper', function ($compile, i18NService, $log, compositionService, spinService) {
        "ngInject";

        return {
            restrict: 'E',
            replace: true,
            template: "<div></div>",
            scope: {
                metadata: '=',
                parentschema: '=',
                parentdata: '=',
                cancelfn: '&',
                previousschema: '=',
                previousdata: '=',
                mode: '@',
                inline: '@',
                ismodal: '@',
                tabid: '@'
            },
            link: function (scope, element, attrs) {

                scope.$name = 'compositionlistwrapper';

                var doLoad = function () {
                    $log.getInstance('compositionlistwrapper#doLoad').debug('loading composition {0}'.format(scope.tabid));
                    var metadata = scope.metadata;
                    scope.tabLabel = i18NService.get18nValue(metadata.schema.schemas.list.applicationName + '._title', metadata.label);
                    if (scope.parentdata.fields) {
                        scope.compositiondata = scope.parentdata.fields[scope.metadata.relationship];
                    } else {
                        scope.compositiondata = scope.parentdata[scope.metadata.relationship];
                    }
                    if (!scope.compositiondata) {
                        var arr = [];
                        scope.parentdata[scope.metadata.relationship] = arr;

                        //a blank array if nothing exists, scenario for selfcompositions
                        scope.compositiondata = arr;

                    }

                    scope.compositionschemadefinition = metadata.schema;
                    scope.relationship = metadata.relationship;

                    //display the list composition by default
                    if (scope.compositionschemadefinition.schemas.list.properties.masterdetail === "true") {
                        element.append(
                            "<composition-master-details data-cancelfn='cancel(data,schema)' " +
                            "data-compositiondata='compositiondata' data-compositionschemadefinition='compositionschemadefinition' " +
                            "data-parentdata='parentdata' parentschema='parentschema' " +
                            "mode='{{mode}}' " +
                            "data-relationship='{{relationship}}' data-title='{{tabLabel}}' />");
                    } else {
                        element.append("<composition-list data-title='{{tabLabel}}' ismodal='{{ismodal}}'" +
                            "compositionschemadefinition='compositionschemadefinition' " +
                            "relationship='{{relationship}}' " +
                            "compositiondata='compositiondata' " +
                            "metadatadeclaration='metadata' " +
                            "parentschema='parentschema' " +
                            "mode='{{mode}}' " +
                            "parentdata='parentdata' " +
                            "cancelfn='cancel(data,schema)' " +
                            "previousschema='previousschema' " +
                            "previousdata='previousdata' />");
                    }

                    $compile(element.contents())(scope);

                    //controls tab lazy loading
                    scope.loaded = true;
                }

                var custom = scope.metadata.schema.renderer.rendererType == 'custom';
                var isInline = scope.metadata.inline;

                if (scope.metadata.type == "ApplicationCompositionDefinition" && isInline && !custom) {
                    //inline compositions should load automatically
                    doLoad();
                 
                }

                scope.cancel = function (data, schema) {
                    scope.cancelfn({ data: data, schema: schema });
                }

                scope.$on("sw.modal.show", function (event, modalData) {
                    if (scope.ismodal === "true") {
                        //inline compositions inside of the modal need to be refreshed (relinked)
                        var datamap = modalData.datamap;
                        scope.parentdata = datamap;
                        doLoad();
                    }
                });

                scope.$on("sw.modal.hide", function (event) {
                    if (scope.ismodal === "true") {
                        $log.get('compositionlistwrapper#doLoad', ["composition", "inline"]).debug('wiping <composition-list> directive due to modal disposal');
                        //inline compositions inside of the modal need to be refreshed (relinked)
                        element.empty();
                    }
                });

                scope.$on("sw_lazyloadtab", function (event, tabid) {
                    if (scope.tabid == tabid) {
                        if (!compositionService.isCompositionLodaded(scope.tabid)) {
                            spinService.start({ compositionSpin: true });
                        }
                        if (!scope.loaded) {
                            doLoad();
                        }

                    }

                });

            }
        }
    });

    function CompositionListController($scope, $q, $log, $timeout, $filter, $injector, $http, $attrs, $element, $rootScope, i18NService, tabsService, alertService,
        formatService, fieldService, commandService, compositionService, validationService, dispatcherService, cmpAutocompleteClient, userPreferencesService, associationService,
        expressionService, modalService, redirectService, eventService, iconService, cmplookup, cmpfacade, crud_inputcommons, spinService, crudContextHolderService, gridSelectionService,
        schemaService, contextService, fixHeaderService, applicationService, searchService) {

        $scope.lookupObj = {};

        $scope.lookupAssociationsCode = [];
        $scope.lookupAssociationsDescriptions = [];

        $scope.searchData = {};
        $scope.searchOperator = {};
        $scope.searchSort = {};
        $scope.filtersDatamap = {};
        $scope.panelId = null;
        $scope.selectAllChecked = false;

        $scope.filterForColumn = function (column) {
            var schema = $scope.compositionlistschema;
            if (!schema || !schema.schemaFilters || !schema.schemaFilters.filters) {
                return null;
            }
            return schema.schemaFilters.filters.find(function (filter) {
                return filter.attribute === column.attribute;
            });
        }

        $scope.filterApplied = function () {
            $scope.paginationData.pageNumber = 1;
            var searchDTO = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, null, $scope.paginationData);
            compositionService.searchCompositionList($scope.relationship, $scope.parentschema, $scope.parentdata.fields || $scope.parentdata, searchDTO).then(function (result) {
                $scope.refreshList(result[$scope.relationship]);
            }).finally(function () {
                crudContextHolderService.setDetailDataResolved();
            });
        };

        $scope.shouldShowSort = function (column, orientation) {
            if ($scope.hideFilter()) {
                return false;
            }
            return !!column.attribute && ($scope.searchSort.field === column.attribute || $scope.searchSort.field === column.rendererParameters["sortattribute"]) && $scope.searchSort.order === orientation;
        };

        $scope.sort = function (column) {
            var columnName = column.attribute;

            var sorting = $scope.searchSort;
            if (sorting.field != null && sorting.field === columnName) {
                sorting.order = sorting.order === "desc" ? "asc" : "desc";
            } else {
                sorting.field = columnName;
                sorting.order = "asc";
            }
            $scope.filterApplied();
        };

        $scope.sortLabel = function (column) {
            return $scope.i18N("_grid.filter.clicksort", "{0}, Click to sort".format(column.label));
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

        $scope.setForm = function (form) {
            $scope.crudform = form;
        };


        $scope.showTableHover = function () {
            return ($scope.compositionlistschema.properties['list.click.event'] != undefined || $scope.compositiondetailschema != undefined) && $scope.compositiondata.length > 0;
        };

        $scope.clearCompositionData = function () {
            var arr = $scope.compositionData();
            var previousCompositionData = $scope.compositionData();
            arr.splice(0, arr.length);
            $scope.paginationData = null;
            var parameters = {
                parentdata: $scope.parentdata,
                parentschema: $scope.parentschema,
                relationship: $scope.relationship,
                clonedCompositionData: arr,
                previousData: previousCompositionData
            };
            eventService.onload($scope, $scope.compositionlistschema, $scope.parentdata.fields, parameters);
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
            return compositionitem[$scope.compositionlistschema.idFieldName] > 0 || ($scope.compositionData().length === 1 && isCompositionRequired());
        }

        $scope.isCompositionItemFieldHidden = function (application, fieldMetadata, item) {
            var datamap = item == null ? $scope.parentdata : compositionService.buildMergedDatamap(item, $scope.parentdata);

            return fieldService.isFieldHidden(datamap, application, fieldMetadata);
        };


        $scope.initField = function (fieldMetadata, item) {
            var idx = $scope.compositionData().indexOf(item);
            crud_inputcommons.initField($scope, fieldMetadata, "compositiondata[{0}]".format(idx), idx);
        };

        function watchForDirty(index) {
            return $scope.$watch("compositiondata[{0}]".format(index), function (newValue, oldValue) {
                //make sure any change on the composition marks it as dirty
                if (oldValue !== newValue && $scope.compositiondata[index]) {
                    $scope.compositiondata[index]["#isDirty"] = true;
                }
            }, true);
        }

        function init(previousCompositionData, isPaginationRefresh) {
            if (!$scope.compositionschemadefinition.schemas) {
                //this means that we recevived only the list schema, for inline compositions
                $scope.compositionschemadefinition.schemas = {};
                $scope.compositionschemadefinition.schemas.list = $scope.compositionschemadefinition;
                $scope.compositionschemadefinition.inline = true;
                $scope.compositionschemadefinition.collectionProperties = {
                    allowInsertion: "false",
                    allowUpdate: "false",
                }
            }

            //Extra variables
            $scope.compositionlistschema = $scope.compositionschemadefinition.schemas.list;
            $scope.compositiondetailschema = $scope.compositionschemadefinition.schemas.detail;
            $scope.fetchfromserver = $scope.compositionschemadefinition.fetchFromServer;
            $scope.collectionproperties = $scope.compositionschemadefinition.collectionProperties;
            $scope.inline = $scope.compositionschemadefinition.inline;

            $scope.detailData = $scope.clonedData = {};

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

            var parameters = {
                parentdata: $scope.parentdata,
                parentschema: $scope.parentschema,
                previousData: previousCompositionData,
                element: $element,
                relationship: $scope.relationship,
                clonedCompositionData: $scope.compositionData(),
                paginationApplied: isPaginationRefresh
            };
            eventService.onload($scope, $scope.compositionlistschema, $scope.parentdata.fields, parameters);


            if (!$scope.paginationData) {
                //case the tab is loaded after the event result, the event would not be present on the screen
                $scope.paginationData = contextService.get("compositionpagination_{0}".format($scope.relationship), true, true);
            }

            if ($scope.paginationData) {
                userPreferencesService.syncSchemaPreference($scope.paginationData, "pageSize", "compositionPageSize", $scope.parentschema.applicationName, $scope.parentschema.schemaId);
            }

            if (!$scope.isBatch()) {
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
            } else {

                $scope.unWatcherArray = [];

                $scope.compositionData().forEach(function (value, index, array) {
                    //for eventually already existing items
                    var id = schemaService.getId(value, $scope.compositionlistschema);
                    var watches = crud_inputcommons.configureAssociationChangeEvents($scope,
                        "compositiondata[{0}]".format(index), $scope.compositionlistschema.displayables, id);
                    watches.push(watchForDirty(index));

                    $scope.unWatcherArray = watches;


                });

                if (fieldService.isPropertyTrue($scope.metadatadeclaration.schema, "composition.inline.startwithentry")) {
                    //inline composition should apear with an initial item
                    //add BatchItem already configure the listeners
                    $scope.addBatchItem();
                }
            }

            var eventData = compositionService.pollCompositionEvent($scope.relationship);
            if (eventData) {
                $scope.onAfterCompositionResolved(null, eventData);
            }

        }

        $scope.getApplicationPath = function (datamap, fieldMetadata) {
            var path = fieldMetadata.applicationPath + schemaService.getId(datamap, $scope.compositionlistschema);
            return replaceAll(path, "\\.", "_");
        }

        $scope.isNoRecords = function () {
            return $scope.compositiondata.length <= 0;
        }

        $scope.showPagination = function () {
            return !$scope.isNoRecords() && // has items to show
                !!$scope.paginationData &&
                $scope.paginationData.paginationOptions.some(function (option) {
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

            var log = $log.get("compositionlist#resolved", ["composition"]);

            spinService.stop({ compositionSpin: true });
            var thisCompData = compositiondata[$scope.relationship];
            if (thisCompData == null) {
                log.debug("cleaning up composition data (but keeping same array)");
                $scope.clearCompositionData();
                return;
            }
            var list = thisCompData.list || thisCompData.resultList;

            log.debug("composition data refreshed for {0} | entries: {1}".format($scope.relationship, list.length));

            $scope.paginationData = thisCompData.paginationData;
            $scope.compositiondata = list;
            $scope.parentdata[$scope.relationship] = list;

            init();


            try {
                $scope.$digest();
            } catch (e) {
                //digest already in progress...
            }

        };

        $scope.$on("sw_compositiondataresolved", $scope.onAfterCompositionResolved);

        $scope.getBooleanClass = function (compositionitem, attribute) {
            if (compositionitem[attribute] == "true" || compositionitem[attribute] == 1) {
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
                var idFieldName = compositionlistschema.idFieldName;
                return compositionitem[idFieldName] != null;
            }
            return false;
        }

        $scope.invokeCustomCheckBoxService = function (fieldMetadata, datamap, $event) {
            if (fieldMetadata.rendererParameters["clickservice"] == null) {
                return;
            }
            var customfn = dispatcherService.loadServiceByString(fieldMetadata.rendererParameters["clickservice"]);
            $q.when(customfn(fieldMetadata, $scope.parentdata, datamap));
            $event.stopImmediatePropagation();
        }


        $scope.isModifiableEnabled = function (fieldMetadata, item) {
            var result = expressionService.evaluate(fieldMetadata.enableExpression, compositionService.buildMergedDatamap(item, $scope.parentdata), $scope);
            return result;
        };


        $scope.compositionProvider = function () {
            var localCommands = {};
            var toAdd = [];
            localCommands.toAdd = toAdd;
            var log = $log.getInstance('composition_service#compositionProvider');

            localCommands.toKeep = compositionService.getListCommandsToKeep($scope.compositionschemadefinition);

            if (!expressionService.evaluate($scope.collectionproperties.allowInsertion, $scope.parentdata) || $scope.inline) {
                log.debug('local commands without add. {0} '.format(JSON.stringify(localCommands)));
                return localCommands;
            }

            var addCommand = {};
            addCommand.label = $scope.i18N($scope.relationship + '.add', 'Add ' + $scope.title);
            addCommand.method = $scope.newDetailFn;
            addCommand.defaultPosition = 'left';
            var iconCompoistionAddbutton = $scope.compositionschemadefinition.schemas.list.properties['icon.composition.addbutton'];
            if (!iconCompoistionAddbutton) {
                //use the same as the tab by default
                iconCompoistionAddbutton = $scope.compositionschemadefinition.schemas.list.properties['icon.composition.tab'];
            }
            addCommand.icon = iconCompoistionAddbutton;
            toAdd.push(addCommand);
            log.debug('local commands: {0} '.format(JSON.stringify(localCommands)));
            return localCommands;
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
            var labelOverride = $scope.compositiondetailschema.properties['add.button.label'];

            if (labelOverride) {
                return $scope.i18N($scope.relationship + '.add', labelOverride);
            } else {
                return $scope.i18N($scope.relationship + '.add', 'Add ' + $scope.title);
            }
        }

        this.newDetailFn = function () {
            $scope.isUpdate = true;
            var datamap = {
                _iscreation: true
            }
            $scope.edit(datamap);
        };

        $scope.draggableValue = function () {
            return $scope.relationship.contains("attachment") ? "true" : "false";
        };

        this.save = function () {
            return $scope.save();
        }

        this.cancel = function () {
            $('#crudmodal').modal('hide');
            if (GetPopUpMode() == 'browser') {
                close();
            }

            $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
            $scope.$emit('sw_cancelclicked');
        };

        //#region attachments

        function onAttachmentFileDropped(event, file) {
            // remove preview element if a new file is added
            getFileUploadFields().forEach(function (field) {
                field.rendererParameters["showImagePreview"] = false;
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
                var evt = (event.originalEvent || event);
                var src = evt.srcElement;
                var item = angular.element(src).scope().compositionitem;
                if (!item) return;

                // add extension to fileName if it does't have
                var fileName = item["document"];
                if (fileName.lastIndexOf(".") < 0) {
                    var fileUrl = item["docinfo_.urlname"];
                    var extension = fileUrl.substring(fileUrl.lastIndexOf(".") + 1);
                    fileName += ("." + extension);


                    // download = '<mime_type>:<file_name_on_save>:<file_download_url>'
                    var downloadData = "application/octet-stream:" + fileName + ":" + item["download_url"];
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
                    var renderer = field.renderer.rendererType;
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

        $scope.$on("sw.composition.edit", function (event, datamap, actionTitle, forceModal) {
            $scope.edit(datamap, actionTitle, forceModal);
        });

        $scope.edit = function (datamap, actionTitle, forceModal) {
            if (!!forceModal || shouldEditInModal()) {
                // Check that main tab has all required fields filled before opening modal
                var parentDatamap = crudContextHolderService.rootDataMap();
                var parentSchema = crudContextHolderService.currentSchema();
                if (validationService.validate(parentSchema, parentSchema.displayables, parentDatamap.fields).length > 0) {
                    //crudContextHolderService.setActiveTab(null);
                    redirectService.redirectToTab('main');
                    return;
                }

                modalService.show($scope.compositiondetailschema, datamap, { title: actionTitle }, function saveCallBack(datamap) {

                    $scope.save(datamap);
                }, null, $scope.parentdata, $scope.parentschema);

            } else {
                //TODO: switch to edit
                $scope.newDetail = true;
                $scope.selecteditem = datamap;
                $timeout(function () {
                    $(".no-touch [rel=tooltip]").tooltip({ container: "body", trigger: "hover" });
                    $(".no-touch [rel=tooltip]").tooltip("hide");
                });
            }
            $scope.collapseAll();
        };

        $scope.doToggle = function (id, item, originalListItem, forcedState) {

            if ($scope.clonedData[id] == undefined) {
                $scope.clonedData[id] = {};
                $scope.clonedData[id].data = formatService.doContentStringConversion(jQuery.extend(true, {}, item));
            }

            if ($scope.detailData[id] == undefined) {
                $scope.detailData[id] = {};
                $scope.detailData[id].expanded = false;
                $scope.detailData[id].data = formatService.doContentStringConversion(item);
                $scope.detailData[id].data["_iscreation"] = id == null || id < 0;
            }

            var newState = forcedState != undefined ? forcedState : !$scope.detailData[id].expanded;
            $scope.detailData[id].expanded = newState;

            if (newState) {
                var parameters = {};
                parameters.compositionItemId = id;
                parameters.compositionItemData = originalListItem;
                parameters.parentData = $scope.parentdata;
                parameters.parentSchema = $scope.parentschema;

                var compositionSchema = $scope.parentschema.cachedCompositions[$scope.relationship];
                eventService.onviewdetail(compositionSchema, parameters);
            }
        };


        $scope.hasDetailSchema = function () {
            return !!$scope.compositiondetailschema;
        }

        $scope.expansionAllowed = function (item) {
            var compositionId = item[$scope.compositionlistschema.idFieldName];
            //we cannot expand an item that doesn´t have an id
            return compositionId != null;
        }

        $scope.handleSingleSelectionClick = function (item, rowIndex) {
            if (!this.isSingleSelection()) {
                return;
            }
            var items = this.compositionData();
            var previousValue = item["#selected"];
            for (var i = 0; i < items.length; i++) {
                items[i]["#selected"] = "false";
                $scope.compositiondata[i]["#selected"] = "false";
            }
            if (previousValue == undefined || "false" == previousValue) {
                item["#selected"] = "true";
                //updating the original item, to make it possible to send custom action selection to server-side
                $scope.compositiondata[rowIndex]["#selected"] = "true";
            }

        }


        /// <summary>
        ///  Method called when an entry of the composition is clicked
        /// </summary>
        /// <param name="item">the row entry, datamap</param>
        /// <param name="column">the specific column clicked,might be used by different implementations</param>
        $scope.toggleDetails = function (item, column, columnMode, $event, rowIndex) {

            $scope.isUpdate = columnMode === "edit";

            var event = $scope.compositionlistschema.events["onedit.validation"];
            if ($scope.isUpdate && event) {
                var fn = dispatcherService.loadService(event.service, event.method);
                fn(item, $scope.compositionlistschema).then(function () {
                    $scope.executeToggleDetails(item, column, columnMode, $event, rowIndex);
                });
            } else {
                $scope.executeToggleDetails(item, column, columnMode, $event, rowIndex);
            }


        };
        $scope.executeToggleDetails = function (item, column, columnMode, $event, rowIndex) {
            // if there is a custom list click action, do it
            var customAction = $scope.compositionlistschema.properties["list.click.event"];
            if (customAction) {
                dispatcherService.invokeServiceByString(customAction, [item]);
                return;
            }

            if (columnMode === "arrow" || columnMode === "singleselection") {
                //to avoid second call
                $event.stopImmediatePropagation();
            }
            this.handleSingleSelectionClick(item, rowIndex);

            var log = $log.get("compositionlist#toggleDetails");

            if (column != null && column.attribute == null) {
                //for sections inside compositionlist, ex: reply/replyall of commlogs
                return;
            }

            if (($scope.isBatch() && columnMode !== "arrow")) {
                //For batch mode, as the items will be edited on the lines, 
                //we cannot allow the details to be expanded unless the button is clicked on the left side of the table.
                return;
            }

            var compositionId = item[$scope.compositionlistschema.idFieldName];


            var updating = parseBooleanValue($scope.collectionproperties.allowUpdate);

            var fullServiceName = $scope.compositionlistschema.properties['list.click.service'];
            var clonedItem = angular.copy(item);
            if (fullServiceName != null) {
                var compositionschema = $scope.compositionschemadefinition['schemas']['detail'];
                // TODO: watch for siteid changes to recalculate the whole composition list

                if (clonedItem.hasOwnProperty("siteid") && !clonedItem["siteid"]) {
                    clonedItem.siteid = $scope.parentdata.fields["siteid"];
                }

                var shouldToggle = commandService.executeClickCustomCommand(fullServiceName, clonedItem, column, $scope.compositionlistschema);
                if (shouldToggle && $scope.hasDetailSchema()) {
                    doToggle(compositionId, clonedItem, item);
                }
                return;
            };

            if (!$scope.hasDetailSchema()) {
                return;
            }

            $scope.isReadOnly = !updating;

            // Need to disable all other rich text box for viewable real estate
            //                $scope.collapseAll();

            //update header/footer layout
            $timeout(function () {
                $(window).trigger('resize');
            }, false);

            if ($scope.isBatch()) {
                //batches should always pick details locally, therefore make sure to adjust extraprojectionfields on list schema
                $scope.doToggle(compositionId, clonedItem, item);
                return;
            }

            var needServerFetching = $scope.fetchfromserver || $scope.detailData[compositionId] == undefined;
            if (!needServerFetching) {
                //opening it using already existing cached instance
                $scope.innerToggleDetails(false, compositionId, $scope.detailData[compositionId].data, item);
                return;
            }

            if (!compositionId) {
                // we can´t hit the server if there´s no id.
                //this should not be happening
                log.warn("trying to fetch details on a compostion with no id selected");
                return;
            }

            compositionService.getCompositionDetailItem(compositionId, $scope.compositiondetailschema).then(function (result) {
                var datamap = result.resultObject.fields;
                if ($scope.isUpdate) {
                    datamap["#edited"] = 1;
                }
                $scope.innerToggleDetails(true, compositionId, datamap, item);
            });
        };

        $scope.innerToggleDetails = function (fromServer, id, item, originalListItem) {
            if (!shouldEditInModal()) {
                $scope.doToggle(id, item, originalListItem);
                if (fromServer) {
                    $timeout(function () {
                        $rootScope.$broadcast('sw_bodyrenderedevent', $element.parents('.tab-pane').attr('id'));
                    }, 0, false);
                }
                return;
            }

            // Check that main tab has all required fields filled before opening modal
            var parentDatamap = crudContextHolderService.rootDataMap();
            var parentSchema = crudContextHolderService.currentSchema();
            if (validationService.validate(parentSchema, parentSchema.displayables, parentDatamap.fields).length > 0) {
                //crudContextHolderService.setActiveTab(null);
                redirectService.redirectToTab('main');
                return;
            }
            modalService.show($scope.compositiondetailschema, angular.copy(item), {}, $scope.save, null, $scope.parentdata, $scope.parentschema);
        }

        $scope.delete = function (item, column, $event, rowIndex) {
            alertService.confirm2("Are you sure you want to delete this entry").then(function () {
                var compositionId = item[$scope.compositionlistschema.idFieldName];

                compositionService.getCompositionDetailItem(compositionId, $scope.compositiondetailschema).then(function (result) {
                    //TODO: generate composition deletion method
                    var compositionItem = result.resultObject.fields;
                    var event = $scope.compositionlistschema.events["onremoval.validation"];
                    if (event) {
                        var fn = dispatcherService.loadService(event.service, event.method);
                        fn(compositionItem, $scope.compositionlistschema).then(function () {
                            compositionItem["#deleted"] = 1;
                            $scope.save(compositionItem, null, "crud_delete");
                        });
                    } else {
                        compositionItem["#deleted"] = 1;
                        $scope.save(compositionItem, null, "crud_delete");
                    }
                });
            });



        }

        $scope.isUpdate = false;

        // TODO: decide what is better for batch
        $scope.isExpandReadOnly = function () {
            return ($scope.noupdateallowed || !$scope.isUpdate) && !$scope.isBatch();
        }

        $scope.showEditButton = function () {
            return $scope.hasDetailSchema() && !$scope.noupdateallowed && !$scope.isBatch();
        }

        $scope.showDeleteButton = function () {
            return !$scope.nodeleteallowed;
        }

        /***************Batch functions **************************************/

        $scope.addBatchItem = function () {


            var idx = $scope.compositionData().length;

            // validates the last row
            if (idx !== 0) {
                var itemMap = $scope.compositionData()[idx - 1];
                var mergedDataMap = compositionService.buildMergedDatamap(itemMap, $scope.parentdata);
                var arr = validationService.validate($scope.compositionlistschema, $scope.compositionlistschema.displayables, mergedDataMap);
                if (!!arr && arr.length > 0) {
                    return;
                }
            }

            var newItem = {
                //used to make a differentiation between a compositionitem datamap and a regular datamap
                '#datamaptype': "compositionitem",
                '#datamapidx': idx
            }

            // if inside a scroll pane - to update pane size
            fixHeaderService.callWindowResize();


            //this id will be placed on the entity so that angular can use it to track. 
            //It has to be negative to indicate its not a maximo Id, and also a unique value to avoid collisions
            var fakeNegativeId = -Date.now().getTime();
            newItem[$scope.compositionlistschema.idFieldName] = fakeNegativeId;
            $scope.compositionData().push(newItem);
            var watches = crud_inputcommons.configureAssociationChangeEvents($scope, "compositiondata[{0}]".format(idx), $scope.compositionlistschema.displayables, fakeNegativeId);
            watches.push(watchForDirty(idx));

            $scope.unWatcherArray = $scope.unWatcherArray.concat(watches);

            // to allow watchers to be triggered by setting default values
            $timeout(function () {
                fieldService.fillDefaultValues($scope.compositionlistschema.displayables, newItem, $scope);
                if (idx === 0) {
                    associationService.loadSchemaAssociations(newItem, $scope.compositionlistschema);
                }
            }, 0, false);

            //time for the components to be rendered
            $timeout(function () {
                // inits autocomplete clients if needed
                var bodyElement = $("[composition-list-key='{0}'][composition-list-id='{1}']".format($scope.getCompositionListKey(), fakeNegativeId));
                if (bodyElement.length > 0) {
                    cmpAutocompleteClient.init(bodyElement, null, $scope.compositionlistschema);
                }

                // if inside a scroll pane - to update pane size
                fixHeaderService.callWindowResize();
                $(window).trigger('resize');
            }, 400, false);

        }

        $scope.isItemExpanded = function (item) {
            var compositionId = item[$scope.compositionlistschema.idFieldName];
            return $scope.detailData[compositionId] && $scope.detailData[compositionId].expanded;
        }


        $scope.firstItem = function (item) {
            return $scope.compositionData().indexOf(item) == 0;
        }

        $scope.removeBatchItem = function (rowindex) {
            var compositionData = $scope.compositionData();
            var item = compositionData[rowindex];
            var id = item[$scope.compositionlistschema.idFieldName];

            //since the watchers were created using the composition index, we need to regenerate them once the item is fully removed
            //otherwise the index would be out of sync with the data (ex: add 2 items, delete item[0] , add item[1] again --> there would be two watchers at item 1)
            $scope.unWatcherArray.forEach(function (unwatcher) {
                unwatcher();
            });

            $scope.unWatcherArray = [];

            compositionData.splice(rowindex, 1);

            //readding the necessary watches
            $scope.compositionData().forEach(function (value, index, array) {
                //for eventually already existing items
                var itemId = schemaService.getId(value, $scope.compositionlistschema);
                var watches = crud_inputcommons.configureAssociationChangeEvents($scope,
                    "compositiondata[{0}]".format(index), $scope.compositionlistschema.displayables, itemId);
                watches.push(watchForDirty(index));

                $scope.unWatcherArray = watches;
            });

        }

        /***************END Batch functions **************************************/

        $scope.newDetailFn = function () {
            $scope.isUpdate = true;
            var datamap = {
                _iscreation: true
            }
            $scope.edit(datamap);
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
            return "batch" === $scope.compositionschemadefinition.rendererParameters["mode"];
        }

        $scope.isSingleSelection = function () {
            return "single" === schemaService.getProperty($scope.compositionlistschema, "list.selectionstyle");
        }

        $scope.hasModified = function (key, displayables) {
            for (var index = 0; index < displayables.length; index++) {
                var attribute = displayables[index].attribute;

                if ($scope.clonedData[key] == null) {
                    return false;
                } else if ($scope.clonedData[key].data[attribute] == $scope.detailData[key].data[attribute]) {
                    return true;
                }
            }

            return false;
        };

        $scope.save = function (selecteditem, schema, action) {
            if (!selecteditem) {
                //if the function is called inside a modal, then we would receive the item as a parameter, otherwise it would be on the scope of the page itself
                selecteditem = $scope.selecteditem;
            }

            //enforcing the dirtyness of the item
            selecteditem["#isDirty"] = true;

            if (selecteditem == undefined && !$scope.collectionproperties.allowUpdate) {
                //this is for the call to submit without having any item on composition selected, due to having the submit as the default button
                $log.getInstance("compositionlist#save").debug("calling save on server without composition selected");
                //this flag must be true for the spin to work properly... TODO: improve this
                $scope.$parent.$parent.save(null, { isComposition: true });
                return;
            }

            // Validation should happen before adding items to the composition list to allow invalid data to pass into the system.
            var detailSchema = $scope.compositionschemadefinition.schemas.detail;
            var validationErrors;
            if (selecteditem != undefined && selecteditem["#deleted"] !== 1) {
                var crudFormCtrl = getModalCrudFormController();
                validationErrors = validationService.validate(detailSchema, detailSchema.displayables, selecteditem, crudFormCtrl.$error);
                if (validationErrors.length > 0) {
                    //interrupting here, can´t be done inside service
                    return;
                }
            }

            var updatedCompositionData = [];
            if ($scope.hasDetailSchema() && $scope.collectionproperties.allowUpdate === "true") {
                for (var key in $scope.detailData) {
                    if ($scope.detailData.hasOwnProperty(key) && $scope.hasModified(key, detailSchema.displayables)) {
                        updatedCompositionData.push($scope.detailData[key].data);

                        validationErrors = validationService.validate(detailSchema, detailSchema.displayables, $scope.detailData[key].data);
                        if (validationErrors.length > 0) {
                            //interrupting here, can´t be done inside service
                            return false;
                        }
                    }
                }
            }

            //parentdata is bound to the datamap --> this is needed so that the sw_submitdata has the updated data
            if ($scope.hasDetailSchema() && $scope.collectionproperties.allowUpdate) {
                //if composition items are editable, then we should pass the entire composition list back.  One or more item could have been changed.
                //$scope.parentdata.fields[$scope.relationship] = $scope.clonedCompositionData;
                $scope.parentdata.fields[$scope.relationship] = updatedCompositionData;
            }

            if (selecteditem != undefined) {
                //ensure new item is captured as well
                safePush($scope.parentdata.fields, $scope.relationship, selecteditem);
            }

            var log = $log.getInstance('compositionlist#save');
            if (!$scope.collectionproperties.autoCommit) {
                log.warn('autocommit=false is yet to be implemented for compositions');
                return;
            }

            var alwaysrefresh = schemaService.isPropertyTrue($scope.compositiondetailschema, 'compositions.alwaysrefresh');
            if (alwaysrefresh) {
                //this will disable success message, since we know we´ll need to refresh the screen
                contextService.insertIntoContext("refreshscreen", true, true);
            }
            if (!action) {
                action = selecteditem["_iscreation"] ? "crud_create" : "crud_update";
            }

            applicationService.save({
                dispatcherComposition: $scope.relationship,
                nextSchemaObj: { schemaId: crudContextHolderService.currentSchema().schemaId },
                refresh: alwaysrefresh,
                dispatchedByModal: false,
                compositionData: {
                    operation: action,
                    dispatcherComposition: $scope.relationship,
                    id: schemaService.getId(selecteditem, $scope.compositionlistschema)
                }
            }).then(function (data) {
                $scope.onAfterSave(data, alwaysrefresh);
            }).catch(function (data) {
                $scope.onSaveError(data, selecteditem);
            });

            //            //TODO: refactor this, using promises
            //            $scope.$emit("sw_submitdata", {
            //                successCbk: afterSaveListener("onAfterSave", alwaysrefresh),
            //                failureCbk: afterSaveListener("onSaveError", selecteditem),
            //                dispatcherComposition: $scope.relationship,
            //                isComposition: true,
            //                nextSchemaObj: { schemaId: crudContextHolderService.currentSchema().schemaId },
            //                refresh: alwaysrefresh
            //            });
        };

        function afterSaveListener(name, extra) {
            return function (data) {
                $scope[name](data, extra);
            }
        };

        $scope.onSaveError = function (data, selecteditem) {
            $scope.clearNewCompositionData();
            var idx = $scope.compositiondata.indexOf(selecteditem);
            if (idx !== -1) {
                $scope.compositiondata.splice(idx, 1);
            }
            $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
        };

        $scope.onAfterSave = function (data, alwaysrefresh) {
            if (alwaysrefresh) {
                window.location.href = window.location.href;
            }
            // dispose of the edit/creation form
            if ($rootScope.showingModal) {
                 modalService.hide();
            }
            $scope.newDetail = false;
            $scope.selecteditem = null;
            $scope.collapseAll();

            if (!$scope.paginationData) {
                $scope.clearNewCompositionData();
                return $q.when(null);
            }

            var keepfilters = schemaService.isPropertyTrue($scope.parentschema, "compositions.keepfilters");
            var destinationPage = keepfilters ? $scope.paginationData.pageNumber : 1;
            if (!keepfilters) {
                $scope.searchData = {};
                $scope.searchOperator = {};
                $scope.searchSort = {};
            }

            // select first page
            return $scope.selectPage(destinationPage).then(function (result) {
                $scope.clearNewCompositionData();
                crudContextHolderService.setTabRecordCount($scope.relationship, null, $scope.paginationData.totalCount);
            });
        };


        $scope.clearNewCompositionData = function () {
            $scope.compositionData().forEach(function (item) {
                delete item["#isDirty"];
            });
            //TODO: there´s a bug in potential here, after we´re adding an item to a composition the parentdata is getting inconsistent
            //but it´s rather on the save fn
            $scope.parentdata.fields[$scope.relationship] = [];
        };


        /*API Methods*/
        this.showExpansionCommands = function () {
            if ($scope.ismodal === "true") {
                return false;
            }

            // if schema is not present, then it should default back normal expansion commands
            if ($scope.compositionlistschema.properties != null) {
                //this is fix for GRIC-98. Don't remove it
                var isExpansible = $scope.compositionlistschema.properties.expansible;
                if (isExpansible != undefined && isExpansible == "false") {
                    return false;
                }
            }

            return $scope.noupdateallowed && $scope.compositionData().length > 1;
        }


        $scope.collapseAll = function () {
            $.each($scope.detailData, function (key, value) {
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

        function buildExpandAllParams() {
            var params = {};
            params.key = {};
            params.options = {};
            params.application = $scope.parentschema.applicationName;
            params.detailRequest = {};
            var key = {};
            params.detailRequest.key = key;

            var parentSchema = $scope.parentschema;
            params.detailRequest.id = fieldService.getId($scope.parentdata, parentSchema);
            key.schemaId = parentSchema.schemaId;
            key.mode = parentSchema.mode;
            key.platform = platform();

            var compositionsToExpand = {};
            compositionsToExpand[$scope.relationship] = { schema: $scope.compositionlistschema, value: true };
            //                var compositionsToExpand = { 'worklog_': true };

            params.options.compositionsToExpand = tabsService.buildCompositionsToExpand(compositionsToExpand, parentSchema,
                $scope.parentdata, $scope.compositiondetailschema.schemaId);
            params.options.printMode = true;
            return params;
        }

        this.expandAll = function () {
            if ($scope.wasExpandedBefore) {
                $.each($scope.detailData, function (key, value) {
                    $scope.detailData[key].expanded = true;
                });
                return;
            }

            var compositionListData = [];
            for (var i = 0; i < $scope.compositiondata.length; i++) {
                var data = $scope.compositiondata[i];
                var id = data[$scope.compositiondetailschema.idFieldName];
                compositionListData[id] = data;
            }

            var urlToInvoke = removeEncoding(url("/api/generic/Composition/ExpandCompositions?" + $.param(buildExpandAllParams())));
            $http.get(urlToInvoke).success(function (result) {
                $.each(result.resultObject[$scope.relationship], function (key, value) {
                    //TODO: This function is not utilizing the needServerFetching optimization as found in the toggleDetails function
                    var itemId = value[$scope.compositiondetailschema.idFieldName];
                    $scope.doToggle(itemId, value, compositionListData[itemId], true);
                });
                $scope.wasExpandedBefore = true;
            });
        };


        $scope.shouldDisplayCommand = function (commandSchema, id) {
            return commandService.shouldDisplayCommand(commandSchema, id);
        };

        $scope.commandLabel = function (schema, id, defaultValue) {
            return commandService.commandLabel(schema, id, defaultValue);
        };

        $scope.isEnabledToExpand = function () {
            return $scope.isReadonly && $scope.compositiondetailschema != null &&
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

        //#region command interception
        (function (self) {
            $scope.$on("sw:command:scope", function ($event, method) {
                var selectedTab = crudContextHolderService.getActiveTab();
                if (selectedTab === $scope.relationship) {
                    self[method]();
                }
            });
        })(this);
        //#endregion

        //#region pagination
        $scope.selectPage = function (pageNumber, pageSize, printMode) {
            if (pageNumber === undefined || pageNumber <= 0 || pageNumber > $scope.paginationData.pageCount) {
                $scope.paginationData.pageNumber = pageNumber;
                return $q.when();
            }

            var fields = $scope.parentdata.fields || $scope.parentdata;
            crudContextHolderService.clearDetailDataResolved();

            var searchDTO = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, null, $scope.paginationData);
            searchDTO.pageNumber = pageNumber;
            searchDTO.pageSize = pageSize;
            return compositionService
                .searchCompositionList($scope.relationship, $scope.parentschema, fields, searchDTO)
                .then(function (result) {
                    $scope.refreshList(result[$scope.relationship]);
                }).finally(function () {
                    crudContextHolderService.setDetailDataResolved();
                });
        };

        $scope.refreshList = function (compositionData) {
            $scope.clonedCompositionData = [];

            //in order to use on onloadevent
            var previousData = $scope.compositiondata;

            $scope.compositiondata = compositionData.list;
            $scope.paginationData = compositionData.paginationData;

            init(previousData, true);
        }

        //#endregion

        //#region modal helpers
        function shouldEditInModal() {
            var openInLine = $scope.compositionlistschema.properties && "true" === $scope.compositionlistschema.properties["list.click.openinline"];
            return !openInLine && $scope.isUpdate;
        }

        function getModalCrudFormController() {
            var crudModalForm = document.querySelector("#crudmodal").querySelector("#crudbodyform");
            return !crudModalForm ? null : angular.element(crudModalForm).scope()["crudbodyform"];
        }

        //#endregion
        if (!$scope.metadatadeclaration || expressionService.evaluate($scope.metadatadeclaration.showExpression, $scope.parentdata, $scope)) {
            //this is needed for cases where we have an inline composition hidden under a showexpression, and we don´t want it even to be initialized
            init();
        }

    };

    CompositionListController.$inject = ["$scope", "$q", "$log", "$timeout", "$filter", "$injector", "$http", "$attrs", "$element", "$rootScope", "i18NService", "tabsService", "alertService",
            "formatService", "fieldService", "commandService", "compositionService", "validationService", "dispatcherService", "cmpAutocompleteClient", "userPreferencesService", "associationService",
            "expressionService", "modalService", "redirectService", "eventService", "iconService", "cmplookup", "cmpfacade", "crud_inputcommons", "spinService", "crudContextHolderService", "gridSelectionService",
            "schemaService", "contextService", "fixHeaderService", "applicationService", "searchService"];

    window.CompositionListController = CompositionListController;

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

            controller: CompositionListController
        };
    }]);
})(angular, app);
