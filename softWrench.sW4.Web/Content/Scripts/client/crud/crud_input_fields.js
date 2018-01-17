(function (app, angular) {
    "use strict";


    app.directive("configUpdateSectionDatamap", function () {
        return {
            restrict: "A",
            link: function (scope, element, attr) {
                if (scope.$first) {
                    scope.datamap[scope.fieldMetadata.id] = [];
                }
                const item = {
                    label: scope.i18NLabel(scope.field),
                    value: scope.datamap[scope.field.attribute],
                    '#newvalue': ""
                };
                scope.$watch(`datamap["${scope.field.attribute}"]`, function (newValue, oldValue) {
                    if (oldValue === newValue) {
                        return;
                    }
                    scope.datamap[scope.fieldMetadata.id][scope.$index]["value"] = newValue;
                });
                scope.datamap[scope.fieldMetadata.id].push(item);
            }
        };
    })

        .directive("sectionElementInput", ["$compile", "$timeout", "crudContextHolderService", function ($compile, $timeout, crudContextHolderService) {
            return {
                restrict: "E",
                replace: true,
                scope: {
                    schema: '=',
                    fieldMetadata: '=',
                    datamap: '=',
                    isDirty: '=',
                    displayables: '=',
                    blockedassociations: '=',
                    extraparameters: '=',
                    elementid: '@',
                    orientation: '@',
                    islabelless: '@',
                    panelid: '@',
                    ismodal: '@',
                    lookupAssociationsCode: '=',
                    lookupAssociationsDescription: '=',
                    rendererParameters: '=',
                    headerSection: '@',
                    tabsection: "@",
                    cancelfn: "&"
                },
                template: "<div></div>",
                link: function (scope, element, attrs) {


                    if (angular.isArray(scope.displayables)) {

                        scope.sectionParameters = scope.rendererParameters || {};

                        if (scope.fieldMetadata && scope.fieldMetadata.header) {
                            if (scope.fieldMetadata.header.parameters) {
                                Object.keys(scope.fieldMetadata.header.parameters).forEach(k => {
                                    scope.sectionParameters[k] = scope.fieldMetadata.header.parameters[k];
                                });
                            }
                        }

                        //                    scope.rendererParameters = [];

                        if (scope.headerSection === "true") {
                            //check crud_layoutservice.js getfieldclass
                            scope.sectionParameters["headerSection"] = true;
                        }



                        element.append(
                            "<crud-input-fields displayables='displayables'"+
                            "schema='schema'" +
                            "datamap='datamap'" +
                            "is-dirty='isDirty'" +
                            "ismodal = '{{ismodal}}'" +
                            "panelid = 'panelid'" +
                            "extraparameters='extraparameters'"+
                            "blockedassociations='blockedassociations'" +
                            "section-parameters='sectionParameters'" +
                            "elementid='{{elementid}}'" +
                            "orientation='{{orientation}}' insidelabellesssection='{{islabelless}}'" +
                            "outerassociationcode='lookupAssociationsCode' " +
                            "outerassociationdescription='lookupAssociationsDescription' " +
                            "issection='true'" +
                            "tabsection='{{tabsection}}'" +
                            "></crud-input-fields>"
                        );
                        $timeout(function () {
                                // mark data as not resolved since the template has not yet been compiled: can still trigger formatters and default values
                                // avoiding bugs on the dirty checker this way
                                crudContextHolderService.clearDetailDataResolved();
                                $compile(element.contents())(scope);
                            }, 0, false)
                            .finally(() => // section already compiled, formatters and default values already triggered: mark data as resolved
                                crudContextHolderService.setDetailDataResolved());
                    }
                }
            }
        }])
        .directive("crudInputFields", ["contextService", "eventService", "crud_inputcommons", "crudContextHolderService", function (contextService, eventService, crud_inputcommons, crudContextHolderService) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_input_fields.html'),
                scope: {
                    schema: '=',
                    datamap: '=',
                    extraparameters: '=',
                    isDirty: '=',
                    displayables: '=',
                    blockedassociations: '=',
                    elementid: '@',
                    orientation: '@',
                    headerSection: '@',
                    insidelabellesssection: '@',
                    previousdata: '=',
                    previousschema: '=',
                    parentdata: '=',
                    parentschema: '=',
                    panelid: "=",
                    outerassociationcode: '=',
                    sectionParameters: '=',
                    outerassociationdescription: '=',
                    issection: '@',
                    ismodal: '@',
                    tabsection: "@",
                    cancelfn: "&"
                },

                link: function (scope, element, attrs) {
                    if (scope.outerassociationcode && scope.issection == "true") {
                        //this is for sections, we receive it as parameters
                        scope.lookupAssociationsCode = scope.outerassociationcode;
                        scope.lookupAssociationsDescription = scope.outerassociationdescription;
                    } else {
                        scope.lookupAssociationsCode = {};
                        scope.lookupAssociationsDescription = {};

                        if (scope.ismodal === "true") {
                            scope.$on(JavascriptEventConstants.ModalShown, function (event, modaldata) {
                                scope.lookupAssociationsCode = {};
                                scope.lookupAssociationsDescription = {};
                                scope.crudInputForm.$setPristine();
                            });
                        }
                    }

                    const parameters = {
                        element: element,
                        tabid: crudContextHolderService.getActiveTab()
                    };
                    eventService.onload(scope, scope.schema, scope.datamap, parameters);

                    scope.getInputType = function (fieldMetadata) {
                        if (fieldMetadata.rendererType === "email") {
                            return "email";
                        }
                        else if (fieldMetadata.rendererType === "password") {
                            return "password";
                        }
                        return "text";
                    }

                    //dnd-list cannot be bound to a method: it requires a variable to work
                    scope.nonTabFieldsCached = scope.nonTabFields(scope.displayables);

                    scope.$on(JavascriptEventConstants.ReevalDisplayables, () => {
                        //whenever a field is added via a dynforms need to recalculate the cached fields 
                        scope.nonTabFieldsCached = scope.nonTabFields(scope.displayables);
                    });

                    

                    scope.fieldMoved = () => {
                        //the nonTabFieldsCached array will be automatically updated by the dnd-list directive however the original displayables need to be updated here
                        scope.displayables = scope.nonTabFieldsCached;
                    }

                },

                controller: ["$q", "$scope", "$http","$rootScope", "$element", "$injector", "$timeout", "$log", "alertService",
                    "printService", "compositionService", "commandService", "fieldService", "i18NService",
                    "associationService", "expressionService", "styleService", "tabsService","focusService",
                    "cmpfacade", "cmpComboDropdown", "redirectService", "validationService", "contextService", "eventService", "formatService", "modalService", "dispatcherService",
                    "layoutservice", "attachmentService", "richTextService",
                    function ($q, $scope, $http, $rootScope, $element, $injector, $timeout, $log, alertService,
                        printService, compositionService, commandService, fieldService, i18NService,
                        associationService, expressionService, styleService, tabsService, focusService,
                        cmpfacade, cmpComboDropdown, redirectService, validationService, contextService, eventService, formatService, modalService, dispatcherService,
                        layoutservice, attachmentService, richTextService) {

                        $scope.$name = 'crud_input_fields';
                        $scope.lookupObj = {};
                        $scope.unWatcherArray = [];
                        //dictionary containing which details are or not expanded
                        $scope.expandeddetails = {};

                        //                    $scope.setForm = function (form) {
                        //                        $scope.crudform = form;
                        //                    };

                        $scope.$on('sw_block_association', function (event, association) {
                            $scope.blockedassociations[association] = true;
                        });

                        $scope.$on('sw_unblock_association', function (event, association) {
                            $scope.blockedassociations[association] = null;
                        });

                        $scope.getPanelId = function () {
                            return $scope.panelid || ($scope.ismodal === "true" ? "#modal" : null);
                        }

                        $scope.doubleClickDispatched = function ($event) {
                            $rootScope.$broadcast(JavascriptEventConstants.FormDoubleClicked, $event, false);
                            $event.stopImmediatePropagation();
                        }

                     


                        $scope.isPositionLeft = function (fieldMetadata) {
                            return "left".equalIc(fieldMetadata.rendererParameters['position']);
                        }

                     

                        $scope.$on(JavascriptEventConstants.AssociationUpdated, function (event, associationoptions) {
                            $scope.associationsloaded = true;
                            //                        if (!$scope.associationOptions) {
                            //                            //this in scenarios where a section is compiled before the association has returned from the server... angular seems to get lost in the bindings
                            //                            $scope.associationOptions = associationoptions;
                            //                        }
                        });

                        //$scope.$on("sw.modal.hide", function () {

                        //    if ($scope.ismodal === "true" && $scope.crudform) {
                        //        //fixing SWWEB-2660
                        //        $scope.crudform.$setPristine();
                        //    }
                        //});


                        //this will get called when the input form is done rendering
                        $scope.$on(JavascriptEventConstants.BodyRendered, function (ngRepeatFinishedEvent, parentElementId) {
                            eventService.onload($scope, $scope.schema, $scope.datamap, { tabid: crudContextHolderService.getActiveTab() });
                            const bodyElement = $('#' + parentElementId + "[schemaid=" + $scope.schema.schemaId + "]");
                            if (bodyElement.length <= 0) {
                                return;
                            }
                            // Configure tooltips
                            $('.no-touch [rel=tooltip]', bodyElement).tooltip({ container: 'body', trigger: 'hover' });


                            cmpfacade.init(bodyElement, $scope);
                            // workaround in order to make the <select> comboboxes work properly on ie9
                            angular.forEach($("select"), function (currSelect) {
                                if (currSelect.selectedIndex >= 0) {
                                    currSelect.options[currSelect.selectedIndex].text += " ";
                                }
                                $(currSelect).change(function () {
                                    if (this.selectedIndex >= 0) {
                                        this.options[this.selectedIndex].text += " ";
                                    }
                                });
                            });
                            //both ids refers to the main form, but crudInputMainCompositionFields when there are other tabs, or crudInputMainFields when there are no other tabs
                            // Fields refers to composition inline batch expand form
                            if (parentElementId.equalsAny('crudInputMainCompositionFields', 'crudInputMainFields', 'crudInputNewItemCompositionFields', 'Fields')) {
                                if ($scope.unWatcherArray) {
                                    $scope.unWatcherArray.forEach(function (fn) {
                                        //unregistering previously watches
                                        fn();
                                    });
                                }
                                //to avoid registering these global listeners multiple times, as the page main contain sections.
                                $scope.configureNumericInput();
                                $scope.configureOptionFields();
                                $scope.unWatcherArray = crud_inputcommons.configureAssociationChangeEvents($scope, 'datamap', $scope.displayables);
                                $scope.configureFieldChangeEvents();
                                //                    $scope.configureDirtyWatcher();
                            }
                            const datepickers = $('.datetimereadonly').data("DateTimePicker");
                            if (datepickers) {
                                datepickers.disable();
                            }
                        });

                        $scope.$on(JavascriptEventConstants.CrudSaved, () => {
                            const displayables = fieldService.getDisplayablesOfRendererTypes($scope.displayables, ["checkbox"]);
                            displayables.forEach(f => {
                                //refreshsing checkboxes so that the undo logic keeps working after a save takes place.
                                //Basically, need to update the originaldatamap with the formated value
                                $scope.initCheckbox(f);
                            });
                        });

                        $scope.browseFile = function ($event) {
                            $event.preventDefault();
                            $("#uploadInput").trigger("click");
                        };


                        /* Association (COMBO, AUTOCOMPLETECLIENT) functions */

                        $scope.haslookupModal = function (schema) {
                            return fieldService.getDisplayablesOfRendererTypes(schema.displayables, ['lookup']).length > 0;
                        }
                        $scope.isModifiableEnabled = function (fieldMetadata) {
                            const result = expressionService.evaluate(fieldMetadata.enableExpression, $scope.datamap);
                            return result;
                        };

                        $scope.isSelectEnabled = function (fieldMetadata) {
                            const key = fieldMetadata.associationKey;
                            $scope.disabledassociations = $scope.disabledassociations || {};
                            if (key == undefined) {
                                return true;
                            }
                            const result = ($scope.blockedassociations == null || !$scope.blockedassociations[key]) && expressionService.evaluate(fieldMetadata.enableExpression, $scope.datamap, $scope);
                            if (result != $scope.disabledassociations[key]) {
                                cmpfacade.blockOrUnblockAssociations($scope, !result, !$scope.disabledassociations[key], fieldMetadata);
                                $scope.disabledassociations[key] = result;
                            }
                            return result;
                        };

                        $scope.setMaxNumericInput = function (datamap, fieldMetadata) {
                            if (fieldMetadata.rendererParameters['max'] != null) {
                                return parseInt(expressionService.evaluate(fieldMetadata.rendererParameters['max'], datamap));
                            }
                            return null;
                        };


                      

                        $scope.restoreDefault = function (fieldMetadata) {
                            alertService.confirm("Are you sure you want to restore this field to its default value?").then(r => {
                                crudContextHolderService.rootDataMap($scope.panelId)[fieldMetadata.attribute] = crudContextHolderService.originalDatamap($scope.panelId)[fieldMetadata.attribute];
                            });
                        }

                        $scope.showUndoIcon = function (fieldMetadata) {
                            if (crudContextHolderService.isShowingModal()) {
                                return false;
                            }

                            if (fieldMetadata.isReadOnly || fieldMetadata.enableExpression === "false" || !crudContextHolderService.getDetailDataResolved()) {
                                return false;
                            }

//                            if (fieldMetadata.type === "ApplicationCompositionDefinition") {
//                                return false;
//                            }

                            const originalDatamap = crudContextHolderService.originalDatamap($scope.panelId);
                            const datamap = crudContextHolderService.rootDataMap($scope.panelId);
                            const value = datamap[fieldMetadata.attribute];
                            if (fieldMetadata.rendererType === "datetime") {
                                //check datetime.js

                                let originalUnformatted = `${fieldMetadata.attribute}_formatted`;
                                if (originalUnformatted[0] !== "#") {
                                    originalUnformatted = "#" + originalUnformatted;
                                }

                                if (!!originalDatamap[originalUnformatted]) {
                                    return !originalDatamap[originalUnformatted].equalIc(value);
                                }
                            }
                            const original = originalDatamap[fieldMetadata.attribute];
                            if (fieldMetadata.rendererType === "checkbox") {
                                return !booleanEquals(original, value);
                            }


                            if (fieldMetadata.type === "ApplicationCompositionDefinition" && fieldMetadata.inline) {
                                //disabling support as for now
                                //TODO: fix and fix of a decent way to do array comparison
                                return false;

//                                const arrayComparison = JSON.stringify(original) === JSON.stringify(value);
//
//
//                                if (!arrayComparison && !original && value === []) {
//                                    //ignoring differences for an undefined vs blank array
//                                    return false;
//                                }
                            }

                            return original !== value;

                        }

                        $scope.doToggleCheckBox = function (fieldMetadata, option, datamapKey) {

                            var model = $scope.datamap[datamapKey];
                            if (model == undefined) {
                                model = [];
                            }
                            if (datamapKey === "selectallHLAG") {
                                // This is wrong!!!!
                                $('option', '.multiselect').each(function (element) {
                                    $('.multiselect').multiselect('select', 'ADL');
                                });
                            }
                            const idx = model.indexOf(option.value);
                            if (idx > -1) {
                                model.splice(idx, 1);
                            } else {
                                model.push(option.value);
                            }
                            $scope.datamap[datamapKey] = model;
                            eventService.afterchange(fieldMetadata, { option });
                        }

                        $scope.toggleCheckboxSelection = function ($event, fieldMetadata, option, datamapKey) {
                            var beforeChain = eventService.beforechange(fieldMetadata, { fieldMetadata, option });

                            if (beforeChain && beforeChain.then) {
                                return beforeChain.then(() => {
                                    return $scope.doToggleCheckBox(fieldMetadata, option, datamapKey);
                                }).catch(r => {
                                    $event.preventDefault();
                                });
                            } else {
                                return $scope.doToggleCheckBox(fieldMetadata, option, datamapKey);
                            }
                        };

                        $scope.initCheckbox = function (fieldMetadata) {
                            if (fieldMetadata.type === "OptionField") {
                                //TODO: implement later
                                return;
                            }

                            const content = $scope.datamap[fieldMetadata.attribute];
                            const formattedValue = formatService.isChecked(content);
                            $scope.datamap[fieldMetadata.attribute] = formattedValue;
                            const originalDatamap = crudContextHolderService.originalDatamap($scope.panelid);
                            if (originalDatamap) {
                                originalDatamap[fieldMetadata.attribute] = formattedValue;
                            }
                        }

                        $scope.isChecked = function (fieldMetadata) {
                            const content = $scope.datamap[fieldMetadata.attribute];
                            const isChecked = formatService.isChecked(content);
                            $scope.datamap[fieldMetadata.attribute] = isChecked;
                            return isChecked;
                        }

                        /* LOOKUP functions */

                        $scope.getLookUpDescriptionLabel = function (fieldMetadata) {
                            return i18NService.getLookUpDescriptionLabel(fieldMetadata);
                        };

                        $scope.getOptionFieldLabelOrNull = function (fieldMetadata) {
                            if (fieldMetadata.type !== "OptionField") {
                                return null;
                            }
                            return fieldMetadata.label;
                        };

                        $scope.configureNumericInput = function () {
                            if (!$scope.datamap) return;
                            const displayables = fieldService.getDisplayablesOfRendererTypes($scope.displayables, ["numericinput"]);
                            angular.forEach(displayables, function (fieldMetadata) {
                                const currentValue = $scope.datamap[fieldMetadata.attribute];
                                if (currentValue == null) {
                                    /* without default value */
                                    $scope.datamap[fieldMetadata.attribute] = 1;
                                } else if (typeof currentValue === "string") {
                                    $scope.datamap[fieldMetadata.attribute] = parseInt(currentValue);
                                }
                            });
                        };

                        $scope.configureFieldChangeEvents = function () {
                            const fields = fieldService.getDisplayablesOfTypes($scope.displayables, ['ApplicationFieldDefinition']);
                            $.each(fields, function (key, field) {
                                var shouldDoWatch = true;
                                $scope.$watch('datamap["' + field.attribute + '"]', function (newValue, oldValue) {
                                    if (oldValue === newValue || !shouldDoWatch) {
                                        return;
                                    }
                                    const eventToDispatch = {
                                        oldValue: oldValue,
                                        newValue: newValue,
                                        fields: $scope.datamap,
                                        displayables: $scope.displayables,
                                        scope: $scope,
                                        'continue': function () {
                                            fieldService.postFieldChange(field, $scope, oldValue, newValue);
                                            try {
                                                $scope.$digest();
                                            } catch (ex) {
                                                //nothing to do, just checking if digest was already in place or not
                                            }
                                        },
                                        interrupt: function () {
                                            $scope.datamap[association.attribute] = oldValue;
                                            //to avoid infinite recursion here.
                                            shouldDoWatch = false;
                                            cmpfacade.digestAndrefresh(association, $scope);
                                            //turn it on for future changes
                                            shouldDoWatch = true;
                                        }
                                    };
                                    fieldService.onFieldChange(field, eventToDispatch);
                                    cmpfacade.digestAndrefresh(field, $scope);
                                });
                            });
                        };

                        $scope.configureOptionFields = function () {
                            const log = $log.get("crud_input_fields#configureOptions");
                            //TODO: check field parameter as well, with top priority before schema
                            if ($scope.schema.properties["optionfield.donotusefirstoptionasdefault"] === "true") {
                                return;
                            }
                            if ($scope.displayables == null || $scope.datamap == null) {
                                //no need to further check it
                                return;
                            }
                            const optionsFields = fieldService.getDisplayablesOfTypes($scope.displayables, ['OptionField']);
                            for (let i = 0; i < optionsFields.length; i++) {
                                const optionfield = optionsFields[i];
                                if (fieldService.isPropertyTrue(optionfield, "optionfield.donotusefirstoptionasdefault")) {
                                    log.debug("ignoring first value of field {0}".format(optionfield.associationKey));
                                    continue;
                                }
                                const shouldUseFirstOption = optionfield.providerAttribute == null || fieldService.isPropertyTrue(optionfield, "optionfield.forcefirstoption");
                                if ($scope.datamap[optionfield.target] == null && shouldUseFirstOption && optionfield.rendererType !== 'checkbox') {
                                    const values = $scope.GetOptionFieldOptions(optionfield);
                                    if (values != null && values.length > 0) {
                                        $scope.datamap[optionfield.target] = values[0].value;
                                    }
                                }
                            }
                        };
                        $scope.getSelectedTexts = function (fieldMetadata) {
                            return cmpComboDropdown.getSelectedTexts(fieldMetadata);
                        };

                        $scope.formatLabel = function (text, fieldMetadata) {
                            const maxLength = fieldMetadata.rendererParameters["labelmaxlength"];
                            if (!maxLength || text.length <= maxLength) {
                                return text;
                            }
                            return text.substring(0, maxLength) + "...";
                        }

                        $scope.opendetails = function (fieldMetadata) {
                            if ($scope.enabletoopendetails(fieldMetadata)) {
                                const parameters = { id: $scope.paramstopendetails.idtopendetails, popupmode: 'browser' };
                                redirectService.goToApplicationView($scope.paramstopendetails.application, 'detail', 'output', null, parameters);
                            }
                        };
                        $scope.fillparamstoopendetails = function (fieldMetadata) {
                            $scope.paramstopendetails = null;
                            if (!nullOrUndef(fieldMetadata.rendererParameters)) {
                                const idtopendetails = fieldMetadata.rendererParameters['idtopendetails'];
                                if (!nullOrUndef(fieldMetadata.applicationTo)) {
                                    const application = fieldMetadata.applicationTo.replace('_', '');
                                    const id = $scope.datamap[idtopendetails];
                                    if (!nullOrUndef(id) && !nullOrUndef(application)) {
                                        $scope.paramstopendetails = { idtopendetails: id, application: application };
                                    }
                                }
                            }
                        };

                        $scope.enabletoopendetails = function (fieldMetadata) {
                            $scope.fillparamstoopendetails(fieldMetadata);
                            return $scope.paramstopendetails == null ? false : true;
                        };
                        $scope.getLengthParam = function (fieldMetadata) {
                            var lengthclass = null;
                            if (!nullOrUndef(fieldMetadata.rendererParameters)) {
                                const length = fieldMetadata.rendererParameters['length'];
                                if (!nullOrUndef(length)) {
                                    switch (length) {
                                        case 'full':
                                            lengthclass = 'col-md-10';
                                            break;
                                        case 'half':
                                            lengthclass = 'col-md-5';
                                            break;
                                        case 'quarter':
                                            lengthclass = 'col-md-2';
                                            break;
                                    }
                                }
                            }
                            return lengthclass;
                        };

                        $scope.contextPath = function (path) {
                            return url(path);
                        };
                        $scope.isIE = function () {
                            return isIE();
                        };
                        $scope.getLabelStyle = function (fieldMetadata) {
                            const rendererColor = styleService.getLabelStyle(fieldMetadata, 'color');
                            const weight = styleService.getLabelStyle(fieldMetadata, 'font-weight');
                            const result = {
                                'color': rendererColor,
                                'font-weight': weight
                            };
                            return result;
                        }
                        $scope.showLabelTooltip = function (fieldMetadata) {
                            const helpIcon = $scope.getHelpIconPos(fieldMetadata);
                            if (fieldMetadata.label !== fieldMetadata.toolTip && (helpIcon == undefined || helpIcon === '')) {
                                return 'tooltip';
                            } else {
                                return '';
                            }
                        }

                        $scope.isVerticalOrientation = function () {
                            return $scope.orientation === 'vertical';
                        };
                        $scope.isSectionWithoutLabel = function (fieldMetadata) {
                            return fieldMetadata.type === 'ApplicationSection' && fieldMetadata.resourcepath == null && fieldMetadata.header == null;
                        };

                        $scope.isSectionControlEnabled = function (fieldMetadata) {
                            return fieldMetadata.type === 'ApplicationSection' && fieldMetadata.header !== null && fieldMetadata.attribute !== null && fieldMetadata.header.parameters["enablecontrol"] === "true";
                        }

                        $scope.hasPersistedFieldInsideSection = function (fieldMetadata) {
                            const originalDatamap = crudContextHolderService.originalDatamap();
                            const displayables = fieldService.getLinearDisplayables(fieldMetadata);
                            fieldMetadata.jscache = fieldMetadata.jscache || {};
                            if (fieldMetadata.jscache.hasPersistedFieldInsideSection === true) {
                                return true;
                            }

                            for (var i = 0; i < displayables.length; i++) {
                                const displayable = displayables[i];
                                if (!!displayable.associationKey && !!originalDatamap[displayable.associationKey]) {
                                    if (Array.isArray(originalDatamap[displayable.associationKey])) {
                                        return originalDatamap[displayable.associationKey].length > 0;
                                    }
                                    return true;
                                }

                                if (!!originalDatamap[displayable.attribute]) {
                                    return true;
                                }
                            }

                            return false;

                        }

                        $scope.toggleSectionSelection = function (fieldMetadata) {
                            $scope.datamap[fieldMetadata.attribute] = !$scope.datamap[fieldMetadata.attribute];
                        }

                        $scope.isExpansionAvailable = function (fieldMetadata) {
                            if (!fieldService.isAssociation(fieldMetadata) || !fieldMetadata.detailSection) {
                                //if there are no details, there�s nothing at all to expand
                                return false;
                            }
                            const key = fieldMetadata.associationKey;
                            if (!$scope.datamap[key]) {
                                //if the item is not yet selected it should not be shown
                                return false;
                            }
                            return true;
                        }

                        $scope.toggleDetailExpansion = function (fieldMetadata) {
                            if (!fieldService.isAssociation(fieldMetadata) || !fieldMetadata.detailSection) {
                                return;
                            }
                            const key = fieldMetadata.associationKey;
                            $scope.expandeddetails[key] = !$scope.expandeddetails[key];
                        }

                        $scope.detailsExpanded = function (fieldMetadata) {
                            if (!this.isExpansionAvailable(fieldMetadata)) {
                                return false;
                            }
                            const key = fieldMetadata.associationKey;
                            if (!$scope.expandeddetails[key]) {
                                $scope.expandeddetails[key] = false;
                            }
                            return $scope.expandeddetails[key];
                        }

                        $scope.isOnDynFormEdition = function() {
                            return $scope.schema.properties["dynforms.editionallowed"] === "true";
                        }

                        $scope.hasSameLineLabel = function (fieldMetadata) {


                            return (fieldMetadata.header != null && fieldMetadata.header.displacement !== 'ontop') ||
                                (fieldMetadata.header == null);

                        };

                        $scope.sectionHasSameLineLabel = function (fieldMetadata) {
                            return $scope.hasSameLineLabel(fieldMetadata) && fieldMetadata.type === 'ApplicationSection' && fieldMetadata.resourcepath == null;
                        };

                        $scope.nonTabFields = function (displayables) {
                            return fieldService.nonTabFields(displayables);
                        };

                        $scope.getApplicationPath = function (fieldMetadata) {
                            return replaceAll(fieldMetadata.applicationPath, "\\.", "_");
                        }

                        $scope.getDispatchFn = function (serviceCall) {
                            if (serviceCall == null) {
                                return () => true;
                            }

                            var validationFunction = dispatcherService.loadServiceByString(serviceCall);
                            if (validationFunction == null) {
                                validationFunction = () => true;
                            }
                            return validationFunction;
                        };

                        $scope.uploadAcceptedFiles = function (fieldMetadata) {
                            if (!fieldMetadata || !fieldMetadata.rendererParameters || !fieldMetadata.rendererParameters.acceptedfiles) {
                                return ".jpg,.bmp,.png,.pdf,.zip,text/plain,.doc,.docx,.dwg,.csv,.xls,.xlsx,.ppt,application/xml,.xsl,text/html";
                            }
                            return fieldMetadata.rendererParameters.acceptedfiles;
                        }

                        function init() {
                            if (!$scope.isVerticalOrientation()) {
                                const countVisibleDisplayables = fieldService.countVisibleDisplayables($scope.datamap, $scope.schema, $scope.displayables);
                                if (countVisibleDisplayables > 0) {
                                    $scope.horizontalWidth = {
                                        width: (100 / countVisibleDisplayables) + "%"
                                    };
                                }
                            }
                            $injector.invoke(BaseController, this, {
                                $scope: $scope,
                                i18NService: i18NService,
                                fieldService: fieldService,
                                formatService: formatService,
                                layoutservice: layoutservice,
                                expressionService: expressionService
                            });
                        }
                        init();

                        $scope.initField = function (fieldMetadata) {
                            crud_inputcommons.initField($scope, fieldMetadata, "datamap");
                        };

                        $scope.blurField = function ($event, fieldMetadata) {
//                            console.log($event);
//                            $event.currentTarget.blur();
                            $event.currentTarget.blur();
                            var result = focusService.moveFocus($scope.datamap, $scope.schema, fieldMetadata.attribute);
//                            if (!result) {
//                                
//                            }

//                            var fields = $(this).parents('form:eq(0),body').find('input, textarea, select');
//                            var index = fields.index(this);
//                            if (index > -1 && (index + 1) < fields.length)
//                                fields.eq(index + 1).focus();

                        }

                        $scope.getValidationPattern = function (fieldMetadata) {
                            return validationService.getValidationPattern(fieldMetadata);
                        };

                        $scope.getValidationPatterString = function (fieldMetadata) {
                            return validationService.getValidationPatterString(fieldMetadata);
                        };

                        $scope.getHelpIconPos = function (fieldMetadata) {
                            return fieldMetadata.header ? fieldMetadata.header.helpIcon : fieldMetadata.helpIcon;
                        };

                        $scope.showHelpIcon = function (fieldMetadata, position) {
                            const helpIconPosition = $scope.getHelpIconPos(fieldMetadata);
                            return (helpIconPosition != null && helpIconPosition != '' && helpIconPosition === position);
                        };

                        $scope.helpToolTip = function (fieldMetadata) {
                            return fieldMetadata.header ? fieldMetadata.header.toolTip : fieldMetadata.toolTip;
                        };

                        $scope.initRichtextField = function (fieldMetadata) {
                            const content = $scope.datamap[fieldMetadata.attribute];
                            $scope.datamap[fieldMetadata.attribute] = richTextService.getDecodedValue(content);
                        }

                        $scope.isMobile = function () {
                            return isMobile();
                        };

                        $scope.isFieldRequired = function (fieldMetadata) {
                            return fieldService.isFieldRequired(fieldMetadata, $scope.datamap);
                        };

                        $scope.codeEditorMode = function (fieldMetadata) {
                            return fieldMetadata.rendererParameters ? fieldMetadata.rendererParameters["codemode"] : null;
                        }

                        $scope.isTabSection = function (sectionMetadata) {
                            if (!sectionMetadata) {
                                let isTabSection = $scope.tabsection === "true";
                                return isTabSection;
                            }
                            let isTabSection = sectionMetadata.rendererParameters["tabsection"] === "true";
                            return isTabSection;
                        }

                        $scope.tabsDisplayables = function (displayables) {
                            return tabsService.filterTabs(displayables);
                        };
                    }]
            }
        }])
        .directive('numberSpinner', function () {
            return {
                restrict: 'A',
                scope: {
                    //Binds the ngModel attribute from the element which has the number-spinner tag
                    ngModel: '=ngModel'
                },
                link: function (scope, element, attr) {
                    $(element).spinner({
                        change: function (event, ui) {
                            $(element).change();
                        },
                        min: attr.min,
                        max: attr.max
                    });
                    //Binds the mousewheel event to the below function.
                    $(element).bind("mousewheel", function (event, delta) {
                        if (delta > 0) {
                            /*Updates the ngModel, which is bound as an attribute to 
                            the crud_input_field's input element for number-spinners*/
                            scope.ngModel = parseInt(this.value);
                        } else {
                            if (parseInt(this.value) > 0) {
                                scope.ngModel = parseInt(this.value);
                            }
                        }
                        scope.$digest();

                        return false;
                    });
                }
            }
        })
        .directive('selectCombo', function () {
            return {
                restrict: 'A',
                link: function (scope, element, attr) {
                    $(element).on('click', 'input', function (e) {
                        //console.log('click');
                        $(element).find('[data-dropdown="dropdown"]').click();
                        //return false;
                        //e.stopPropagation();
                    });

                }
            };
        });

})(app, angular);
