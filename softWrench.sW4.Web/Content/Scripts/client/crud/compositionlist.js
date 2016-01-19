app.directive('expandedItemOutput', function ($compile) {
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

app.directive('newItemInput', function ($compile, fieldService, associationService) {

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
                if (scope.compositionschemadefinition.schemas.list.properties.masterdetail == 'true') {
                    element.append("<composition-master-details data-cancelfn='cancel(data,schema)' data-compositiondata='compositiondata' data-compositionschemadefinition='compositionschemadefinition' data-parentdata='parentdata' parentschema='parentschema' data-relationship='{{relationship}}' data-title='{{tabLabel}}' />");
                } else {
                    element.append("<composition-list data-title='{{tabLabel}}' ismodal='{{ismodal}}'" +
                        "compositionschemadefinition='compositionschemadefinition' " +
                        "relationship='{{relationship}}' " +
                        "compositiondata='compositiondata' " +
                        "metadatadeclaration='metadata' " +
                        "parentschema='parentschema' " +
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

app.directive('compositionList', function (contextService, formatService, schemaService) {

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

        controller: function ($scope, $q, $log, $timeout, $filter, $injector, $http, $attrs, $element, $rootScope, i18NService, tabsService,
            formatService, fieldService, commandService, compositionService, validationService, dispatcherService,
            expressionService, modalService, redirectService, eventService, iconService, cmplookup, cmpfacade, crud_inputcommons, spinService, crudContextHolderService) {

            $scope.lookupObj = {};

            $scope.lookupAssociationsCode = [];
            $scope.lookupAssociationsDescriptions = [];

            $scope.setForm = function (form) {
                $scope.crudform = form;
            };



            $scope.compositionData = function () {
                if (this.isBatch() || !this.hasDetailSchema()) {
                    return $scope.compositiondata;
                }
                return $scope.clonedCompositionData;
            }

            $scope.isFieldRequired = function (item, requiredExpression) {
                return expressionService.evaluate(requiredExpression, item);
            };

            $scope.isCompositionItemFieldHidden = function (application, fieldMetadata, item) {
                var datamap = item == null ? $scope.parentdata : compositionService.buildMergedDatamap(item, $scope.parentdata);

                return fieldService.isFieldHidden(datamap, application, fieldMetadata);
            };


            $scope.initField = function (fieldMetadata, item) {
                var idx = $scope.compositionData().indexOf(item);
                crud_inputcommons.initField($scope, fieldMetadata, "compositiondata[{0}]".format(idx), idx);
            };

            function init() {
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

                $scope.detailData = {};
                $scope.clonedData = {};

                $scope.noupdateallowed = !$scope.isBatch() && !expressionService.evaluate($scope.collectionproperties.allowUpdate, $scope.parentdata);
                $scope.expanded = false;
                $scope.wasExpandedBefore = false;
                $scope.isReadonly = !expressionService.evaluate($scope.collectionproperties.allowUpdate, $scope.parentdata);


                $injector.invoke(BaseList, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService,
                    commandService: commandService,
                    formatService: formatService
                });


                var parameters = {
                    parentdata: $scope.parentdata,
                    parentschema: $scope.parentschema,
                    element: $element,
                    clonedCompositionData: $scope.compositionData()
                };
                eventService.onload($scope, $scope.compositionlistschema, $scope.parentdata.fields, parameters);
                contextService.insertIntoContext('clonedCompositionData', $scope.compositionData(), true);

                if (!$scope.paginationData) {
                    //case the tab is loaded after the event result, the event would not be present on the screen
                    $scope.paginationData = contextService.get("compositionpagination_{0}".format($scope.relationship), true, true);
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

                    $scope.compositionData().forEach(function (value, index, array) {
                        //for eventually already existing items
                        var id = schemaService.getId(value, $scope.compositionlistschema);
                        crud_inputcommons.configureAssociationChangeEvents($scope,
                            "compositiondata[{0}]".format(index), $scope.compositionlistschema.displayables, id);

                        
                        $scope.$watch("compositiondata[{0}]".format(index), function (newValue, oldValue) {
                            //make sure any change on the composition marks it as dirty
                            if (oldValue !== newValue) {
                                $scope.compositiondata[index]["#isDirty"] = true;
                            }
                        },true);


                    });

                if (fieldService.isPropertyTrue($scope.metadatadeclaration.schema, "composition.inline.startwithentry")) {
                    //inline composition should apear with an initial item
                    //add BatchItem already configure the listeners
                    $scope.addBatchItem();
                }
            }

            $scope.isNoRecords = $scope.compositiondata.length <= 0;



            $scope.showPagination = !$scope.isNoRecords && // has items to show
                                    !!$scope.paginationData && // has paginationdata
                                    $scope.paginationData.paginationOptions.some(function (option) { // totalCount is bigger than at least one option
                                        return $scope.paginationData.totalCount > option;
                                    });
        };

    $scope.getApplicationPah = function (datamap, fieldMetadata) {
        var path = fieldMetadata.applicationPath + schemaService.getId(datamap, $scope.compositionlistschema);
        return replaceAll(path, "\\.", "_");
    }

    $scope.$on('sw_compositiondataresolved', function (event, compositiondata) {
        if (!compositiondata[$scope.relationship]) {
            //this is not the data this tab is interested
            return;
        }
        spinService.stop({ compositionSpin: true });
        $scope.paginationData = compositiondata[$scope.relationship].paginationData;
        $scope.compositiondata = compositiondata[$scope.relationship].list;
        init();
        $scope.$digest();
    });

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
        return $scope.i18N($scope.relationship + '.add', 'Add ' + $scope.title);
    }

    this.newDetailFn = function () {
        $scope.edit({});
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




    $scope.$on("sw.composition.edit", function (event, datamap) {
        $scope.edit(datamap);
    });

    $scope.edit = function (datamap) {
        if ($scope.compositionlistschema.properties && "modal" == $scope.compositionlistschema.properties["list.click.popup"]) {
            modalService.show($scope.compositiondetailschema, datamap, {}, $scope.save);
        } else {
            //TODO: switch to edit
            $scope.newDetail = true;
            $scope.selecteditem = datamap;
        }
        $scope.collapseAll();
    }

    var doToggle = function (id, item, originalListItem, forcedState) {
        if ($scope.clonedData[id] == undefined) {
            $scope.clonedData[id] = {};
            $scope.clonedData[id].data = formatService.doContentStringConversion(jQuery.extend(true, {}, item));
        }

        if ($scope.detailData[id] == undefined) {
            $scope.detailData[id] = {};
            $scope.detailData[id].expanded = false;
            $scope.detailData[id].data = formatService.doContentStringConversion(item);
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
        return $scope.compositiondetailschema != null;
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

        if ((this.isBatch() && columnMode !== "arrow")) {
            //For batch mode, as the items will be edited on the lines, 
            //we cannot allow the details to be expanded unless the button is clicked on the left side of the table.
            return;
        }


        var compositionId = item[$scope.compositionlistschema.idFieldName];

        var updating = $scope.collectionproperties.allowUpdate;


        var fullServiceName = $scope.compositionlistschema.properties['list.click.service'];
        if (fullServiceName != null) {
            var compositionschema = $scope.compositionschemadefinition['schemas']['detail'];
            var shouldToggle = commandService.executeClickCustomCommand(fullServiceName, item, column, $scope.compositionlistschema);
            if (shouldToggle && this.hasDetailSchema()) {
                doToggle(compositionId, item, item);
            }
            return;
        };

        if (!this.hasDetailSchema()) {
            return;
        }

        $scope.isReadOnly = !updating;

        // Need to disable all other rich text box for viewable real estate
        //                $scope.collapseAll();


        var needServerFetching = $scope.fetchfromserver && $scope.detailData[compositionId] == undefined;
        if (!needServerFetching || this.isBatch()) {
            //batches should always pick details locally, therefore make sure to adjust extraprojectionfields on list schema
            doToggle(compositionId, item, item);
            return;
        }

        if (!compositionId) {
            // we can´t hit the server if there´s no id.
            //this should not be happening
            log.warn("trying to fetch details on a compostion with no id selected");
            return;
        }

        var compositiondetailschema = $scope.compositiondetailschema;
        var applicationName = compositiondetailschema.applicationName;
        var parameters = {};
        var request = {};
        var key = {};
        parameters.request = request;
        request.id = compositionId;
        request.key = key;
        key.schemaId = compositiondetailschema.schemaId;
        key.mode = compositiondetailschema.mode;
        key.platform = "web";
        var urlToCall = url("/api/data/" + applicationName + "?" + $.param(parameters));
        $http.get(urlToCall).success(
            function (result) {
                doToggle(compositionId, result.resultObject.fields, item);
                //this timeout is here to avoid a strange exception exception: digest alredy in place
                //TODO: investigate further
                $timeout(function () {
                    $rootScope.$broadcast('sw_bodyrenderedevent', $element.parents('.tab-pane').attr('id'));
                }, 0, false);
            });

    };


    /***************Batch functions **************************************/

    $scope.addBatchItem = function () {


        var idx = $scope.compositionData().length;
        if (idx !== 0) {
            var itemMap = $scope.compositionData()[idx - 1];
            var mergedDataMap = compositionService.buildMergedDatamap(itemMap, $scope.parentdata);
            var arr = validationService.validate($scope.compositionlistschema, $scope.compositionlistschema.displayables, mergedDataMap);
            if (arr != null && arr.length != 0) {
                return;
            }
        }
        var newItem = {
            //used to make a differentiation between a compositionitem datamap and a regular datamap
            '#datamaptype': "compositionitem",
            '#datamapidx': idx

        };
        fieldService.fillDefaultValues($scope.compositionlistschema.displayables, newItem, $scope);
        //this id will be placed on the entity so that angular can use it to track. 
        //It has to be negative to indicate its not a maximo Id, and also a unique value to avoid collisions
        var fakeNegativeId = -Date.now().getTime();
        newItem[$scope.compositionlistschema.idFieldName] = fakeNegativeId;
        $scope.compositionData().push(newItem);
        crud_inputcommons.configureAssociationChangeEvents($scope, "compositiondata[{0}]".format(idx), $scope.compositionlistschema.displayables, fakeNegativeId);
        $scope.$watch("compositiondata[{0}]".format(idx), function (newValue, oldValue) {
            //make sure any change on the composition marks it as dirty
            if (oldValue !== newValue) {
                $scope.compositiondata[idx]["#isDirty"] = true;
            }
        }, true);
    }

    $scope.isItemExpanded = function (item) {
        var compositionId = item[$scope.compositionlistschema.idFieldName];
        return $scope.detailData[compositionId] && $scope.detailData[compositionId].expanded;
    }


    $scope.firstItem = function (item) {
        return $scope.compositionData().indexOf(item) == 0;
    }

    $scope.removeBatchItem = function (rowindex) {
        $scope.compositionData().splice(rowindex, 1);
    }

    /***************END Batch functions **************************************/

    $scope.newDetailFn = function () {
        $scope.edit({});
    };



    $scope.cancelComposition = function () {
        $scope.newDetail = false;
        $scope.selecteditem = null;
        //                $scope.isReadonly = true;
    };

    $scope.allowButton = function (value) {
        return expressionService.evaluate(value, $scope.parentdata) && $scope.inline && !this.isBatch();
    };

    $scope.isBatch = function () {
        return "batch" == $scope.compositionschemadefinition.rendererParameters["mode"];
    }

    $scope.isSingleSelection = function () {
        return "single" == schemaService.getProperty($scope.compositionlistschema, "list.selectionstyle")
    }

    $scope.hasModified = function (key, displayables) {
        for (var index = 0; index < displayables.length; index++) {
            var attribute = displayables[index].attribute;

            if ($scope.clonedData[key] == null) {
                return false;
            }
            else if ($scope.clonedData[key].data[attribute] != $scope.detailData[key].data[attribute]) {
                return true;
            }
        }

        return false;
    };

    $scope.save = function (selecteditem) {
        if (!selecteditem) {
            //if the function is called inside a modal, then we would receive the item as a parameter, otherwise it would be on the scope of the page itself
            selecteditem = $scope.selecteditem;
        }

        if (selecteditem == undefined && !$scope.collectionproperties.allowUpdate) {
            //this is for the call to submit without having any item on composition selected, due to having the submit as the default button
            $log.getInstance("compositionlist#save").debug("calling save on server without composition selected");
            //this flag must be true for the spin to work properly... TODO: improve this
            $scope.$parent.$parent.save(null, { isComposition: true });
            return;
        }

        // Validation should happen before adding items to the composition list to allow invalid data to pass into the system.
        var detailSchema = $scope.compositionschemadefinition.schemas.detail;
        if (selecteditem != undefined) {
            var validationErrors = validationService.validate(detailSchema, detailSchema.displayables, selecteditem, $scope.crudform.$error);
            if (validationErrors.length > 0) {
                //interrupting here, can´t be done inside service
                return;
            }
        }

        var updatedCompositionData = [];
        if (this.hasDetailSchema() && $scope.collectionproperties.allowUpdate === "true") {
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
        if (this.hasDetailSchema() && $scope.collectionproperties.allowUpdate) {
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
        //TODO: refactor this, using promises
        $scope.$emit("sw_submitdata", {
            successCbk: function (data) {
                var updatedArray = data.resultObject != null ? data.resultObject.fields[$scope.relationship] : null;
                if (alwaysrefresh || updatedArray == null || updatedArray.length === 0) {
                    window.location.href = window.location.href;
                    return;
                }
                //we need to clone it again here, to avoid binding, otherwise the data would be shown in the list before submission confirms on server side
                $scope.clonedCompositionData = JSON.parse(JSON.stringify(updatedArray));
                $scope.clonedData = {};
                $scope.compositiondata = updatedArray;
                $scope.newDetail = false;
                $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
                $scope.selecteditem = {};
                $scope.collapseAll();
                if ($rootScope.showingModal) {
                    //hides the modal after submiting it
                    modalService.hide();
                }
                $scope.selecteditem = null;
            },
            failureCbk: function (data) {
                var idx = $scope.compositiondata.indexOf(selecteditem);
                if (idx != -1) {
                    $scope.compositiondata.splice(idx, 1);
                }
                $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
            },
            dispatcherComposition: $scope.relationship,
            isComposition: true,
            nextSchemaObj: { schemaId: $scope.$parent.$parent.schema.schemaId },
            refresh: alwaysrefresh,
        });
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
                doToggle(itemId, value, compositionListData[itemId], true);
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

    //overriden function
    $scope.i18NLabel = function (fieldMetadata) {
        return i18NService.getI18nLabel(fieldMetadata, $scope.compositionlistschema);
    };

    /* pagination */

    $scope.selectPage = function (pageNumber, pageSize, printMode) {
        if (pageNumber === undefined || pageNumber <= 0 || pageNumber > $scope.paginationData.pageCount) {
            $scope.paginationData.pageNumber = pageNumber;
            return;
        }
        compositionService
            .getCompositionList($scope.relationship, $scope.parentschema, $scope.parentdata.fields, pageNumber, $scope.paginationData.pageSize)
            .then(function (result) {
                $scope.clonedCompositionData = [];
                // clear lists
                $scope.compositiondata = result[$scope.relationship].list;
                $scope.paginationData = result[$scope.relationship].paginationData;
                init();
            });
    };

    /* end pagination */

    init();
}
};
});
