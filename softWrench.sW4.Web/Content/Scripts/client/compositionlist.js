(function (app, angular, $) {
    "use strict";



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

app.directive('newItemInput', function ($compile) {

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

                $.each(scope.displayables, function (key, value) {
                    var target = value.attribute;
                    if (value.defaultValue != undefined && target != undefined) {
                        if (scope.datamap[target] == null) {
                            //TODO: extract a service here, to be able to use @user, @person, @date, etc...
                            scope.datamap[target] = value.defaultValue;
                        }
                    }
                });

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

app.directive('compositionListWrapper', function ($compile, i18NService, $log, $rootScope, spinService, compositionService) {
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
            tabid: '@'
        },
        link: function (scope, element, attrs) {

            var doLoad = function () {
                $log.getInstance('compositionlistwrapper#doLoad').debug('loading composition {0}'.format(scope.tabid));
                var metadata = scope.metadata;
                scope.tabLabel = i18NService.get18nValue(metadata.schema.schemas.list.applicationName + '._title', metadata.label);
                if (scope.parentdata.fields) {
                    scope.compositiondata = scope.parentdata.fields[scope.metadata.relationship];
                } else {
                    scope.compositiondata = scope.parentdata[scope.metadata.relationship];
                }
                scope.compositionschemadefinition = metadata.schema;
                scope.relationship = metadata.relationship;
                element.append("<composition-list title='{{tabLabel}}'" +
                    "compositionschemadefinition='compositionschemadefinition'" +
                    "relationship='{{relationship}}'" +
                    "compositiondata='compositiondata'" +
                    "parentschema='parentschema'" +
                    "parentdata='parentdata'" +
                    "cancelfn='toListSchema(data,schema)'" +
                    "previousschema='previousschema' previousdata='previousdata'/>");
                $compile(element.contents())(scope);
                scope.loaded = true;
            }

            var custom = scope.metadata.schema.renderer.rendererType == 'custom';
            var isInline = scope.metadata.inline;

            if (scope.metadata.type == "ApplicationCompositionDefinition" && isInline && !custom) {
                doLoad();
            }

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid != tabid) {
                    //not this tab
                    return;
                }
                if (!compositionService.isCompositionLodaded(scope.tabid)) {
                    spinService.start({ compositionSpin: true });
                }
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

        }



    }
});


app.directive('compositionList', function (contextService, spinService) {

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

        controller: ["$scope", "$log", "$filter", "$injector", "$http", "$element", "$rootScope", "i18NService", "tabsService",
            "formatService", "fieldService", "commandService", "compositionService", "validationService", "expressionService", "$timeout",
            function ($scope, $log, $filter, $injector, $http, $element, $rootScope, i18NService, tabsService,
                formatService, fieldService, commandService, compositionService, validationService, expressionService, $timeout) {

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

                if (!$scope.paginationData) {
                    //case the tab is loaded after the event result, the event would not be present on the screen
                    $scope.paginationData = contextService.get("compositionpagination_{0}".format($scope.relationship), true, true);
                    //workaround for hapag
                    if ($scope.paginationData) {
                        $scope.paginationData.selectedPage = $scope.paginationData.pageNumber;
                    }
                }

                $scope.showPagination = !$scope.isNoRecords && // has items to show
                                        !!$scope.paginationData && // has paginationdata
                                        $scope.paginationData.paginationOptions.some(function (option) { // totalCount is bigger than at least one option
                                            return $scope.paginationData.totalCount > option;
                                        });

                // scroll to top on ajax errors
                $scope.$on("sw_ajaxerror", function() {
                    $(document.body).animate({ scrollTop: 0 });
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
                toAdd.push(addCommand);
                log.debug('local commands: {0} '.format(JSON.stringify(localCommands)));
                return localCommands;
            };


            $scope.newDetailFn = function ($event) {
                $scope.newDetail = true;
                $scope.selecteditem = {};
                $scope.collapseAll();
                // scroll to detail form (500ms for smoothness)
                $timeout(function () {
                    var scrollTarget = $(".js_compositionnewitem");
                    if (!scrollTarget[0]) scrollTarget = $($event.delegateTarget);
                    $(document.body).animate({ scrollTop: scrollTarget.offset().top });
                }, 500, false);
            };

            var doToggle = function (id, item, forcedState) {
                if ($scope.detailData[id] == undefined) {
                    $scope.detailData[id] = {};
                    $scope.detailData[id].expanded = false;
                }
                $scope.detailData[id].data = item;
                var newState = forcedState != undefined ? forcedState : !$scope.detailData[id].expanded;
                $scope.detailData[id].expanded = newState;
            };

            $scope.toggleDetails = function (item, updating) {
                contextService.insertIntoContext("sw:crudbody:scrolltop", false);
                
                var fullServiceName = $scope.compositionlistschema.properties['list.click.service'];
                if (fullServiceName != null) {
                    commandService.executeClickCustomCommand(fullServiceName, item, $scope.compositionlistschema.displayables);
                    return;
                };

                $scope.isReadOnly = !updating;
                var compositionId = item[$scope.compositionlistschema.idFieldName];
                if (!$scope.fetchfromserver) {
                    doToggle(compositionId, item);
                    return;
                } else if ($scope.detailData[compositionId] != undefined) {
                    doToggle(compositionId, $scope.detailData[compositionId].data);
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
                $http.get(urlToCall)
                    .then( function (response) {
                        var result = response.data;
                        doToggle(compositionId, result.resultObject.fields);
                        $rootScope.$broadcast('sw_bodyrenderedevent', $element.parents('.tab-pane').attr('id'));
                    })
                    .then(function () {
                    });
            };

            $scope.getGridColumnStyle = function (column, propertyName) {
                var property = column.rendererParameters[propertyName];

                if (property != null) {
                    return property;
                }

                if (propertyName === 'maxwidth') {
                    var high = $(window).width() > 1199;
                    if (high) {
                        return '135px';
                    }
                    return '100px';
                }
                return null;
            }

            $scope.$on('sw_compositiondataresolved', function (event, compositiondata) {
                if (!compositiondata[$scope.relationship]) {
                    //this is not the data this tab is interested
                    return;
                }
                spinService.stop({ compositionSpin: true });
                $scope.paginationData = compositiondata[$scope.relationship].paginationData;
                if (!!$scope.paginationData) {
                    $scope.paginationData.selectedPage = $scope.paginationData.pageNumber;
                }
                $scope.compositiondata = compositiondata[$scope.relationship].list;
                
                init();
                if (!$scope.$$phase && !$scope.$root.$$phase) {
                    $scope.$digest();
                }
            });

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

            $scope.refresh = function () {
                //TODO: make a composition refresh only --> now it will be samething as F5
                window.location.href = window.location.href;
            };

            $scope.allowButton = function (value) {
                return expressionService.evaluate(value, $scope.parentdata) && $scope.inline;
            };

            $scope.save = function () {
                var selecteditem = $scope.selecteditem;
                //todo:update
                if ($scope.compositiondata == null) {
                    $scope.compositiondata = [];
                }

                var validationErrors = validationService.validate($scope.compositionschemadefinition.schemas.detail, $scope.compositionschemadefinition.schemas.detail.displayables, selecteditem);
                if (validationErrors.length > 0) {
                    //interrupting here, can´t be done inside service
                    return;
                }

                $scope.compositiondata.push(selecteditem);

                if (!$scope.collectionproperties.autoCommit) {
                    return;
                }

                var alwaysrefresh = $scope.compositiondetailschema.properties && "true" == $scope.compositiondetailschema.properties['compositions.alwaysrefresh'];
                if (alwaysrefresh) {
                    //this will disable success message, since we know we´ll need to refresh the screen
                    contextService.insertIntoContext("refreshscreen", true, true);
                }
                $scope.$parent.$parent.save(null, {
                    successCbk: function (data) {

                        if (alwaysrefresh) {
                            window.location.href = window.location.href;
                            return;
                        }
                        var updatedArray = data.resultObject.fields[$scope.relationship];
                        if (updatedArray == null || updatedArray.length == 0) {
                            window.location.href = window.location.href;
                            return;
                        }
                        $scope.clonedCompositionData = updatedArray;
                        $scope.compositiondata = updatedArray;
                        $scope.newDetail = false;
                        $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
                        $scope.selecteditem = {};
                        $scope.collapseAll();

                        var relName = $scope.relationship;
                        var eventData = {};

                        eventData[relName] = {
                            list: data.resultObject.fields[relName],
                            relationship: relName
                        };

                        $scope.$emit("sw_compositiondataresolved", eventData);
                    },
                    failureCbk: function (data) {
                        var idx = $scope.compositiondata.indexOf(selecteditem);
                        if (idx != -1) {
                            $scope.compositiondata.splice(idx, 1);
                        }
                        $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
                    },
                    isComposition: true,
                    nextSchemaObj: { schemaId: $scope.$parent.$parent.schema.schemaId },
                    refresh: alwaysrefresh
                });
            };

            $scope.collapseAll = function () {
                $.each($scope.detailData, function (key, value) {
                    $scope.detailData[key].expanded = false;
                });
            };

            $scope.showListCommands = function () {
                return !$scope.detail || $scope.expanded;
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
                    $scope.parentdata, $scope.compositiondetailschema.schemaId, [], false);
                params.options.printMode = true;
                return params;
            }

            $scope.expandAll = function () {
                if ($scope.wasExpandedBefore) {
                    $.each($scope.detailData, function (key, value) {
                        $scope.detailData[key].expanded = true;
                    });
                    return;
                }

                var urlToInvoke = removeEncoding(url("/api/generic/ExtendedData/ExpandCompositions?" + $.param(buildExpandAllParams())));
                $http.get(urlToInvoke).success(function (result) {
                    $.each(result.resultObject[$scope.relationship], function (key, value) {
                        doToggle(value[$scope.compositiondetailschema.idFieldName], value, true);
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
                        //workaround for hapag
                        $scope.paginationData.selectedPage = $scope.paginationData.pageNumber;
                        init();
                    });
            };
        }]
    };
});

})(app, angular, jQuery);