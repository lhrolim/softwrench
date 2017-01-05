(function (app, angular) {
    "use strict";


    app.directive('configAssociationListInputDatamap', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.$first) {
                    scope.datamap[scope.fieldMetadata.attribute] = [];
                }
                var item = {};
                var displayables = scope.associationSchemas[scope.fieldMetadata.associationKey].displayables;
                angular.forEach(displayables, function (displayable) {
                    var attribute = displayable.attribute;
                    item[attribute] = scope.option.extrafields[attribute];
                });
                scope.datamap[scope.fieldMetadata.attribute].push(item);
            }
        };
    })
    .directive("configUpdateSectionDatamap", function () {
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
                datamap: '=',
                isDirty: '=',
                displayables: '=',
                associationSchemas: '=',
                blockedassociations: '=',
                extraparameters: '=',
                elementid: '@',
                orientation: '@',
                islabelless: '@',
                ismodal: '@',
                lookupAssociationsCode: '=',
                lookupAssociationsDescription: '=',
                rendererParameters: '='
            },
            template: "<div></div>",
            link: function (scope, element, attrs) {
                if (angular.isArray(scope.displayables)) {
                    element.append(
                    "<crud-input-fields displayables='displayables'" +
                    "schema='schema'" +
                    "datamap='datamap'" +
                    "is-dirty='isDirty'" +
                    "ismodal = '{{ismodal}}'" +
                    "displayables='displayables'" +
                    "association-schemas='associationSchemas'" +
                    "blockedassociations='blockedassociations'" +
                    "section-parameters='rendererParameters'" +
                    "elementid='{{elementid}}'" +
                    "orientation='{{orientation}}' insidelabellesssection='{{islabelless}}'" +
                    "outerassociationcode='lookupAssociationsCode' outerassociationdescription='lookupAssociationsDescription' issection='true'" +
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
                associationSchemas: '=',
                blockedassociations: '=',
                elementid: '@',
                orientation: '@',
                insidelabellesssection: '@',
                previousdata: '=',
                previousschema: '=',
                parentdata: '=',
                parentschema: '=',
                outerassociationcode: '=',
                sectionParameters: '=',
                outerassociationdescription: '=',
                issection: '@',
                ismodal: '@',
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
            },

            controller: ["$scope", "$http", "$element", "$injector", "$timeout", "$log",
                "printService", "compositionService", "commandService", "fieldService", "i18NService",
                "associationService", "expressionService", "styleService",
                "cmpfacade", "cmpComboDropdown", "redirectService", "validationService", "contextService", "eventService", "formatService", "modalService", "dispatcherService", 
                "layoutservice", "attachmentService", "richTextService",
                function ($scope, $http, $element, $injector, $timeout, $log,
                printService, compositionService, commandService, fieldService, i18NService,
                associationService, expressionService, styleService,
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
                        //TODO: pass whole panelid
                        return $scope.ismodal === "true" ? "#modal" : null;
                    }

                    $scope.getCheckboxOptions = function (fieldMetadata) {
                        if (fieldMetadata.providerAttribute == null) {
                            return fieldMetadata.options;
                        }
                        var contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;
                        return crudContextHolderService.fetchEagerAssociationOptions(fieldMetadata.associationKey, contextData, $scope.panelid);
                    }
                    $scope.isPositionLeft = function (fieldMetadata) {
                        return "left".equalIc(fieldMetadata.rendererParameters['position']);
                    }



                    $scope.$on(JavascriptEventConstants.AssociationUpdated, function (event, associationoptions) {
                        $scope.associationsloaded = true;
                        if (!$scope.associationOptions) {
                            //this in scenarios where a section is compiled before the association has returned from the server... angular seems to get lost in the bindings
                            $scope.associationOptions = associationoptions;
                        }
                    });

                    //$scope.$on("sw.modal.hide", function () {

                    //    if ($scope.ismodal === "true" && $scope.crudform) {
                    //        //fixing SWWEB-2660
                    //        $scope.crudform.$setPristine();
                    //    }
                    //});


                    //this will get called when the input form is done rendering
                    $scope.$on(JavascriptEventConstants.BodyRendered, function (ngRepeatFinishedEvent, parentElementId) {
                        eventService.onload($scope.schema, $scope.datamap);
                        var bodyElement = $('#' + parentElementId + "[schemaid=" + $scope.schema.schemaId + "]");
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
                        //
                        if (parentElementId.equalsAny('crudInputMainCompositionFields', 'crudInputMainFields', 'crudInputNewItemCompositionFields')) {
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
                        var datepickers = $('.datetimereadonly').data("DateTimePicker");
                        if (datepickers) {
                            datepickers.disable();
                        }
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
                        var result = expressionService.evaluate(fieldMetadata.enableExpression, $scope.datamap);
                        return result;
                    };

                    $scope.isSelectEnabled = function (fieldMetadata) {
                        var key = fieldMetadata.associationKey;
                        $scope.disabledassociations = $scope.disabledassociations || {};
                        if (key == undefined) {
                            return true;
                        }
                        var result = ($scope.blockedassociations == null || !$scope.blockedassociations[key]) && expressionService.evaluate(fieldMetadata.enableExpression, $scope.datamap, $scope);
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


                    /* CHECKBOX functions */
                    $scope.isCheckboxSelected = function (option, datamapKey) {
                        var model = $scope.datamap[datamapKey];
                        if (model == undefined) {
                            return false;
                        }
                        return model.indexOf(option.value) > -1;
                    };
                    $scope.toggleCheckboxSelection = function (option, datamapKey) {
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
                        var idx = model.indexOf(option.value);
                        if (idx > -1) {
                            model.splice(idx, 1);
                        } else {
                            model.push(option.value);
                        }
                        $scope.datamap[datamapKey] = model;
                    };
                    $scope.initCheckbox = function (fieldMetadata) {
                        var content = $scope.datamap[fieldMetadata.attribute];
                        $scope.datamap[fieldMetadata.attribute] = formatService.isChecked(content);
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

                    $scope.configureNumericInput = function () {
                        if (!$scope.datamap) return;
                        var displayables = fieldService.getDisplayablesOfRendererTypes($scope.displayables, ["numericinput"]);
                        angular.forEach(displayables, function (fieldMetadata) {
                            var currentValue = $scope.datamap[fieldMetadata.attribute];
                            if (currentValue == null) {
                                /* without default value */
                                $scope.datamap[fieldMetadata.attribute] = 1;
                            } else if (typeof currentValue === "string") {
                                $scope.datamap[fieldMetadata.attribute] = parseInt(currentValue);
                            }
                        });
                    };

                    $scope.configureFieldChangeEvents = function () {
                        var fields = fieldService.getDisplayablesOfTypes($scope.displayables, ['ApplicationFieldDefinition']);
                        $.each(fields, function (key, field) {
                            var shouldDoWatch = true;
                            $scope.$watch('datamap["' + field.attribute + '"]', function (newValue, oldValue) {
                                if (oldValue === newValue || !shouldDoWatch) {
                                    return;
                                }

                                var eventToDispatch = {
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
                        var log = $log.get("crud_input_fields#configureOptions");
                        //TODO: check field parameter as well, with top priority before schema
                        if ($scope.schema.properties["optionfield.donotusefirstoptionasdefault"] === "true") {
                            return;
                        }
                        if ($scope.displayables == null || $scope.datamap == null) {
                            //no need to further check it
                            return;
                        }
                        var optionsFields = fieldService.getDisplayablesOfTypes($scope.displayables, ['OptionField']);
                        for (var i = 0; i < optionsFields.length; i++) {
                            var optionfield = optionsFields[i];

                            if (fieldService.isPropertyTrue(optionfield, "optionfield.donotusefirstoptionasdefault")) {
                                log.debug("ignoring first value of field {0}".format(optionfield.associationKey));
                                continue;
                            }
                            var shouldUseFirstOption = optionfield.providerAttribute == null || fieldService.isPropertyTrue(optionfield, "optionfield.forcefirstoption");

                            if ($scope.datamap[optionfield.target] == null && shouldUseFirstOption && optionfield.rendererType !== 'checkbox') {
                                var values = $scope.GetOptionFieldOptions(optionfield);
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
                        var maxLength = fieldMetadata.rendererParameters["labelmaxlength"];
                        if (!maxLength || text.length <= maxLength) {
                            return text;
                        }
                        return text.substring(0, maxLength) + "...";
                    }

                    $scope.opendetails = function (fieldMetadata) {
                        if ($scope.enabletoopendetails(fieldMetadata)) {
                            var parameters = { id: $scope.paramstopendetails.idtopendetails, popupmode: 'browser' };
                            redirectService.goToApplicationView($scope.paramstopendetails.application, 'detail', 'output', null, parameters);
                        }
                    };
                    $scope.fillparamstoopendetails = function (fieldMetadata) {
                        $scope.paramstopendetails = null;
                        if (!nullOrUndef(fieldMetadata.rendererParameters)) {
                            var idtopendetails = fieldMetadata.rendererParameters['idtopendetails'];
                            if (!nullOrUndef(fieldMetadata.applicationTo)) {
                                var application = fieldMetadata.applicationTo.replace('_', '');
                                var id = $scope.datamap[idtopendetails];
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
                            var length = fieldMetadata.rendererParameters['length'];
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
                        var rendererColor = styleService.getLabelStyle(fieldMetadata, 'color');
                        var weight = styleService.getLabelStyle(fieldMetadata, 'font-weight');
                        var result = {
                            'color': rendererColor,
                            'font-weight': weight
                        }
                        return result;
                    }
                    $scope.showLabelTooltip = function (fieldMetadata) {
                        var helpIcon = $scope.getHelpIconPos(fieldMetadata);
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

                    $scope.isExpansionAvailable = function (fieldMetadata) {
                        if (!fieldService.isAssociation(fieldMetadata) || !fieldMetadata.detailSection) {
                            //if there are no details, there�s nothing at all to expand
                            return false;
                        }
                        var key = fieldMetadata.associationKey;
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
                        var key = fieldMetadata.associationKey;
                        $scope.expandeddetails[key] = !$scope.expandeddetails[key];
                    }

                    $scope.detailsExpanded = function (fieldMetadata) {
                        if (!this.isExpansionAvailable(fieldMetadata)) {
                            return false;
                        }
                        var key = fieldMetadata.associationKey;
                        if (!$scope.expandeddetails[key]) {
                            $scope.expandeddetails[key] = false;
                        }
                        return $scope.expandeddetails[key];
                    }


                    $scope.hasSameLineLabel = function (fieldMetadata) {


                        return (fieldMetadata.header != null && fieldMetadata.header.displacement !== 'ontop') ||
                        (fieldMetadata.header == null);

                    };

                    $scope.associationOptionsToStringArray = function (fieldMetadata) {
                        if (!$scope.associationsloaded) {
                            return [];
                        }

                        $scope.schema.jscache = $scope.schema.jscache || {};

                        var cacheKey = fieldMetadata.associationKey + "stringarraycache";
                        if ($scope.schema.jscache[cacheKey]) {
                            return $scope.schema.jscache[cacheKey];
                        }

                        var options = $scope.associationOptions[fieldMetadata.associationKey];

                        var strArr = new Array();
                        for (var option in options) {
                            if (!options.hasOwnProperty(option)) {
                                continue;
                            }
                            strArr.push(options[option].value);
                        }
                        $scope.schema.jscache[cacheKey] = strArr;
                        return strArr;
                    }

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

                    function init() {
                        if (!$scope.isVerticalOrientation()) {
                            var countVisibleDisplayables = fieldService.countVisibleDisplayables($scope.datamap, $scope.schema, $scope.displayables);
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

                    $scope.getValidationPattern = function (type) {
                        return validationService.getValidationPattern(type);
                    };

                    $scope.getHelpIconPos = function (fieldMetadata) {
                        return fieldMetadata.header ? fieldMetadata.header.helpIcon : fieldMetadata.helpIcon;
                    };

                    $scope.showHelpIcon = function (fieldMetadata, position) {
                        var helpIconPosition = $scope.getHelpIconPos(fieldMetadata);                        
                        return (helpIconPosition != null && helpIconPosition != '' && helpIconPosition === position);
                    };

                    $scope.helpToolTip = function (fieldMetadata) {
                        return fieldMetadata.header ? fieldMetadata.header.toolTip : fieldMetadata.toolTip;
                    };

                    $scope.initRichtextField = function (fieldMetadata) {
                        var content = $scope.datamap[fieldMetadata.attribute];
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
