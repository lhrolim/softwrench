(function (angular) {
    "use strict";
    const app = angular.module('sw_layout');

    app.directive('crudBody', function (contextService, genericTicketService) {
        "ngInject";

        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_body.html'),
            scope: {
                isList: '=',
                isDetail: '=',
                blockedassociations: '=',
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
                panelid: "@",
                ismodal: '@',
                checked: '=',
                timestamp: '=',
            },

            link: function (scope, element, attrs) {
                scope.$name = 'crudbody';

                if (scope.ismodal === "true") {
                    //handling the case where the modal is opened for the firsttime before the directive was even compiled
                    scope.allTabsLoaded();
                }

            },

            controller: function ($scope, $http, $q, $element, $rootScope, $filter, $injector,
            formatService, fixHeaderService, dispatcherService,
            searchService, tabsService, crudCrawlService,
            fieldService, commandService, i18NService,
            submitService, redirectService,
            associationService, crudContextHolderService, alertService,
            validationService, schemaService, $timeout, $interval, eventService, $log, expressionService, focusService, modalService,
            compositionService, attachmentService, sidePanelService, spinService) {

                //holding the spinners used for the tabs on the screen, at SWWEB-2905
                const tabSpinners = [];

                this.shouldshowprint = function () {
                    return $scope.schema.stereotypeAttr !== 'detailnew';
                }

                this.getCancelLabel = function () {
                    if ($scope.schema.stereotypeAttr === 'detailnew') {
                        return 'Cancel';
                    }

                    return 'Back';
                };

                this.getCancelIcon = function () {
                    if ($scope.schema.stereotypeAttr === 'detailnew') {
                        return 'fa-times';
                    }

                    return 'fa-arrow-left';
                };

                this.enableSave = function () {
                    return !genericTicketService.isClosed();
                };

                this.saveTooltip = function () {
                    if (genericTicketService.isClosed()) {
                        return 'You can\'t change closed tickets.';
                    }

                    return '';
                };

                $(document).on("sw_autocompleteselected", function (event, key) {
                    focusService.resetFocusToCurrent($scope.schema, key);
                });

                $scope.$on(JavascriptEventConstants.ModalShown, function (event, modalData) {
                    if ($scope.ismodal === "true") {
                        if (modalData && modalData.schema) {
                            $scope.schema = modalData.schema;
                        }
                        $scope.allTabsLoaded();
                    }
                
                });

                $scope.$on(JavascriptEventConstants.TabsLoaded, function (event, firstTabId, panelId) {
                    if ($scope.panelid !== panelId || (!schemaService.areTheSame($scope.schema, crudContextHolderService.currentSchema()))) {
                        return;
                    }

                    tabSpinners.forEach(s => {
                        //just in case, enforcing spinners have been stopped.
                        s.stop();
                    });
                    
                    tabSpinners.splice(0,tabSpinners.length);
                    $(".tabRecordspin").each((idx,e) => {
                        tabSpinners.push(spinService.startSpinner(e, { extraSmall: true }));
                    });

                    $scope.allTabsLoaded(event, firstTabId);
                });

                $scope.allTabsLoaded = function (event, firstTabId) {
                    if (!$scope.schema) {
                        return;
                    }
                    const hasMainTab = schemaService.hasAnyFieldOnMainTab($scope.schema);
                    if (!hasMainTab) {
                        //if main tab is absent (schema with just compositions) redirect to first tab
                        redirectService.redirectToTab(firstTabId);
                    }

                    // covers breadcrumb redirect when the target page does not have the active tab of the src page
                    const tab = crudContextHolderService.getActiveTab();
                    var datamap = $scope.datamap;
                    if (tab != null && !tabsService.hasTab($scope.schema, tab)) {
                        // active tab not found
                        redirectService.redirectToTab($scope.getMainTabId());
                    }

                    $timeout(function () {
                        //time for the components to be rendered
                        focusService.setFocusToFirstField($scope.schema, datamap);
                    }, 1000, false);
                    eventService.onschemafullyloaded($scope.schema);
                
                }

                $scope.showTabBusyCursor = function(element) {
                    const spinneroptions = {
                        small: true
                    };
                    spinService.startSpinner(element,spinneroptions);
                }

                $scope.getTabRecordCount = function (tab) {
                    return crudContextHolderService.getTabRecordCount(tab);
                }

                $scope.showTabRecordCount = function (tab) {
                    return crudContextHolderService.shouldShowRecordCount(tab);
                }

                $scope.setForm = function (form) {
                    crudContextHolderService.crudForm($scope.panelid, form);
                };

                $scope.getPosition = function (schema, propertyName, defaultPosition) {
                    if (!schema.properties || !schema.properties[propertyName]) {
                        if ("true" === $scope.ismodal) {
                            return "modal." + defaultPosition;

                        }
                        return defaultPosition;
                    }
                    return schema.properties[propertyName];
                };

                // Listeners region

                /**
                * Listener responsible for invoking providerloaded events.
                *  
                */
                $rootScope.$on(JavascriptEventConstants.Association_EagerOptionUpdated, function (event, associationKey, options, contextData) {
                    if (contextData && contextData.schemaId === "#modal" && "true" !== $scope.ismodal) {
                        //ignoring 
                        return $q.reject();
                    }
                    const panelId = (contextData && contextData.schemaId === "#modal") ? "#modal" : null;
                    const displayables = fieldService.getDisplayablesByAssociationKey(crudContextHolderService.currentSchema(panelId), associationKey);
                    const promiseArray = [];
                    for (let i = 0; i < displayables.length; i++) {
                        const displayable = displayables[i];
                        const fields = crudContextHolderService.rootDataMap();
                        const providerLoadedParameters = {
                            fields: fields,
                            options: options
                        };

                        $log.getInstance('crudinputfieldcommons#updateeager', ["lifecycle"]).debug(`Invoking post load association ${displayable.target}|${displayable.associationKey}`);
                        const result = eventService.providerloaded(displayable, providerLoadedParameters);
                        if (result) {
                            promiseArray.push($q.when(result));
                        }
                    }
                    if (promiseArray.length > 0) {
                        return $q.all(promiseArray);
                    }
                    return $q.when();
                }
            );

//                $scope.$on(JavascriptEventConstants.CrudSubmitData, function (event, parameters) {
//                    if ($scope.ismodal !== "true" && !!parameters.dispatchedByModal) {
//                        return;
//                    } else if ($scope.ismodal === "true" && !parameters.dispatchedByModal) {
//                        return;
//                    }
//                    $scope.save(parameters);
//                });




                $scope.$on(JavascriptEventConstants.COMPOSITION_RESOLVED, function (event, data) {
                    const tab = crudContextHolderService.getActiveTab();
                    if (tab != null && data[tab] != null) {
                        redirectService.redirectToTab(tab);
                    }

                    tabSpinners.forEach(s => {
                        s.stop();
                    });

                });

                $scope.$on(JavascriptEventConstants.BodyRendered, function (ngRepeatFinishedEvent, parentElementId) {
                    const log = $log.getInstance('on#sw_bodyrenderedevent');
                    log.debug('enter');
                    const onLoadMessage = contextService.fetchFromContext("onloadMessage", false, false, true);
                    if (onLoadMessage) {
                        alertService.notifymessage('success', onLoadMessage);
                    }



                });

                $scope.getMainTabId = function () {
                    if ($scope.ismodal === "true") {
                        return "modalmain";
                    }
                    return "main";
                }

                $scope.setActiveTab = function (tabId) {
                    crudContextHolderService.setActiveTab(tabId);
                    $rootScope.$broadcast("sw4_activetabchanged", tabId);
                };
                $scope.hasTabs = function (schema) {
                    return tabsService.hasTabs(schema);
                };
                $scope.isEditDetail = function (datamap, schema) {
                    return datamap[schema.idFieldName] != null;
                };
                $scope.request = function (datamap, schema) {
                    return datamap[schema.userIdFieldName];
                };

                $scope.request = function (datamap, schema) {
                    return datamap[schema.userIdFieldName];
                };

                $scope.shouldShowComposition = function (composition) {
                    if (composition.hidden) {
                        return false;
                    }
                    return expressionService.evaluate(composition.showExpression, $scope.datamap, $scope);
                }

                // method for defined <tab> on metadata
                $scope.shouldShowTab = function (tab) {
                    return $scope.shouldShowComposition(tab);
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




                $scope.showNavigationButtons = function (schema) {
                    const property = schema.properties['detail.navigationbuttons.disabled'];
                    return "true" != property && $scope.ismodal == "false";
                };

                $scope.showdirectionButtons = function () {
                    const value = contextService.fetchFromContext("crud_context", true);

                    // is detail and is not creation
                    return schemaService.isDetail($scope.schema) && !!$scope.datamap[$scope.schema.idFieldName] && (value && (value.detail_previous || value.detail_next));
                };

                $scope.disableNavigationButton = function (direction) {
                    const value = contextService.fetchFromContext("crud_context", true);
                    if (value == undefined) {
                        return true;
                    }
                    return direction == 0 ? value.detail_previous : value.detail_next;
                }

                $scope.showActionSeparator = function (position) {
                    const commands = commandService.getBarCommands($scope.schema, position);
                    if (commands == null) {
                        return false;
                    }

                    return commands.length > 0;
                }

                $scope.isEditing = function (schema) {
                    const idFieldName = schema.idFieldName;
                    const id = $scope.datamap[idFieldName];
                    return id != null;
                };

                $scope.shouldShowField = function (expression) {
                    if (expression == "true") {
                        return true;
                    }
                    const stringExpression = '$scope.datamap.' + expression;
                    const ret = eval(stringExpression);
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
                    const data = crudCrawlService.crawlData(direction);
                    if (!data) {
                        return;
                    }
                    const mode = $scope.$parent.mode;
                    const popupmode = $scope.$parent.popupmode;
                    const title = $scope.$parent.title;
                    $scope.$emit(JavascriptEventConstants.NavigateRequestCrawl, data.applicationname, data.schemaid, mode, title, { id: data.id, popupmode: popupmode, customParameters: data.customParameters });
                };

                $scope.delete = function () {
                    const schema = $scope.schema;
                    const idFieldName = schema.idFieldName;
                    const applicationName = schema.applicationName;
                    const id = $scope.datamap[idFieldName];
                    var parameters = {};
                    if (sessionStorage.mockmaximo == "true") {
                        parameters.mockmaximo = true;
                    }
                    parameters.platform = platform();
                    parameters = addSchemaDataToParameters(parameters, $scope.schema);
                    const deleteParams = $.param(parameters);
                    const deleteURL = removeEncoding(url("/api/data/" + applicationName + "/" + id + "?" + deleteParams));
                    return $http.delete(deleteURL)
                        .then(function (response) {
                            const data = response.data;
                            defaultSuccessFunction(data);
                        });
                };

                this.cancel = function (data, schema) {
                    return $scope.cancel(data, schema);
                }

                $scope.cancel = function (data, schema) {
                    var previousDataToUse = data;
                    //https://controltechnologysolutions.atlassian.net/browse/SWWEB-1717
                    //this line will assure that the grid is refreshed
                    if (crudContextHolderService.needsServerRefresh()) {
                        previousDataToUse = null;
                    }
                    $scope.cancelfn({ data: previousDataToUse, schema: schema });
                }


                this.save = function (parameters) {
                    return $scope.save();
                }

                // flag to block multiple save calls while one is still executing
                $scope.save = function (parameters ={}) {
                    const schemaToSave = parameters.schema || $scope.schema;
                    parameters.originaldatamap = parameters.originaldatamap || $scope.originalDatamap;
                    return submitService.submit(schemaToSave, $scope.datamap, parameters);
                };



                // adds a padding right to not be behind side panels handles
                $scope.sidePanelStyle = function () {
                    const style = {};
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
                        const items = !window.clipboardData
                            ? ($event.clipboardData || $event.originalEvent.clipboardData).items // chrome: image comes from the event
                            : window.clipboardData.files; // IE: image comes from global object

                        // FF: has to wait for the image to be appended to a `contenteditable` element (as an image node) 
                        if (!items) {
                            $(pasteCatcher).focus();
                            attachmentService.createAttachmentFromElement(pasteCatcher, crudContextHolderService.currentSchema());
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
                        attachmentService.createAttachmentFromFile(image, crudContextHolderService.currentSchema());
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
                            && !$(element).parents("richtext-field").length > 0 // not inside richtext
                            && !$(element).hasClass("js_crud_pastecatcher"); // not the pasteCatcher
                    }

                    if (!isChrome()) {
                        // polls the current focused element:
                        // if its 'background' set focus on the pasteCatcher so it can capture `paste`
                        var interval = $interval(function () {
                            const focused = document.activeElement;
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
