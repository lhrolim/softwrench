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
            elementid: '@',
            orientation: '@',
            islabelless: '@'
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
                                "orientation='{{orientation}}' insidelabellesssection={{islabelless}}></crud-input-fields>"
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
            isDirty: '=',
            displayables: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            elementid: '@',
            orientation: '@',
            insidelabellesssection: '@',
        },

        controller: function ($scope, $http, $element, $injector, $timeout,
            printService, compositionService, commandService, fieldService, i18NService,
            associationService, expressionService, styleService,
            cmpfacade,cmpComboDropdown, redirectService,validationService, contextService) {

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

            $scope.getCheckboxOptions = function(fieldMetadata){
                if (fieldMetadata.providerAttribute == null) {
                    return fieldMetadata.options;
                }
                return associationOptions[fieldMetadata.associationKey];
            }

            $scope.isPositionLeft = function(fieldMetadata) {
                return "left".equalIc(fieldMetadata.rendererParameters['position']);
            }

            $scope.$on('sw_associationsupdated', function (event, associationoptions) {
                //this in scenarios where a section is compiled before the association has returned from the server... angular seems to get lost in the bindings
                $scope.associationOptions = associationoptions;
            });

            //this will get called when the input form is done rendering
            $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent, parentElementId) {

                var bodyElement = $('#' + parentElementId);
                if (bodyElement.length <= 0) {
                    return;
                }
                // Configure tooltips
                $('.no-touch [rel=tooltip]', bodyElement).tooltip({ container: 'body' });

                $scope.configureLookupModals(bodyElement);
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

                if (parentElementId.indexOf('Section') == -1) {
                    //to avoid registering these global listeners multiple times, as the page main contain sections.
                    $scope.configureNumericInput();
                    $scope.configureOptionFields();
                    $scope.configureAssociationChangeEvents();
                    
                    $scope.configureDirtyWatcher();
                }

                $('.datetimereadonly').datepicker("remove");
                // Configure input files
                $('#uploadBtn').on('change', function (e) {
                    if (this != null) {
                        $('#uploadFile').attr("value", this.value);}});
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
                                    associationService.postAssociationHook(association, $scope, { phase: 'configured', dispatchedbytheuser: true });
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

            $scope.lookupAssociationsCode = {};
            $scope.lookupAssociationsDescription = {};

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

            $scope.configureLookupModals = function (bodyElement) {
                // Configure lookup modals
                var lookups = fieldService.getDisplayablesOfRendererTypes($scope.schema.displayables, ['lookup']);
                $.each(lookups, function (key, value) {
                    var fieldMetadata = value;
                    if ($scope.associationOptions == null) {
                        //this scenario happens when a composition has lookup-associations on its details, 
                        //but the option list has not been fetched yet
                        $scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
                        $scope.lookupAssociationsCode[fieldMetadata.attribute] = null;
                    } else {
                        var options = $scope.associationOptions[fieldMetadata.associationKey];

                        var doConfigure = function (optionValue) {

                            $scope.lookupAssociationsCode[fieldMetadata.attribute] = optionValue;
                            if (options == null || options.length <= 0) {
                                //it should always be lazy loaded... why is this code even needed?
                                return;
                            }

                            var optionSearch = $.grep(options, function (e) {
                                return e.value == optionValue;
                            });

                            var valueToSet = optionSearch != null && optionSearch.length > 0 ? optionSearch[0].label : null;
                            $scope.lookupAssociationsDescription[fieldMetadata.attribute] = valueToSet;
                        }

                        doConfigure($scope.datamap[fieldMetadata.target]);
                    }
                });
            };

            $scope.bindEvalExpression = function (fieldMetadata) {
                if (fieldMetadata.evalExpression == null) {
                    return;
                }
                var variables = expressionService.getVariablesForWatch(fieldMetadata.evalExpression);
                $scope.$watchCollection(variables, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        $scope.datamap[fieldMetadata.attribute] = expressionService.evaluate(fieldMetadata.evalExpression, $scope.datamap);
                    }
                });
            }

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

            //SM - 09/24 - SWWEB-441 change OTM column classes
            $scope.getLabelClass = function (fieldMetadata) {
                //return $scope.hasSameLineLabel(fieldMetadata) ? 'col-md-2' : 'col-md-12';
                if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                    return null;
            }
                return $scope.hasSameLineLabel(fieldMetadata) ? 'col-sm-3 col-md-2' : 'col-xs-12';
            }

            $scope.getFieldClass = function (fieldMetadata) {
                if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                    return 'col-md-12';
            }
                return $scope.hasSameLineLabel(fieldMetadata) ? 'col-sm-9 col-md-10' : 'col-xs-12';
            }
            //SM - 09/24 - SWWEB-441 change OTM column classes

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
                    //                    classes += 'col-md-12 ';
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
                    //                    classes += 'col-md-8 ';
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
            console.log(attr);
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