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
            angular.forEach(displayables, function(displayable) {
                var attribute = displayable.attribute;
                item[attribute] = scope.option.extrafields[attribute];
            });
            scope.datamap[scope.fieldMetadata.attribute].push(item);
        }
    };
})
.directive('configUpdateSectionDatamap', function () {
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
                if (oldValue === newValue) {
                    return;
                }
                scope.datamap[scope.fieldMetadata.id][scope.$index]["value"] = newValue;
            });
            item["#newvalue"] = '';
            scope.datamap[scope.fieldMetadata.id].push(item);
        }
    };
})
.directive('sectionElementInput', ["$compile", "$timeout", function ($compile, $timeout) {
    var template =  "<crud-input-fields displayables='displayables'" +
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
                	"></crud-input-fields>";
    return {
        restrict: "E",
        replace: false,
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
            ismodal:'@',
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',
            rendererParameters: '='
        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (!angular.isArray(scope.displayables)) return;
            var compiled = $compile(template)(scope);
            element.append(compiled);
        }
    }
}])
.directive('crudInputFields', ["contextService", "eventService", "crud_inputcommons", "crudContextHolderService", function (contextService, eventService, crud_inputcommons, crudContextHolderService) {
    return {
        restrict: 'E',
        replace: false,
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
                    scope.$on('sw.modal.show', function (event, modaldata) {
                        scope.lookupAssociationsCode = {};
                        scope.lookupAssociationsDescription = {};
                    });
                }
            }

            var parameters = {
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
            "cmpfacade", "cmpComboDropdown", "redirectService", "validationService", "contextService", "eventService", "formatService", "modalService", "dispatcherService", "cmplookup",
            "layoutservice", "attachmentService", "richTextService", 
            function ($scope, $http, $element, $injector, $timeout, $log,
            printService, compositionService, commandService, fieldService, i18NService,
            associationService, expressionService, styleService,
            cmpfacade, cmpComboDropdown, redirectService, validationService, contextService, eventService, formatService, modalService, dispatcherService, cmplookup,
            layoutservice, attachmentService, richTextService) {
            
            $scope.$name = 'crud_input_fields';
            $scope.lookupObj = {};

            //dictionary containing which details are or not expanded
            $scope.expandeddetails = {};

            $scope.vm = {
                nonTabDisplayables: []
            };

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

            $scope.setForm = function (form) {
                $scope.crudform = form;
            };



            $scope.$on('sw_block_association', function (event, association) {
                $scope.blockedassociations[association] = true;
            });

            $scope.$on('sw_unblock_association', function (event, association) {
                $scope.blockedassociations[association] = null;
            });

            $scope.getCheckboxOptions = function (fieldMetadata) {
                if (fieldMetadata.providerAttribute == null) {
                    return fieldMetadata.options;
                }
                var contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;
                return crudContextHolderService.fetchEagerAssociationOptions(fieldMetadata.associationKey, contextData);
            }
            $scope.isPositionLeft = function (fieldMetadata) {
                return "left".equalIc(fieldMetadata.rendererParameters['position']);
            }
            $scope.$on('sw_associationsupdated', function (event, associationoptions) {
                $scope.associationsloaded = true;
                if (!$scope.associationOptions) {
                    //this in scenarios where a section is compiled before the association has returned from the server... angular seems to get lost in the bindings
                    $scope.associationOptions = associationoptions;
                }
            });
            //this will get called when the input form is done rendering
            $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent, parentElementId) {
                eventService.onload($scope.schema, $scope.datamap);
                var bodyElement = $('#' + parentElementId);
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
                    //to avoid registering these global listeners multiple times, as the page main contain sections.
                    $scope.configureNumericInput();
                    $scope.configureOptionFields();
                    crud_inputcommons.configureAssociationChangeEvents($scope, 'datamap', $scope.displayables);
                    $scope.configureFieldChangeEvents();
                    //                    $scope.configureDirtyWatcher();
                }
                var datepickers = $('.datetimereadonly').data("DateTimePicker");
                if (datepickers) {
                    datepickers.disable();
                }


                // Configure input files
                $('#uploadBtn').on('change', function (e) {
                    var fileName = this.value.match(/[^\/\\]+$/);
                    var isValid = attachmentService.isValid(this.value);
                    if (!isValid) {
                        $('#uploadFile').attr("value", "");
                        if (isIe9()) {
                            //hacky around ie9 -- HAP-894
                            $('#uploadFile').attr("value", "");
                            $(this).replaceWith($(this).clone(true));
                        } else {
                            $('#uploadFile').val('');
                        }
                        return;
                    }
                    $('#uploadFile').attr("value", fileName);
                });

                $("#uploadInputLink").on('click', function (e) {
                    e.preventDefault();
                    $("#uploadInput").trigger('click');
                });
            });


            $scope.$on("sw_configuredirtywatcher", function () {
                $timeout(function () {
                    $scope.$watch('datamap', function (newValue, oldValue) {
                        if (newValue !== oldValue) {
                            crudContextHolderService.setDirty();
                        }
                    }, true);
                }, 0, false);
            });


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
                $scope.disabledassociations = instantiateIfUndefined($scope.disabledassociations);
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
            $scope.toogleCheckboxSelection = function (option, datamapKey) {
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
                if (content === "1" || content === "true" || content === true) {
                    content = "true";
                } else {
                    content = "false";
                }
                $scope.datamap[fieldMetadata.attribute] = content;
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

                    if ($scope.datamap[optionfield.target] == null && optionfield.providerAttribute == null && optionfield.rendererType !== 'checkbox') {
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
                if (fieldMetadata.type === "OptionField") {
                    return $scope.GetOptionFieldOptions(fieldMetadata);
                }
                var contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;
                return crudContextHolderService.fetchEagerAssociationOptions(fieldMetadata.associationKey, contextData);
            }
            $scope.GetOptionFieldOptions = function (optionField) {
                if (optionField.providerAttribute == null) {
                    return optionField.options;
                }
                var contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;
                return crudContextHolderService.fetchEagerAssociationOptions(optionField.providerAttribute,contextData);
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
            $scope.showLabelTooltip = function (fieldMetadata) {
                if (fieldMetadata.label !== fieldMetadata.toolTip) {
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
                    //if there are no details, there´s nothing at all to expand
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

            $scope.formatId = function (id) {
                return RemoveSpecialChars(id);
            }

            $scope.nonTabFields = function (displayables) {
                return fieldService.nonTabFields(displayables);
            };

            $scope.getDispatchFn = function (serviceCall) {
                var validationFunction = dispatcherService.loadServiceByString(serviceCall);
                if (validationFunction == null) {
                    validationFunction = function () { return true };
                }
                return validationFunction;
            }

            function init() {

                $scope.vm.nonTabDisplayables = $scope.nonTabFields($scope.displayables);

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

            $scope.initRichtextField = function (fieldMetadata) {
                var content = $scope.datamap[fieldMetadata.attribute];
                $scope.datamap[fieldMetadata.attribute] = richTextService.getDecodedValue(content);
            }

            $scope.isMobile = function () {
                return isMobile();
            };

            $scope.isDesktop = function () {
                return isDesktop();
            };

            $scope.isFieldRequired = function (fieldMetadata) {
                return fieldService.isFieldRequired(fieldMetadata, $scope.datamap);
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