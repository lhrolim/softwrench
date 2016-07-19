(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("BaseController", BaseController);
    //idea took from  https://www.exratione.com/2013/10/two-approaches-to-angularjs-controller-inheritance/
    BaseController.$inject = ["$scope", "$log", "i18NService", "fieldService", "commandService", "formatService", "layoutservice", "expressionService", "crudContextHolderService", "dispatcherService", "compositionService", "genericTicketService","$timeout"];
    function BaseController($scope, $log, i18NService, fieldService, commandService, formatService, layoutservice, expressionService, crudContextHolderService, dispatcherService, compositionService, genericTicketService,$timeout) {

        const blankArray = [];

        //to overcome an angular issue with infinite loops
        var lastArray = [];
        var lastArrayValues = [];

        /* i18N functions */
        $scope.i18NLabelTooltip = function (fieldMetadata) {
            return i18NService.getI18nLabelTooltip(fieldMetadata, $scope.schema);
        };

        //to allow overriding
        $scope.i18NLabel = $scope.i18NLabel || function (fieldMetadata) {
            return i18NService.getI18nLabel(fieldMetadata, $scope.schema);
        };

        $scope.i18NInputLabel = $scope.i18NInputLabel || function (fieldMetadata) {
            return i18NService.getI18nInputLabel(fieldMetadata, $scope.schema);
        };

        $scope.getBooleanClass = function (item, attribute) {
            if (item[attribute] == "true" || item[attribute] === 1 || item[attribute] === "1") {
                return 'fa-check-square-o';
            }
            return 'fa-square-o';
        }


        $scope.i18NOptionField = function (option, fieldMetadata, schema) {
            return i18NService.getI18nOptionField(option, fieldMetadata, schema);
        };


        $scope.i18N = function (key, defaultValue, paramArray) {
            return i18NService.get18nValue(key, defaultValue, paramArray);
        };

        $scope.i18NCommandLabel = function (command) {
            return i18NService.getI18nCommandLabel(command, $scope.schema);
        };



        $scope.i18NTabLabel = function (tab) {
            return i18NService.getTabLabel(tab, $scope.schema);
        };

        $scope.getFormattedValue = $scope.getFormattedValue || function (value, column, datamap) {
            return formatService.format(value, column, datamap);
        };

        $scope.isIE9 = function () {
            return isIe9();
        };

        $scope.isIe9 = function () {
            return isIe9();
        };

        $scope.i18nSectionLabel = function (section) {
            var label = i18NService.getI18nSectionHeaderLabel(section, section.header, $scope.schema);
            if (label != undefined && label != "") {
                label = label.replace(':', '');
            }
            return label;
        };

        $scope.nonTabFields = function (displayables) {
            return fieldService.nonTabFields(displayables);
        };

        $scope.contextPath = function (path) {
            return url(path);
        };

        $scope.isFieldHidden = function (application, fieldMetadata) {
            return fieldService.isFieldHidden($scope.datamap, application, fieldMetadata);
        };

        $scope.fieldHasValue = function (fieldMetadata) {
            return fieldService.fieldHasValue($scope.datamap, fieldMetadata);
        };

        $scope.shouldDisplayCommand = function (commandSchema, id) {
            return commandService.shouldDisplayCommand(commandSchema, id);
        };

        $scope.commandLabel = function (schema, id, defaultValue) {
            return commandService.commandLabel(schema, id, defaultValue);
        };


        $scope.isCommandHidden = function (schema, command) {
            return commandService.isCommandHidden($scope.datamap, schema, command);
        };

        $scope.doCommand = function (command) {
            commandService.doCommand($scope, command);
        };

        $scope.hasSameLineLabel = function (fieldMetadata) {
            if (!$scope.isVerticalOrientation()) {
                return false;
            }
            if (fieldMetadata.rendererType === "TABLE") {
                //workaround because compositions are appending "" as default label values, but we dont want it!
                return false;
            }
            return fieldMetadata.label != null || (fieldMetadata.header != null && fieldMetadata.header.displacement !== 'ontop');
        };

        $scope.isVerticalOrientation = function () {
            return $scope.orientation === 'vertical';
        };


        $scope.getSectionClass = function (fieldMetadata) {
            if (fieldMetadata.rendererType === "TABLE") {
                //workaround because compositions are appending "" as default label values, but we dont want it!
                return null;
            }
            if (!$scope.hasSameLineLabel(fieldMetadata)) {
                return 'col-horizontal-orientation';
            }
            return null;
        };

        function applyFilter(filter, options) {
            if (options && filter && filter.clientFunction) {
                const fn = dispatcherService.loadServiceByString(filter.clientFunction);
                if (fn == null) {
                    $log.get("baselist#getoptionfields", ["association", "optionfield"]).warn("method {0} not found. review your metadata".format(filter.clientFunction));
                    return options;
                }
                const filteredOptions = options.filter(fn);

                const currentValues = filteredOptions.map(item => {
                    return item.value;
                });

                const arrayEquals = currentValues.equals(lastArrayValues);
                //this code is used due to a bug on angular where it consider two arrays to be different even though they are exactly the same
                //TODO: improve this solution
                if (arrayEquals) {
                    return lastArray;
                }
                lastArray = filteredOptions;
                lastArrayValues = currentValues;

                if (filteredOptions.length === 0) {
                    //need to return this very same array every time to avoid angular infinite digest loops
                    
                    return blankArray;
                }
                return filteredOptions;
            }
            return options;
        }

        $scope.GetAssociationOptions = function (fieldMetadata) {
            if (fieldMetadata.type === "OptionField") {
                return $scope.GetOptionFieldOptions(fieldMetadata);
            }
            var contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;

            // special case of a composition list
            if (compositionService.isCompositionListItem($scope.datamap)) {
                contextData = compositionService.buildCompositionListItemContext(contextData, $scope.datamap, $scope.schema);
            }
            var rawOptions = crudContextHolderService.fetchEagerAssociationOptions(fieldMetadata.associationKey, contextData, $scope.panelid);
            return applyFilter(fieldMetadata.filter, rawOptions);
        }
        $scope.GetOptionFieldOptions = function (optionField) {
            if (optionField.providerAttribute == null) {
                return applyFilter(optionField.filter, optionField.options);
            }
            var contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;
            var options = crudContextHolderService.fetchEagerAssociationOptions(optionField.providerAttribute, contextData, $scope.panelid);
            return applyFilter(optionField.filter, options);
        }


        $scope.getFieldClass = function (fieldMetadata) {
            if (fieldMetadata.rendererParameters && fieldMetadata.rendererParameters["class"]) {
                return fieldMetadata.rendererParameters["class"];
            }

            return layoutservice.getFieldClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() });
        }

        $scope.getLabelClass = function (fieldMetadata) {
            return layoutservice.getLabelClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() });
        }

        $scope.getInputClass = function (fieldMetadata) {
            return layoutservice.getInputClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() });
        }

        $scope.isReadOnlyField = function (fieldMetadata,datamap) {
            const isDefaultReadOnly = fieldService.isFieldReadOnly(datamap, null, fieldMetadata, $scope);
            if (isDefaultReadOnly) {
                return true;
            }

            if (genericTicketService.isClosed() && !!fieldMetadata.rendererType) {
                return fieldMetadata.rendererType.equalsAny('checkbox',"datetime","imagepreview","lookup","richtext","textarea") ||
                (fieldMetadata.rendererType === 'default' && !!fieldMetadata.dataType);
            }
            return false;
        }

        $scope.getCheckboxLabelLeftClass = function (fieldMetadata) {
            return layoutservice.getCheckboxLabelLeftClass(fieldMetadata);
        }

        $scope.getCheckboxLabelRightClass = function (fieldMetadata) {
            return layoutservice.getCheckboxLabelRightClass(fieldMetadata);
        }

        ///
        // legendevaluation is boolean indicating the mode we are calling this method, either for an ordinary field or for a header with legend
        ////
        $scope.isLabelVisible = function (fieldMetadata, legendEvaluationMode) {
            //                if (!$scope.isVerticalOrientation()) {
            //                    return false;
            //                }
            var header = fieldMetadata.header;
            if (!header) {
                return !legendEvaluationMode && fieldMetadata.label;
            }
            var isVisible = expressionService.evaluate(header.showExpression, $scope.datamap);
            var isFieldSet = header.parameters != null && "true" === header.parameters['fieldset'];
            //if header is declared as fieldset return true only for the legendEvaluation
            return isVisible && (isFieldSet === legendEvaluationMode);
        }

        $scope.formatId = function (id) {
            return RemoveSpecialChars(id);
        }

        $scope.isDesktop = function () {
            return isDesktop();
        };

        $scope.isString = function (value) {
            return angular.isString(value);
        };

        $scope.isArray = function (value) {
            return angular.isArray(value);
        };
    }

    window.BaseController = BaseController;

})(angular);
