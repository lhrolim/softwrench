(function (angular) {
    "use strict";

    var app = angular.module('sw_layout');

    app.directive('tabsrendered', function ($timeout, $log, $rootScope, eventService, schemaService, redirectService, spinService) {
        "ngInject";

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
                if (scope.$last === undefined) {
                    //0 tabs scenario
                    return $rootScope.$broadcast("sw_alltabsloaded");
                }
                
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
        "ngInject";

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

                controller: function ($scope, $http,$q,$element, $rootScope, $filter, $injector,
                formatService, fixHeaderService,dispatcherService,
                searchService, tabsService,
                fieldService, commandService, i18NService,
                submitService, redirectService,
                associationService, crudContextHolderService, alertService,
                validationService, schemaService, $timeout, $interval, eventService, $log, expressionService, focusService, modalService,
                compositionService, attachmentService, sidePanelService) {

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
                    eventService.dispatchEvent($scope.schema, "onschemafullyloaded");
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

                /**
                * Listener responsible for invoking providerloaded events.
                *  
                */
                $rootScope.$on("sw.crud.associations.updateeageroptions", function (event, associationKey, options, contextData) {
                    var displayables = fieldService.getDisplayablesByAssociationKey(crudContextHolderService.currentSchema(), associationKey);
                    var promiseArray = [];

                    for (var i = 0; i < displayables.length; i++) {
                        var displayable = displayables[i];
                        var providerLoadedEvent = displayable.events["providerloaded"];

                        if (providerLoadedEvent != undefined) {
                            var fn = dispatcherService.loadService(providerLoadedEvent.service, providerLoadedEvent.method);
                            if (fn != undefined) {
                                var fields = crudContextHolderService.rootDataMap();
                                if (fields && fields.fields) {
                                    fields = fields.fields;
                                }
                                var providerLoadedParameters = {
                                    fields: fields,
                                    options: options,
                                };
                                $log.getInstance('crudinputfieldcommons#updateeager', ["lifecycle"]).debug('invoking post load service {0} method {1} for association {2}|{3}'
                                    .format(providerLoadedEvent.service, providerLoadedEvent.method, displayable.target, displayable.associationKey));
                                promiseArray.push($q.when(fn(providerLoadedParameters)));
                            }
                        }
                    }
                    if (promiseArray.length > 0) {
                        return $q.all(promiseArray);
                    }
                    return $q.when();
                }
            );

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
                    var schema = crudContextHolderService.currentSchema();
                    var value = contextService.fetchFromContext("crud_context", true);
                    var id = direction == 1 ? value.detail_previous : value.detail_next;
                    if (id == undefined) {
                        return;
                    }
                    // If the detail crawl has a custom param, we need to get it from the pagination list
                    var customParams = {};
                    if (schema.properties && schema.properties["detail.crawl.customparams"]) {
                        // Get the next/previous record that the params will be coming from
                        var record = value.previousData.filter(function (obj) {
                            return obj.fields[schema.idFieldName] == id;
                        });

                        // TODO: If the record is null (item not on page in previous data)????

                        var customparamAttributes = schema.properties["detail.crawl.customparams"].replace(" ", "").split(",");
                        for (var param in customparamAttributes) {
                            if (!customparamAttributes.hasOwnProperty(param)) {
                                continue;
                            }
                            customParams[param] = {};
                            customParams[param]["key"] = customparamAttributes[param];
                            customParams[param]["value"] = record[0].fields[customparamAttributes[param]];
                        }
                    }

                    var mode = $scope.$parent.mode;
                    var popupmode = $scope.$parent.popupmode;
                    var schemaid = schema.schemaId;
                    var applicationname = schema.applicationName;
                    var title = $scope.$parent.title;

                    $scope.$emit("sw_navigaterequest", applicationname, schemaid, mode, title, { id: id, popupmode: popupmode, customParameters: customParams });

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


                    var modalSavefn = $scope.ismodal === "true" ? modalService.getSaveFn() : null;
                    //if there´s a custom modal service, let´s use it instead of the ordinary crud savefn
                    if (modalSavefn) {
                        var errorForm = $scope.crudform ? $scope.crudform.$error : {};
                        var validationErrors = validationService.validate(schemaToSave, schemaToSave.displayables, $scope.datamap.fields, errorForm);
                        if (validationErrors.length > 0) {
                            //interrupting here, can´t be done inside service
                            return;
                        }
                        var result = modalSavefn($scope.datamap.fields, schemaToSave);
                        if (result && result.then) {
                            result.then(function () {
                                modalService.hide();
                            });
                        }

                        return;
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


                        if (data.type !== "BlankApplicationResponse") {
                            var responseDataMap = data.resultObject;

                            // handle the case where the datamap had lazy compositions already fetched
                            // and the response does not have them (for performance reasons)
                            var compositions = compositionService.getLazyCompositions($scope.schema, $scope.datamap.fields);
                            compositions.forEach(function (composition) {
                                var currentValue = $scope.datamap.fields[composition];
                                var updatedValue = responseDataMap.fields[composition];
                                // has previous data but has no updated data: not safe to update -> hydrate with previous value
                                if (!!currentValue && (!responseDataMap.fields.hasOwnProperty(composition) || !angular.isArray(updatedValue))) {
                                    responseDataMap.fields[composition] = currentValue;
                                }
                            });

                            $scope.datamap = responseDataMap;
                        }
                        if (data.id && $scope.datamap.fields &&
                            /* making sure not to update when it's not creation */
                            (!$scope.datamap.fields.hasOwnProperty($scope.schema.idFieldName) || !$scope.datamap.fields[$scope.schema.idFieldName])
                            ) {
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

                // adds a padding right to not be behind side panels handles
                $scope.sidePanelStyle = function () {
                    var style = {};
                    if (sidePanelService.getTotalHandlesWidth() > 210) {
                        style["padding-right"] = "24px";
                    }
                    return style;
                }

                function init(self) {
                    $injector.invoke(BaseController, self, {
                        $scope: $scope,
                        i18NService: i18NService,
                        fieldService: fieldService,
                        commandService: commandService,
                        formatService: formatService
                    });
                    
                    //#region screenshot paste handling
                    // `contenteditable` element
                    var pasteCatcher = $element[0].querySelector(".js_crud_pastecatcher");

                    // 'paste' event listener -> 
                    // if image create an attachment with the clipboard data as the image
                    function pasteListener($event) {
                        var items = !window.clipboardData
                            ? ($event.clipboardData || $event.originalEvent.clipboardData).items // chrome: image comes from the event
                            : window.clipboardData.files; // IE: image comes from global object

                        // FF: has to wait for the image to be appended to a `contenteditable` element (as an image node) 
                        if (!items) {
                            $(pasteCatcher).focus();
                            attachmentService.createAttachmentFromElement(pasteCatcher, $scope.schema);
                            return true;
                        }

                        // look for image
                        var image = Array.prototype.slice
                            .call(items)
                            .filter(function (item) {
                                return item.type.startsWith("image");
                            });
                        // has no image: default behavior
                        if (image === undefined || image === null || image.length <= 0) return true;
                        // has image but is pasting inside richtext element
                        if ($event.target.tagName.equalIc("br") || $($event.target).parents("[text-angular]").length > 0) return true;
                        // can create the attachment
                        image = image[0];
                        attachmentService.createAttachmentFromFile(image, $scope.schema);
                        // prevent bubbling and default behavior
                        $event.stopPropagation();
                        $event.preventDefault();
                        return false;
                    };

                    $element.on("paste", pasteListener);
                    $scope.$on("$destroy", function () {
                        $element.off("paste", pasteListener);
                    });

                    function isBackground(element) {
                        return !!element // not null
                            && !["input", "textarea", "select", "button", "a", "selectize"].some(function (tag) { //not input
                                return element.tagName.equalIc(tag);
                            })
                            && !$(element).parents("[text-angular]").length > 0 // not inside richtext
                            && !$(element).hasClass("js_crud_pastecatcher"); // no the pasteCatcher
                    }

                    if (!isChrome()) {
                        // polls the current focused element:
                        // if its 'background' set focus on the pasteCatcher so it can capture `paste`
                        var interval = $interval(function () {
                            var focused = document.activeElement;
                            if (isBackground(focused)) {
                                $(pasteCatcher).focus();
                            }
                        }, 2000, 0, false);

                        $scope.$on("$destroy", function () {
                            $interval.cancel(interval);
                        });
                    }
                    //#endregion
                }
                init(this);


            }

        };
    });

})(angular);