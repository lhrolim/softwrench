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
            associationOptions: '=',
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
                                "associationOptions='associationOptions'" +
                                "savefn='savefn()'" +
                                "cancelfn='cancelfn()'></crud-input>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('newItemInput', function ($compile, fieldService) {

    return {
        restrict: "E",
        replace: true,
        scope: {
            displayables: '=',
            elementid: '=',
            schema: '=',
            datamap: '=',
            associationOptions: '=',
            cancelfn: '&',
            savefn: '&'

        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {
                fieldService.fillDefaultValues(scope.displayables, scope.datamap);
                element.append(
                    "<crud-input schema='schema'" +
                                "datamap='datamap'" +
                                "displayables='displayables'" +
                                "elementid='elementid'" +
                                "associationOptions='associationOptions'" +
                                "savefn='savefn()'" +
                                "cancelfn='cancelfn()'></crud-input>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('compositionList', function (contextService) {

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
            parentschema: '='
        },

        controller: function ($scope, $log, $filter, $injector, $http, $element, $rootScope, i18NService, tabsService,
            formatService, fieldService, commandService, compositionService, validationService,
            expressionService, $timeout, modalService, redirectService, eventdispatcherService) {
            

            function init() {
                //Extra variables
                $scope.compositionlistschema = $scope.compositionschemadefinition.schemas.list;
                $scope.compositiondetailschema = $scope.compositionschemadefinition.schemas.detail;
                $scope.fetchfromserver = $scope.compositionschemadefinition.fetchFromServer;
                $scope.collectionproperties = $scope.compositionschemadefinition.collectionProperties;
                $scope.inline = $scope.compositionschemadefinition.inline;
                //


                $scope.clonedCompositionData = [];
                jQuery.extend($scope.clonedCompositionData, $scope.compositiondata);
                $scope.isNoRecords = $scope.clonedCompositionData.length > 0 ? false : true;
                $scope.detailData = {};
                $scope.noupdateallowed = !expressionService.evaluate($scope.collectionproperties.allowUpdate, $scope.parentdata);
                $scope.expanded = false;
                $scope.wasExpandedBefore = false;
                $scope.isReadonly = !expressionService.evaluate($scope.collectionproperties.allowUpdate, $scope.parentdata);

                $injector.invoke(BaseController, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService,
                    commandService: commandService
                });
            };

            init();
          

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


            $scope.newDetailFn = function () {
                $scope.edit({});
            };

            $scope.$on("sw.composition.edit", function (event, datamap) {
                $scope.edit(datamap);
            });


            $scope.edit = function (datamap) {
                if ($scope.compositionlistschema.properties && "modal" == $scope.compositionlistschema.properties["list.click.popup"]) {
                    modalService.show($scope.compositiondetailschema, datamap, $scope.save);
                } else {
                    //TODO: switch to edit
                    $scope.newDetail = true;
                }
                $scope.selecteditem = datamap;
                $scope.collapseAll();
            }

            var doToggle = function (id, item, originalListItem, forcedState) {
                if ($scope.detailData[id] == undefined) {
                    $scope.detailData[id] = {};
                    $scope.detailData[id].expanded = false;
                }
                $scope.detailData[id].data = item;
                var newState = forcedState != undefined ? forcedState : !$scope.detailData[id].expanded;
                $scope.detailData[id].expanded = newState;

                if(newState) {
                    var parameters = {};
                    parameters.compositionItemId = id;
                    parameters.compositionItemData = originalListItem;
                    parameters.parentData = $scope.parentdata;
                    parameters.parentSchema = $scope.parentschema;

                    var compositionSchema = $scope.parentschema.cachedCompositions[$scope.relationship];
                    eventdispatcherService.onviewdetail(compositionSchema, parameters);
                }
            };
            /// <summary>
            ///  Method called when an entry of the composition is clicked
            /// </summary>
            /// <param name="item">the row entry, datamap</param>
            /// <param name="column">the specific column clicked,might be used by different implementations</param>
            $scope.toggleDetails = function (item, column) {
                var updating = $scope.collectionproperties.allowUpdate;

                var fullServiceName = $scope.compositionlistschema.properties['list.click.service'];
                if (fullServiceName != null) {
                    commandService.executeClickCustomCommand(fullServiceName, item, column);
                    return;
                };

                $scope.isReadOnly = !updating;
                var compositionId = item[$scope.compositionlistschema.idFieldName];
                var needServerFetching = $scope.fetchfromserver && $scope.detailData[compositionId] == undefined;
                if (!needServerFetching) {
                    doToggle(compositionId, item, item);
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
                        $timeout(function() {
                            $rootScope.$broadcast('sw_bodyrenderedevent', $element.parents('.tab-pane').attr('id'));
                        }, 0, false);
                    });
            };

            $scope.cancelComposition = function () {
                $scope.newDetail = false;
                //                $scope.isReadonly = true;
            };

            $scope.cancel = function (previousData, previousSchema) {
                $('#crudmodal').modal('hide');
                if (GetPopUpMode() == 'browser') {
                    close();
                }
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            $scope.allowButton = function (value) {
                return expressionService.evaluate(value, $scope.parentdata) && $scope.inline;
            };


            $scope.save = function (selecteditem) {
                if (!selecteditem) {
                    //if the function is called inside a modal, then we would receive the item as a parameter, otherwise it would be on the scope of the page itself
                    selecteditem = $scope.selecteditem;
                }
                if (selecteditem == undefined) {
                    //this is for the call to submit without having any item on composition selected, due to having the submit as the default button
                    $log.getInstance("compositionlist#save").debug("calling save on server without composition selected");
                    //this flag must be true for the spin to work properly... TODO: improve this
                    $scope.$parent.save(null, { isComposition: true });
                    return;
                }

                //todo:update
                if ($scope.compositiondata == null) {
                    $scope.compositiondata = [];
                }
                $scope.compositiondata.push(selecteditem);
                if ($scope.collectionproperties.autoCommit) {
                    var validationErrors = validationService.validate($scope.compositionschemadefinition.schemas.detail.displayables, selecteditem);
                    if (validationErrors.length > 0) {
                        //interrupting here, can´t be done inside service
                        return;
                    }
                    $scope.$parent.save(null, {
                        successCbk: function (data) {
                            var updatedArray = data.resultObject.fields[$scope.relationship];
                            var alwaysrefresh = $scope.compositiondetailschema.properties && "true" == $scope.compositiondetailschema.properties['compositions.alwaysrefresh'];

                            if (alwaysrefresh || updatedArray == null || updatedArray.length == 0) {
                                var compositions = window.location.href.split(/[\s#]+/);
                                var compositiontabaftersave = '#' + compositions[compositions.length - 1].replace('/', '');
                                sessionStorage.compositiontabaftersave = compositiontabaftersave;
                                window.location.reload();
                                return;
                            }
                            $scope.clonedCompositionData = updatedArray;
                            $scope.compositiondata = updatedArray;
                            $scope.newDetail = false;
                            $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
                            $scope.selecteditem = {};
                            collapseAll();
                            if ($rootScope.showingModal) {
                                //hides the modal after submiting it
                                modalService.hide();
                            }
                        },
                        failureCbk: function (data) {
                            var idx = $scope.compositiondata.indexOf(selecteditem);
                            if (idx != -1) {
                                $scope.compositiondata.splice(idx, 1);
                            }
                            $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
                        },
                        isComposition: true,
                        nextSchemaObj: { schemaId: $scope.$parent.schema.schemaId }
                    });
                }

            };



            /*API Methods*/
            this.showExpansionCommands = function () {
                return $scope.noupdateallowed && $scope.clonedCompositionData.length > 1;
            }


            $scope.collapseAll=function() {
                $.each($scope.detailData, function (key, value) {
                    $scope.detailData[key].expanded = false;
                });
            }

            this.collapseAll = function () {
                $scope.collapseAll();
            };

            this.refresh = function () {
                //TODO: make a composition refresh only --> now it will be samething as F5
                window.location.reload();
            };
           
            

            $scope.showListCommands = function () {
                return !$scope.detail || $scope.expanded;
            };

            $scope.loadIcon = function (value, metadata) {
                var expression = metadata.rendererParameters['expression'];
                if (expression != null) {
                    expression = replaceAll(expression, '\'', "\"");
                    try {
                        var expressionObj = JSON.parse(expression);
                        var result = expressionObj[value];
                        if (result == null) {
                            //switch case deafult
                            return expressionObj["#default"];
                        }
                        return result;
                    } catch (e) {
                        $log.getInstance('compositionlist#loadicon').warn('invalid expression definition {0}'.format(expression));
                    }
                }
                var iconvalue = metadata.rendererParameters['value'];
                if (iconvalue != null) {
                    return iconvalue;
                }
                //forgot to declare it, just return
                return '';
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

                var urlToInvoke = removeEncoding(url("/api/generic/ExtendedData/ExpandCompositions?" + $.param(buildExpandAllParams())));
                $http.get(urlToInvoke).success(function (result) {
                    $.each(result.resultObject[$scope.relationship], function (key, value) {
                        //TODO: This function is not utilizing the needServerFetching optimization as found in the toggleDetails function
                        var itemId = value[$scope.compositiondetailschema.idFieldName];
                        doToggle(itemId, value, compositionListData[itemId], true);
                    });
                    $scope.wasExpandedBefore = true;
                });
            };

            $scope.getFormattedValue = function (value, column) {
                return formatService.format(value, column);
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


        }
    };
});