app.directive('configAssociationListInputDatamap', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$first) {
                scope.datamap[scope.fieldMetadata.attribute] = [];
            }
            var item = {};
            var displayables = scope.associationSchemas[scope.fieldMetadata.associationKey].displayables;
            for (i = 0; i < displayables.length; i++) {
                var attribute = displayables[i].attribute;
                item[attribute] = scope.option.extrafields[attribute];
            }
            scope.datamap[scope.fieldMetadata.attribute].push(item);
        }
    };
});
app.directive('configUpdateSectionDatamap', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$first) {
                scope.datamap[scope.fieldMetadata.id] = [];
            }
            var item = {};
            item["label"] = scope.i18NLabel(scope.field);
            item["value"] = scope.datamap[scope.field.attribute];
            scope.$watch('datamap["' + scope.field.attribute + '"]', function (newValue, oldValue) {
                if (oldValue == newValue) {
                    return;
                }
                scope.datamap[scope.fieldMetadata.id][scope.$index]["value"] = newValue;
            });
            item["#newvalue"] = '';
            scope.datamap[scope.fieldMetadata.id].push(item);
        }
    };
});
app.directive('sectionElementInput', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            schema: '=',
            datamap: '=',
            isDirty: '=',
            displayables: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            extraparameters: '=',
            elementid: '@',
            orientation: '@',
            islabelless: '@',
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',

        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {
                element.append(
                "<crud-input-fields displayables='displayables'" +
                "schema='schema'" +
                "datamap='datamap'" +
                "is-dirty='isDirty'" +
                "displayables='displayables'" +
                "association-options='associationOptions'" +
                "association-schemas='associationSchemas'" +
                "blockedassociations='blockedassociations'" +
                "elementid='{{elementid}}'" +
                "orientation='{{orientation}}' insidelabellesssection='{{islabelless}}'" +
                "outerassociationcode='lookupAssociationsCode' outerassociationdescription='lookupAssociationsDescription' issection='true'" +
                "></crud-input-fields>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});
app.directive('crudInputFields', function (contextService) {
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
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            elementid: '@',
            orientation: '@',
            insidelabellesssection: '@',
            previousdata: '=',
            previousschema: '=',
            outerassociationcode: '=',
            outerassociationdescription: '=',
            issection: '@'
        },

        link: function (scope, element, attrs) {
            if (scope.outerassociationcode && scope.issection == "true") {
                //this is for sections, we receive it as parameters
                scope.lookupAssociationsCode = scope.outerassociationcode;
                scope.lookupAssociationsDescription = scope.outerassociationdescription;
            } else {
                scope.lookupAssociationsCode = {};
                scope.lookupAssociationsDescription = {};
            }

            
        },

        controller: function ($scope, $http, $element, $injector, $timeout,
            printService, compositionService, commandService, fieldService, i18NService,
            associationService, expressionService, styleService,
            cmpfacade, cmpComboDropdown, redirectService, validationService, contextService, eventService, formatService) {
            $scope.$name = 'crud_input_fields';
            $scope.handlerTitleInputFile = function (cssclassaux) {
                var title = $scope.i18N('attachment.' + cssclassaux, 'No file selected');
                var fileInput = $('.' + cssclassaux);
                $().ready(function () {
                    fileInput.change(function () {
                        var titleaux = title;
                        if (fileInput != undefined && fileInput.val() != '') {
                            title = fileInput.val();
                        }
                        fileInput.attr('title', title);
                        title = titleaux;
                    });
                });
                return title;
            };
            $scope.getCheckboxOptions = function (fieldMetadata) {
                if (fieldMetadata.providerAttribute == null) {
                    return fieldMetadata.options;
                }
                return associationOptions[fieldMetadata.associationKey];
            }
            $scope.isPositionLeft = function (fieldMetadata) {
                return "left".equalIc(fieldMetadata.rendererParameters['position']);
            }
            $scope.$on('sw_associationsupdated', function (event, associationoptions) {
                //this in scenarios where a section is compiled before the association has returned from the server... angular seems to get lost in the bindings
                $scope.associationOptions = associationoptions;
            });
            //this will get called when the input form is done rendering
            $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent, parentElementId) {
                eventService.onload($scope.schema, $scope.datamap);
                var bodyElement = $('#' + parentElementId);
                if (bodyElement.length <= 0) {
                    return;
                }
                // Configure tooltips
                $('.no-touch [rel=tooltip]', bodyElement).tooltip({ container: 'body' });


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
                if (parentElementId.equalsAny('crudInputMainCompositionFields', 'crudInputMainFields')) {
                    //to avoid registering these global listeners multiple times, as the page main contain sections.
                    $scope.configureNumericInput();
                    $scope.configureOptionFields();
                    $scope.configureAssociationChangeEvents();
                    $scope.configureFieldChangeEvents();

                    $scope.configureDirtyWatcher();
                }
                $('.datetimereadonly').datepicker("remove");
                // Configure input files
                $('#uploadBtn').on('change', function (e) {
                    if (this != null) {
                        $('#uploadFile').attr("value", this.value);
                    }
                });
            });
            $scope.configureDirtyWatcher = function () {
                $timeout(function () {
                    $scope.$watch('datamap', function (newValue, oldValue) {
                        if (newValue != oldValue) {
                            validationService.setDirty();
                        }
                    }, true);
                }, 0, false);
            }
            /* Association (COMBO, AUTOCOMPLETECLIENT) functions */
            $scope.configureAssociationChangeEvents = function () {
                var associations = fieldService.getDisplayablesOfTypes($scope.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
                $.each(associations, function (key, association) {
                    var shouldDoWatch = true;
                    var isMultiValued = association.multiValued;
                    $scope.$watch('datamap["' + association.attribute + '"]', function (newValue, oldValue) {
                        if (oldValue == newValue || !shouldDoWatch) {
                            return;
                        }
                        if (newValue != null) {
                            //this is a hacky thing when we want to change a value of a field without triggering the watch
                            var ignoreWatchIdx = newValue.indexOf('$ignorewatch');
                            if (ignoreWatchIdx != -1) {
                                shouldDoWatch = false;
                                $scope.datamap[association.attribute] = newValue.substring(0, ignoreWatchIdx);
                                try {
                                    $scope.$digest();
                                    shouldDoWatch = true;
                                } catch (e) {
                                    //nothing to do, just checking if digest was already in place or not
                                    $timeout(function () {
                                        shouldDoWatch = true;
                                    }, 0, false);
                                }
                                return;
                            }
                        }
                        var eventToDispatch = {
                            oldValue: oldValue,
                            newValue: newValue,
                            fields: $scope.datamap,
                            displayables: $scope.displayables,
                            scope: $scope,
                            'continue': function () {
                                if (isMultiValued && association.rendererType != 'lookup') {
                                    associationService.updateUnderlyingAssociationObject(association, null, $scope);
                                }
                                var result = associationService.updateAssociations(association, $scope);
                                if (result != undefined && result == false) {
                                    var resolved = contextService.fetchFromContext("associationsresolved", false, true);
                                    var phase = resolved ? 'configured' : 'initial';
                                    var dispatchedbytheuser = $scope.associationsResolved ? true : false;
                                    associationService.postAssociationHook(association, $scope, { phase: phase, dispatchedbytheuser: dispatchedbytheuser });
                                }
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
                        associationService.onAssociationChange(association, isMultiValued, eventToDispatch);
                        cmpfacade.digestAndrefresh(association, $scope);
                    });
                    $scope.$watchCollection('associationOptions.' + association.associationKey, function (newvalue, old) {
                        if (newvalue == old) {
                            return;
                        }
                        $timeout(
                        function () {
                            cmpfacade.digestAndrefresh(association, $scope);
                        }, 0, false);
                    });
                    $scope.$watch('blockedassociations.' + association.associationKey, function (newValue, oldValue) {
                        cmpfacade.blockOrUnblockAssociations($scope, newValue, oldValue, association);
                    });
                });
            }
            $scope.isSelectEnabled = function (fieldMetadata) {
                var key = fieldMetadata.associationKey;
                $scope.disabledassociations = instantiateIfUndefined($scope.disabledassociations);
                if (key == undefined) {
                    return true;
                }
                var result = ($scope.blockedassociations == null || !$scope.blockedassociations[key]) && expressionService.evaluate(fieldMetadata.enableExpression, $scope.datamap);
                if (result != $scope.disabledassociations[key]) {
                    cmpfacade.blockOrUnblockAssociations($scope, !result, !$scope.disabledassociations[key], fieldMetadata);
                    $scope.disabledassociations[key] = result;
                }
                return result;
            };
            $scope.haslookupModal = function (schema) {
                return fieldService.getDisplayablesOfRendererTypes(schema.displayables, ['lookup']).length > 0;
            }
            $scope.isModifiableEnabled = function (fieldMetadata) {
                var result = expressionService.evaluate(fieldMetadata.enableExpression, $scope.datamap);
                return result;
            };

            $scope.setMaxNumericInput = function (datamap, fieldMetadata) {
                if (fieldMetadata.rendererParameters['max'] != null) {
                    return parseInt(expressionService.evaluate(fieldMetadata.rendererParameters['max'],datamap));
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
            $scope.toogleCheckboxSelection = function (option, datamapKey) {
                var model = $scope.datamap[datamapKey];
                if (model == undefined) {
                    model = [];
                }
                if (datamapKey == "selectallHLAG") {
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
            /* LOOKUP functions */
        

            $scope.showLookupModal = function (fieldMetadata) {
                if (!$scope.isSelectEnabled(fieldMetadata)) {
                    return;
                }
                $scope.lookupModalSearch = {};
                $scope.lookupModalSearch.descripton = '';
                $scope.lookupModalSearch.fieldMetadata = fieldMetadata;
                var targetValue = $scope.datamap[fieldMetadata.target];
                if (targetValue == null || targetValue == " ") {
                    $scope.lookupModalSearch.code = $scope.lookupAssociationsCode[fieldMetadata.attribute];
                } else {
                    $scope.lookupModalSearch.code = '';
                }
                var modals = $('[data-class="lookupModal"]', $element);
                modals.draggable();
                modals.modal('show');
            };
            $scope.lookupCodeChange = function (fieldMetadata) {
                if ($scope.datamap[fieldMetadata.target] != null) {
                    $scope.datamap[fieldMetadata.target] = " "; // If the lookup value is changed to a null value, set a white space, so it can be updated on maximo WS.
                    $scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
                    associationService.updateUnderlyingAssociationObject(fieldMetadata, null, $scope);
                }
            };
            $scope.getLookUpDescriptionLabel = function (fieldMetadata) {
                return i18NService.getLookUpDescriptionLabel(fieldMetadata);
            };
            $scope.lookupCodeBlur = function (fieldMetadata) {
                var code = $scope.lookupAssociationsCode[fieldMetadata.attribute];
                var targetValue = $scope.datamap[fieldMetadata.target];
                if (code != null && code != '' && (targetValue == null || targetValue == " ")) {
                    $scope.showLookupModal(fieldMetadata);
                }
            };
            $scope.configureNumericInput = function () {
                for (i in $scope.schema.displayables) {
                    var fieldMetadata = $scope.schema.displayables[i];
                    if (fieldMetadata.rendererType != 'numericinput') {
                        continue;
                    }
                    if ($scope.datamap != null) {
                        var currentValue = $scope.datamap[fieldMetadata.attribute];
                        if (currentValue == null) /* without default value */ {
                            $scope.datamap[fieldMetadata.attribute] = 1;
                        } else if (typeof currentValue == "string") {
                            $scope.datamap[fieldMetadata.attribute] = parseInt(currentValue);
                        }
                    }
                }
            };

            $scope.configureFieldChangeEvents = function () {
                var fields = fieldService.getDisplayablesOfTypes($scope.displayables, ['ApplicationFieldDefinition']);
                $.each(fields, function (key, field) {
                    var shouldDoWatch = true;
                    $scope.$watch('datamap["' + field.attribute + '"]', function (newValue, oldValue) {
                        if (oldValue == newValue || !shouldDoWatch) {
                            return;
                        }

                        var eventToDispatch = {
                            oldValue: oldValue,
                            newValue: newValue,
                            fields: $scope.datamap,
                            displayables: $scope.displayables,
                            scope: $scope,
                            'continue': function () {
                                fieldService.postFieldChange(field, $scope);
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
                //TODO: check field parameter as well, with top priority before schema
                if ($scope.schema.properties["optionfield.donotusefirstoptionasdefault"] == "true") {
                    return;
                }
                if ($scope.displayables != null && $scope.datamap != null) {
                    var optionsFields = fieldService.getDisplayablesOfTypes($scope.displayables, ['OptionField']);
                    for (var i = 0; i < optionsFields.length; i++) {
                        var optionfield = optionsFields[i];
                        if ($scope.datamap[optionfield.target] == null && optionfield.providerAttribute == null && optionfield.rendererType != 'checkbox') {
                            var values = $scope.GetOptionFieldOptions(optionfield);
                            if (values != null) {
                                $scope.datamap[optionfield.target] = values[0].value;
                            }
                        }
                    }
                }
            };
            $scope.getSelectedTexts = function (fieldMetadata) {
                return cmpComboDropdown.getSelectedTexts(fieldMetadata);
            };
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

            $scope.getFormattedValue = function (value, field, datamap) {
                var formattedValue = formatService.format(value, field, datamap);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return null;
                }
                return formattedValue;
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
            $scope.GetAssociationOptions = function (fieldMetadata) {
                if (fieldMetadata.type == "OptionField") {
                    return $scope.GetOptionFieldOptions(fieldMetadata);
                }
                $scope.associationOptions = instantiateIfUndefined($scope.associationOptions);
                return $scope.associationOptions[fieldMetadata.associationKey];
            }
            $scope.GetOptionFieldOptions = function (optionField) {
                if (optionField.providerAttribute == null) {
                    return optionField.options;
                }
                $scope.associationOptions = instantiateIfUndefined($scope.associationOptions);
                return $scope.associationOptions[optionField.providerAttribute];
            }
            $scope.contextPath = function (path) {
                return url(path);
            };
            $scope.isIE = function () {
                //TODO: is this needed for all ieversions or only 9 and, in this case replace function for aa_utils
                return isIe9();
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
            $scope.getFieldClass = function (fieldMetadata) {
                var cssclass = "";

                if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters['fieldclass'] != null) {
                    cssclass += fieldMetadata.rendererParameters['fieldclass'];
                } else {
                    if (fieldMetadata.schema != null &&
                    fieldMetadata.schema.rendererParameters != null &&
                    fieldMetadata.schema.rendererParameters['fieldclass'] != null) {
                        cssclass += fieldMetadata.schema.rendererParameters['fieldclass'];
                    }
                }

                return cssclass;
            }
            $scope.getLabelClass = function (fieldMetadata) {
                var cssclass = "";

                if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters['labelclass'] != null) {
                    cssclass += fieldMetadata.rendererParameters['labelclass'];
                } else {
                    if (fieldMetadata.schema != null &&
                    fieldMetadata.schema.rendererParameters != null &&
                    fieldMetadata.schema.rendererParameters['labelclass'] != null) {
                        cssclass += fieldMetadata.schema.rendererParameters['labelclass'];
                    }
                }

                if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                    cssclass = null;
                    return cssclass;
                }

                var returnClass = $scope.hasSameLineLabel(fieldMetadata) ? 'col-sm-3 col-md-2' : 'col-xs-12';

                //fix SWWEB-732, blank lable adding extra space
                if (returnClass === 'col-xs-12' && fieldMetadata.label === null) {
                    returnClass = cssclass + ' ' + returnClass + ' ng-hide';
                }

                //console.log(fieldMetadata);
                return cssclass + ' ' + returnClass;
            }
            $scope.getInputClass = function (fieldMetadata) {
                var cssclass = "";

                if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters['inputclass'] != null) {
                    cssclass += fieldMetadata.rendererParameters['inputclass'];
                } else {
                    if (fieldMetadata.schema != null &&
                    fieldMetadata.schema.rendererParameters != null &&
                    fieldMetadata.schema.rendererParameters['inputclass'] != null) {
                        cssclass += fieldMetadata.schema.rendererParameters['inputclass'];
                    }
                }
                
                if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                    cssclass += ' col-md-12';
                    return cssclass;
                }

                cssclass += $scope.hasSameLineLabel(fieldMetadata) ? ' col-sm-9 col-md-10' : ' col-xs-12';
                return cssclass;
            }

            ///
            // legendevaluation is boolean indicating the mode we are calling this method, either for an ordinary field or for a header with legend
            ////
            $scope.isLabelVisible = function (fieldMetadata, legendEvaluationMode) {
                if (!$scope.isVerticalOrientation()) {
                    return false;
                }
                var header = fieldMetadata.header;
                if (!header) {
                    return !legendEvaluationMode;
                }
                var isVisible = expressionService.evaluate(header.showExpression, $scope.datamap);
                var isFieldSet = header.parameters != null && "true" == header.parameters['fieldset'];
                //if header is declared as fieldset return true only for the legendEvaluation
                return isVisible && (isFieldSet == legendEvaluationMode);
            }
            $scope.isVerticalOrientation = function () {
                return $scope.orientation == 'vertical';
            };
            $scope.isSectionWithoutLabel = function (fieldMetadata) {
                return fieldMetadata.type == 'ApplicationSection' && fieldMetadata.resourcepath == null && fieldMetadata.header == null;
            };
            $scope.hasSameLineLabel = function (fieldMetadata) {
                return ($scope.isVerticalOrientation()) &&
                (
                (fieldMetadata.header != null && fieldMetadata.header.displacement != 'ontop') ||
                (fieldMetadata.header == null)
                );
            };
            $scope.sectionHasSameLineLabel = function (fieldMetadata) {
                return $scope.hasSameLineLabel(fieldMetadata) && fieldMetadata.type == 'ApplicationSection' && fieldMetadata.resourcepath == null;
            };
            $scope.getValueColumnClass = function (fieldMetadata) {
                var classes = '';
                if ($scope.sectionHasSameLineLabel(fieldMetadata)) {
                    classes += 'col-sectionsamelineheader ';
                }
                if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                    return 'col-md-12';
                }
                if ($scope.isSectionWithoutLabel(fieldMetadata)) {
                    // classes += 'col-md-12 ';
                } else if (!$scope.hasSameLineLabel(fieldMetadata)) {
                    classes += 'col-md-12 ';
                } else if ($scope.isVerticalOrientation()) {
                    //SM - 08/30 - Start, fix label width
                    var lengthparam = $scope.getLengthParam(fieldMetadata);
                    if (lengthparam != null) {
                        classes += lengthparam;
                    } else {
                        classes += 'col-md-10 ';
                    }
                    // classes += 'col-md-8 ';
                    //SM - 08/30 - End, fix label width
                }
                return classes;
            };
            $scope.formatId = function (id) {
                return RemoveSpecialChars(id);
            }
            $scope.nonTabFields = function (displayables) {
                return fieldService.nonTabFields(displayables);
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
                    fieldService: fieldService
                });
            }
            init();

            function bindExpression(expression) {
                var variables = expressionService.getVariablesForWatch(expression);
                $scope.$watchCollection(variables, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        $scope.datamap[fieldMetadata.attribute] = expressionService.evaluate(expression, $scope.datamap);
                    }
                });
                return variables;
            }

            $scope.initField = function (fieldMetadata) {
                if (fieldMetadata.evalExpression != null) {
                    return bindExpression(fieldMetadata.evalExpression);
                }
                return null;
            };
        }
    }
});
app.directive('numberSpinner', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            $(element).spinner({
                change: function (event, ui) {
                    $(element).change();
                },
                min: attr.min,
                max: attr.max
            });
        }
    }
});
app.directive('selectCombo', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            $(element).on('click', 'input', function (e) {
                console.log('click');
                $(element).find('[data-dropdown="dropdown"]').click();
                //return false;
                //e.stopPropagation();
            });

        }
    };
});
