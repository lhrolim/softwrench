var app = angular.module('sw_layout');

app.directive('tabsrendered', function ($timeout, $log, $rootScope, eventService, schemaService, redirectService, spinService) {
    /// <summary>
    /// This directive allows for a hookup method when all the tabs of the crud_body have finished rendered successfully.
    /// 
    /// Since the tabs are lazy loaded, we will replace default bootstrap behaviour of tab-toggle to use a custom engine that will dispatch an event, listened by all possible 
    /// tab implementations (compositionlist.js, crud_output.js and crud_input.js)
    /// 
    /// </summary>
    /// <param name="$timeout"></param>
    /// <param name="$log"></param>
    /// <param name="$rootScope"></param>
    /// <returns type=""></returns>
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            // Do not execute until the last iteration of ng-repeat has been reached,
            // or if $last is undefined (this happens when the tabsrendered directive 
            // is placed on something other than ng-repeat).
            if (scope.$last === false) {
                return;
            }

            var log = $log.getInstance('tabsrendered');
            log.debug("finished rendering tabs of detail screen");
            $timeout(function () {
                var firstTabId = null;
                $('.compositiondetailtab li>a').each(function () {
                    var $this = $(this);
                    if (firstTabId == null) {
                        firstTabId = $(this).data('tabid');
                    }
                    $this.click(function (e) {
                        e.preventDefault();
                        $this.tab('show');
                        var tabId = $(this).data('tabid');

                        log.trace('lazy loading tab {0}'.format(tabId));
                        spinService.stop({ compositionSpin: true });
                        $rootScope.$broadcast('sw_lazyloadtab', tabId);

                    });

                });
                $rootScope.$broadcast("sw_alltabsloaded", firstTabId);

            }, 0, false);

        }
    };
});


app.directive('crudBody', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_body.html'),
        scope: {
            isList: '=',
            isDetail: '=',
            blockedassociations: '=',
            associationSchemas: '=',
            schema: '=',
            datamap: '=',
            originalDatamap: '=',
            extraparameters: '=',
            isDirty: '=',
            savefn: '&',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            paginationdata: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            ismodal: '@',
            checked: '=',
            timestamp: '=',
        },

        link: function (scope, element, attrs) {
            scope.$name = 'crudbody';
        },

        controller: function ($scope, $http, $rootScope, $filter, $injector,
            formatService, fixHeaderService,
            searchService, tabsService,
            fieldService, commandService, i18NService,
            submitService, redirectService,
            associationService, crudContextHolderService, alertService,
            validationService, schemaService, $timeout, eventService, $log, expressionService, focusService) {

            $(document).on("sw_autocompleteselected", function (event, key) {
                focusService.resetFocusToCurrent($scope.schema, key);
            });


            $scope.$on("sw_alltabsloaded", function (event, firstTabId) {
                if (!$scope.schema) {
                    return;
                }

                var hasMainTab = schemaService.hasAnyFieldOnMainTab($scope.schema);
                if (!hasMainTab) {
                    //if main tab is absent (schema with just compositions) redirect to first tab
                    redirectService.redirectToTab(firstTabId);
                }
                $timeout(function () {
                    //time for the components to be rendered
                    focusService.setFocusToFirstField($scope.schema, $scope.datamap);
                }, 1000, false);

            });

            $scope.getTabRecordCount = function (tab) {
                return crudContextHolderService.getTabRecordCount(tab);
            }

            $scope.showTabRecordCount = function (tab) {
                return crudContextHolderService.shouldShowRecordCount(tab);
            }

            $scope.setForm = function (form) {
                $scope.crudform = form;
            };

            // Listeners region

            $scope.$on("sw_submitdata", function (event, parameters) {
                $scope.save(parameters);
            });

            $scope.$on('sw_compositiondataresolved', function (event, data) {
                var tab = crudContextHolderService.getActiveTab();
                if (tab != null && data[tab] != null) {
                    redirectService.redirectToTab(tab);
                }
            });

            $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent, parentElementId) {
                var log = $log.getInstance('on#sw_bodyrenderedevent');
                log.debug('enter');

                var onLoadMessage = contextService.fetchFromContext("onloadMessage", false, false, true);
                if (onLoadMessage) {
                    alertService.notifymessage('success', onLoadMessage);
                }
            });

            $scope.setActiveTab = function (tabId) {
                crudContextHolderService.setActiveTab(tabId);
            };
            $scope.hasTabs = function (schema) {
                return tabsService.hasTabs(schema);
            };
            $scope.isEditDetail = function (datamap, schema) {
                return datamap.fields[schema.idFieldName] != null;
            };
            $scope.request = function (datamap, schema) {
                return datamap.fields[schema.userIdFieldName];
            };

            $scope.request = function (datamap, schema) {
                return datamap.fields[schema.userIdFieldName];
            };

            $scope.shouldShowComposition = function (composition) {
                if (composition.hidden) {
                    return false;
                }
                return expressionService.evaluate(composition.showExpression, $scope.datamap, $scope);
            }

            $scope.toConfirmBack = function (data, schema) {
                var previousDataToUse = data;
                //https://controltechnologysolutions.atlassian.net/browse/SWWEB-1717
                //this line will assure that the grid is refreshed
                if (crudContextHolderService.needsServerRefresh()) {
                    previousDataToUse = null;
                }
                $scope.$emit('sw_canceldetail', previousDataToUse, schema, "Are you sure you want to go back?");
            };

            $scope.isCommand = function (schema) {
                if ($scope.schema.properties['command.select'] == "true") {
                    return true;
                }
            };

            $scope.hasAnyFieldOnMainTab = function (schema) {
                return schemaService.hasAnyFieldOnMainTab(schema);
            }

            $scope.shouldShowTitle = function () {
                return $scope.ismodal === "false" && $scope.schema.properties['detail.showtitle'] === 'true';
            }

            $scope.getTitle = function () {
                return schemaService.getTitle($scope.schema, $scope.datamap);
            }

            $scope.isNotHapagTest = function () {
                if ($rootScope.clientName != 'hapag')
                    return true;
            };
            $scope.tabsDisplayables = function (schema) {
                return tabsService.tabsDisplayables(schema);
            };



            function defaultSuccessFunction(data) {
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                if (data != null) {
                    if (data.type === 'ActionRedirectResponse') {
                        //we´ll not do a crud action on this case, so totally different workflow needed
                        redirectService.redirectToAction(null, data.controller, data.action, data.parameters);
                    } else if (data.type !== 'BlankApplicationResponse') {
                        var nextSchema = data.schema;
                        $scope.$parent.renderViewWithData(nextSchema.applicationName, nextSchema.schemaId, nextSchema.mode, nextSchema.title, data);
                    }
                }

            }

            $scope.showNavigationButtons = function (schema) {
                var property = schema.properties['detail.navigationbuttons.disabled'];
                return "true" != property && $scope.ismodal == "false";
            };

            $scope.disableNavigationButton = function (direction) {
                var value = contextService.fetchFromContext("crud_context", true);
                if (value == undefined) {
                    return true;
                }
                return direction == 1 ? value.detail_previous : value.detail_next;
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

            $scope.getTabIcon = function (tab) {
                return tab.schema.schemas.list.properties['icon.composition.tab'];
            };

            $scope.getDetailTabTitle = function (schema, datamap) {
                return i18NService.getI18nRecordLabel(schema, datamap) + ' Details';
            };

            $scope.crawl = function (direction) {
                var value = contextService.fetchFromContext("crud_context", true);
                var id = direction == 1 ? value.detail_previous : value.detail_next;
                if (id == undefined) {
                    return;
                }

                var mode = $scope.$parent.mode;
                var popupmode = $scope.$parent.popupmode;
                var schemaid = $scope.$parent.schema.schemaId;
                var applicationname = $scope.$parent.schema.applicationName;
                var title = $scope.$parent.title

                $scope.$emit("sw_navigaterequest", applicationname, schemaid, mode, title, { id: id, popupmode: popupmode });

            };




            $scope.delete = function () {

                var schema = $scope.schema;
                var idFieldName = schema.idFieldName;
                var applicationName = schema.applicationName;
                var id = $scope.datamap.fields[idFieldName];

                var parameters = {};
                if (sessionStorage.mockmaximo == "true") {
                    parameters.mockmaximo = true;
                }
                parameters.platform = platform();
                parameters = addSchemaDataToParameters(parameters, $scope.schema);
                var deleteParams = $.param(parameters);

                var deleteURL = removeEncoding(url("/api/data/" + applicationName + "/" + id + "?" + deleteParams));
                $http.delete(deleteURL)
                    .success(function (data) {
                        defaultSuccessFunction(data);
                    });
            };

            $scope.cancel = function (data, schema) {
                var previousDataToUse = data;
                //https://controltechnologysolutions.atlassian.net/browse/SWWEB-1717
                //this line will assure that the grid is refreshed
                if (crudContextHolderService.needsServerRefresh()) {
                    previousDataToUse = null;
                }
                $scope.cancelfn({ data: previousDataToUse, schema: schema });
            }



            $scope.save = function (parameters) {
                var log = $log.getInstance('crudbody#save');
                parameters = parameters || {};

                var schemaToSave = $scope.schema;
                if (parameters.schema) {
                    schemaToSave = parameters.schema;
                }

                if ($rootScope.showingModal && $scope.$parent.$parent.$name === "crudbodymodal") {
                    //workaround to invoke the original method that was passed to the modal, instead of the default save.
                    //TODO: use angular's & support
                    if ($scope.$parent.$parent.originalsavefn) {
                        var validationErrors = validationService.validate(schemaToSave, schemaToSave.displayables, $scope.datamap.fields, $scope.crudform.$error);
                        if (validationErrors.length > 0) {
                            //interrupting here, can´t be done inside service
                            return;
                        }
                        $scope.$parent.$parent.originalsavefn($scope.datamap.fields, schemaToSave);
                        return;
                    }

                }
                var selecteditem = parameters.selecteditem;
                //selectedItem would be passed in the case of a composition with autocommit=true, in the case the target would accept only the child instance... not yet supported. 
                //Otherwise, fetching from the $scope.datamap
                var fromDatamap = selecteditem == null;
                var fields = fromDatamap ? $scope.datamap.fields : selecteditem;

                var originalDatamap = $scope.originalDatamap;
                if (parameters.originalDatamap) {
                    originalDatamap = parameters.originalDatamap;
                }



                //need an angular.copy to prevent beforesubmit transformation events from modifying the original datamap.
                //this preserves the datamap (and therefore the data presented to the user) in case of a submission failure
                var transformedFields = angular.copy(fields);

                var eventParameters = {
                    originaldatamap: originalDatamap.fields,
                    'continue': function () {
                        $scope.validateSubmission(selecteditem, parameters, transformedFields, schemaToSave);
                    }
                };

                var eventResult = eventService.beforesubmit_prevalidation(schemaToSave, transformedFields, eventParameters);
                if (eventResult === false) {
                    //this means that the custom service should call the continue method
                    log.debug('waiting on custom prevalidation to invoke the continue function');
                    return;
                }

                $scope.validateSubmission(selecteditem, parameters, transformedFields, schemaToSave);
            };

            $scope.validateSubmission = function (selecteditem, parameters, transformedFields, schemaToSave) {
                var log = $log.getInstance('crudbody#validateSubmission');
                //hook for updating doing custom logic before sending the data to the server
                $rootScope.$broadcast("sw_beforesubmitprevalidate_internal", transformedFields);

                if (sessionStorage.mockclientvalidation == undefined) {
                    var validationErrors = validationService.validate(schemaToSave, schemaToSave.displayables, transformedFields, $scope.crudform.$error);
                    if (validationErrors.length > 0) {
                        //interrupting here, can´t be done inside service
                        return;
                    }
                }

                var originalDatamap = $scope.originalDatamap;
                if (parameters.originalDatamap) {
                    originalDatamap = parameters.originalDatamap;
                }

                var eventParameters = {
                    originaldatamap: originalDatamap.fields,
                    'continue': function () {
                        $scope.submitToServer(selecteditem, parameters, transformedFields, schemaToSave);
                    }
                }

                var eventResult = eventService.beforesubmit_postvalidation(schemaToSave, transformedFields, eventParameters);
                if (eventResult == false) {
                    //this means that the custom postvalidator should call the continue method
                    log.debug('waiting on custom postvalidator to invoke the continue function');
                    return;
                }

                $scope.submitToServer(selecteditem, parameters, transformedFields, schemaToSave);
            };

            $scope.submitToServer = function (selecteditem, parameters, transformedFields, schemaToSave) {
                $rootScope.$broadcast("sw_beforesubmitpostvalidate_internal", transformedFields);
                parameters = parameters || {};
                var originalDatamap = $scope.originalDatamap;
                if (parameters.originalDatamap) {
                    originalDatamap = parameters.originalDatamap;
                }

                //some fields might require special handling
                submitService.removeNullInvisibleFields(schemaToSave.displayables, transformedFields);
                transformedFields = submitService.removeExtraFields(transformedFields, true, schemaToSave);
                submitService.translateFields(schemaToSave.displayables, transformedFields);
                associationService.insertAssocationLabelsIfNeeded(schemaToSave, transformedFields);
                submitService.handleDatamapForMIF(schemaToSave, originalDatamap.fields, transformedFields);


                var successCbk = parameters.successCbk;
                var failureCbk = parameters.failureCbk;
                var nextSchemaObj = parameters.nextSchemaObj;
                var applyDefaultSuccess = parameters.applyDefaultSuccess;
                var applyDefaultFailure = parameters.applyDefaultFailure;
                var isComposition = parameters.isComposition;

                var applicationName = schemaToSave.applicationName;
                var idFieldName = schemaToSave.idFieldName;
                var id = transformedFields[idFieldName];



                var submissionParameters = submitService.createSubmissionParameters(transformedFields, schemaToSave, nextSchemaObj, id, parameters.dispatcherComposition);

                var jsonWrapper = {
                    json: transformedFields,
                    requestData: submissionParameters
                }

                var jsonString = angular.toJson(jsonWrapper);

                $rootScope.savingMain = !isComposition;

                if (isIe9()) {
                    var formToSubmitId = submitService.getFormToSubmitIfHasAttachement(schemaToSave.displayables, transformedFields);
                    if (formToSubmitId != null) {
                        var form = $(formToSubmitId);
                        submitService.submitForm(form, submissionParameters, jsonString, applicationName);
                        return;
                    }
                }

                if ("true" === sessionStorage.logJSON) {
                    $log.info(jsonString);
                }

                $log.getInstance("crud_body#submit").debug(jsonString);


                var urlToUse = url("/api/data/" + applicationName + "/");
                var command = id == null ? $http.post : $http.put;

                command(urlToUse, jsonString).success(function (data) {
                    crudContextHolderService.afterSave();


                    if (data.type !== 'BlankApplicationResponse') {
                        $scope.datamap = data.resultObject;
                    }
                    if (data.id && $scope.datamap.fields) {
                        //updating the id, useful when it´s a creation and we need to update value return from the server side
                        $scope.datamap.fields[$scope.schema.idFieldName] = data.id;
                    }

                    if (successCbk == null || applyDefaultSuccess) {
                        defaultSuccessFunction(data);
                    }
                    if (successCbk != null) {
                        successCbk(data);
                    }

                    $scope.$emit('sw.crud.detail.savecompleted', data);
                }).error(function (data) {
                    if (failureCbk != null) {
                        failureCbk(data);
                    }
                });
            };



            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService,
                formatService: formatService
            });


        }

    };
});

