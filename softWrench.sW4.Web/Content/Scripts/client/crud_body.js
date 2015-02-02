var app = angular.module('sw_layout');

app.directive('tabsrendered', function ($timeout, $log, $rootScope, eventService) {
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
                $('.compositiondetailtab li>a').each(function () {
                    var $this = $(this);
                    $this.click(function (e) {
                        e.preventDefault();
                        $this.tab('show');
                        var tabId = $(this).data('tabid');

                        log.trace('lazy loading tab {0}'.format(tabId));
                        $rootScope.$broadcast('sw_lazyloadtab', tabId);
                    });
                });
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
            associationOptions: '=',
            associationSchemas: '=',
            schema: '=',
            datamap: '=',
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
            associationService, contextService, alertService,
            validationService, schemaService, $timeout, eventService, $log, expressionService) {

            $scope.setForm = function (form) {
                $scope.crudform = form;
            };

            // Listeners region

            $scope.$on("sw_submitdata", function (event, parameters) {
                $scope.save(parameters);
            });

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                if (!$rootScope.showSuccessMessage) {
                    fixHeaderService.resetTableConfig($scope.schema);
                }
            });

            $scope.$on('sw_errormessage', function (event, show) {
                fixHeaderService.topErrorMessageHandler(show, $scope.$parent.isDetail, $scope.schema);
            });

            $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent, parentElementId) {
                //Save the originalDatamap after the body finishes rendering. This will be used in the submit service to update
                //associations that were "removed" with a " ". This is because a null value, when sent to the MIF, is ignored
                $scope.originalDatamap = angular.copy($scope.datamap);

                var tab = contextService.getActiveTab();
                if (tab != null) {
                    redirectService.redirectToTab(tab);
                }

                //make sure we are seeing the top of the detail page 
                window.scrollTo(0, 0);

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
            });

            $scope.setActiveTab = function (tabId) {
                contextService.setActiveTab(tabId);
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

            $scope.toConfirmBack = function (data, schema) {
                $scope.$emit('sw_canceldetail', data, schema, "Are you sure you want to go back?");
            };

            $scope.isCommand = function (schema) {
                if ($scope.schema.properties['command.select'] == "true") {
                    return true;
                }
            };

            $scope.getTitle = function () {
                var schema = $scope.schema;
                var datamap = $scope.datamap;
                if (schema.properties['detail.titleexpression'] != null) {
                    return expressionService.evaluate(schema.properties['detail.titleexpression'], $scope.datamap.fields);
                }
                var titleId = schema.idDisplayable;
                var result = titleId + " " + datamap.fields[schema.userIdFieldName];
                if (datamap.fields.description != null) {
                    result += " Summary: " + datamap.fields.description;
                }
                return result;
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
                    if (data.type == 'ActionRedirectResponse') {
                        //we´ll not do a crud action on this case, so totally different workflow needed
                        redirectService.redirectToAction(null, data.controller, data.action, data.parameters);
                    } else {
                        var nextSchema = data.schema;
                        $scope.$parent.renderViewWithData(nextSchema.applicationName, nextSchema.schemaId, nextSchema.mode, nextSchema.title, data);
                    }
                }

            }

            $scope.disableNavigationButtons = function (schema) {
                var property = schema.properties['detail.navigationbuttons.disabled'];
                return property == "true";
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

            $scope.getTabIcon = function (tab) {
                return tab.schema.schemas.list.properties['icon.composition.tab'];
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
                var title = $scope.$parent.title;

                $scope.$emit("sw_renderview", applicationname, schemaid, mode, title, { id: id, popupmode: popupmode });

            };

            $scope.disableNavigationButton = function (direction) {
                var value = contextService.fetchFromContext("crud_context", true);
                return direction == 1 ? value.detail_previous : value.detail_next;
            }


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
                $scope.cancelfn({ data: data, schema: schema });
            }



            $scope.save = function (parameters) {
                var log = $log.getInstance('crudbody#save');
                parameters = instantiateIfUndefined(parameters);

                if ($rootScope.showingModal && $scope.$parent.$parent.$name == "crudbodymodal") {
                    //workaround to invoke the original method that was passed to the modal, instead of the default save.
                    //TODO: use angular's & support
                    $scope.$parent.$parent.originalsavefn($scope.datamap.fields);
                    return;
                }
                var selecteditem = parameters.selecteditem;
                //selectedItem would be passed in the case of a composition with autocommit=true, in the case the target would accept only the child instance... not yet supported. 
                //Otherwise, fetching from the $scope.datamap
                var fromDatamap = selecteditem == null;
                var itemToSave = fromDatamap ? $scope.datamap : selecteditem;
                var fields = fromDatamap ? itemToSave.fields : itemToSave;

                //need an angular.copy to prevent beforesubmit transformation events from modifying the original datamap.
                //this preserves the datamap (and therefore the data presented to the user) in case of a submission failure
                var transformedFields = angular.copy(fields);

                var eventParameters = {};
                eventParameters.continue = function () {
                    $scope.validateSubmission(selecteditem, parameters, transformedFields);
                };

                var eventResult = eventService.beforesubmit_prevalidation($scope.schema, transformedFields, eventParameters);
                if (eventResult == false) {
                    //this means that the custom service should call the continue method
                    log.debug('waiting on custom prevalidation to invoke the continue function');
                    return;
                }

                $scope.validateSubmission(selecteditem, parameters, transformedFields);
            };

            $scope.validateSubmission = function (selecteditem, parameters, transformedFields) {
                var log = $log.getInstance('crudbody#validateSubmission');
                //hook for updating doing custom logic before sending the data to the server
                $rootScope.$broadcast("sw_beforesubmitprevalidate_internal", transformedFields);

                if (sessionStorage.mockclientvalidation == undefined) {
                    var validationErrors = validationService.validate($scope.schema, $scope.schema.displayables, transformedFields, $scope.crudform.$error);
                    if (validationErrors.length > 0) {
                        //interrupting here, can´t be done inside service
                        return;
                    }
                }

                var eventParameters = {
                    'continue': function () {
                        $scope.submitToServer(selecteditem, parameters, transformedFields);
                    }
                }

                var eventResult = eventService.beforesubmit_postvalidation($scope.schema, transformedFields, eventParameters);
                if (eventResult == false) {
                    //this means that the custom postvalidator should call the continue method
                    log.debug('waiting on custom postvalidator to invoke the continue function');
                    return;
                }

                $scope.submitToServer(selecteditem, parameters, transformedFields);
            };

            $scope.submitToServer = function (selecteditem, parameters, transformedFields) {
                $rootScope.$broadcast("sw_beforesubmitpostvalidate_internal", transformedFields);
                var localScope = parameters.scope == null ? $scope : parameters.scope;
                //some fields might require special handling
                submitService.removeNullInvisibleFields($scope.schema.displayables, transformedFields);
                transformedFields = submitService.removeExtraFields(transformedFields, true, $scope.schema);
                submitService.translateFields($scope.schema.displayables, transformedFields);
                associationService.insertAssocationLabelsIfNeeded($scope.schema, transformedFields, $scope.associationOptions);
                submitService.handleDatamapForMIF($scope.schema, localScope.originalDatamap.fields, transformedFields);

                if (parameters == undefined) {
                    parameters = {};
                }
                var successCbk = parameters.successCbk;
                var failureCbk = parameters.failureCbk;
                var nextSchemaObj = parameters.nextSchemaObj;
                var applyDefaultSuccess = parameters.applyDefaultSuccess;
                var applyDefaultFailure = parameters.applyDefaultFailure;
                var isComposition = parameters.isComposition;

                var applicationName = $scope.schema.applicationName;
                var idFieldName = $scope.schema.idFieldName;
                var id = transformedFields[idFieldName];

                var jsonString = angular.toJson(transformedFields);

                var submissionParameters = submitService.createSubmissionParameters($scope.schema, nextSchemaObj, id);

                $rootScope.savingMain = !isComposition;

                if (isIe9()) {
                    var formToSubmitId = submitService.getFormToSubmitIfHasAttachement($scope.schema.displayables, transformedFields);
                    if (formToSubmitId != null) {
                        var form = $(formToSubmitId);
                        submitService.submitForm(form, submissionParameters, jsonString, applicationName);
                        return;
                    }
                }

                var urlToUse = url("/api/data/" + applicationName + "/?" + $.param(submissionParameters));
                var command = id == null ? $http.post : $http.put;

                command(urlToUse, jsonString)
                    .success(function (data) {
                        //datamap should always be updated
                        $scope.datamap = data.resultObject;
                        if (successCbk == null || applyDefaultSuccess) {
                            defaultSuccessFunction(data);
                        }
                        if (successCbk != null) {
                            successCbk(data);
                        }
                    })
                    .error(function (data) {
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

