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

        $scope.i18NInputLabel = $scope.i18NInputLabel || function (fieldMetadata, avoidColon=false) {
            return i18NService.getI18nInputLabel(fieldMetadata, $scope.schema, avoidColon);
        };

        $scope.getBooleanClass = function (item, attribute) {
            if (formatService.isChecked(item[attribute])) {
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

        $scope.getTabRecordCount = function (tab) {
            return crudContextHolderService.getTabRecordCount(tab);
        }

        $scope.showTabRecordCount = function (tab) {
            return crudContextHolderService.shouldShowRecordCount(tab);
        }

        $scope.getTabIcon = function (tab) {
            return tab.schema.schemas.list.properties['icon.composition.tab'];
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

        function applyFilter(filter, options, {compositionItem} = {}) {
            if (options && filter && filter.clientFunction) {
                const fn = dispatcherService.loadServiceByString(filter.clientFunction);
                if (fn == null) {
                    $log.get("baselist#getoptionfields", ["association", "optionfield"]).warn("method {0} not found. review your metadata".format(filter.clientFunction));
                    return options;
                }

                const filterFunction = function({compositionItem} = {}) {
                    return function (element) {
                        return fn(element, {compositionItem});
                    }
                }

                const filteredOptions = options.filter(filterFunction({compositionItem}));

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

        $scope.GetAssociationOptions = function (fieldMetadata, datamapValue, {compositionItem} = {}) {
            if (fieldMetadata.type === "OptionField") {
                return $scope.GetOptionFieldOptions(fieldMetadata, datamapValue, {compositionItem});
            }
            var contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;

            // special case of a composition list
            if (compositionService.isCompositionListItem($scope.datamap)) {
                contextData = compositionService.buildCompositionListItemContext(contextData, $scope.datamap, $scope.schema);
            }
            const rawOptions = crudContextHolderService.fetchEagerAssociationOptions(fieldMetadata.associationKey, contextData, $scope.panelid, datamapValue);
            return applyFilter(fieldMetadata.filter, rawOptions, {compositionItem});
        }
        $scope.GetOptionFieldOptions = function (optionField, datamapValue, {compositionItem} = {}) {
            if (optionField.providerAttribute == null) {
                return applyFilter(optionField.filter, optionField.options, {compositionItem});
            }
            const contextData = $scope.ismodal === "true" ? { schemaId: "#modal" } : null;
            const options = crudContextHolderService.fetchEagerAssociationOptions(optionField.providerAttribute, contextData, $scope.panelid,datamapValue);
            if (!options) {
                return blankArray;
            }else if (options.length === 1 && fieldService.isFieldRequired(optionField, $scope.datamap)) {
                $scope.datamap[optionField.target] = options[0].value;
            }
            return applyFilter(optionField.filter, options, {compositionItem});
        }


        $scope.getFieldClass = function (fieldMetadata) {
           

            return layoutservice.getFieldClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() });
        }

        $scope.getLabelClass = function (fieldMetadata) {
            return layoutservice.getLabelClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() });
        }

        $scope.getInputClass = function (fieldMetadata) {
            return layoutservice.getInputClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() });
        }

        $scope.isReadOnlyField = function (fieldMetadata,datamap, staticCheck=false) {
            const isDefaultReadOnly = fieldService.isFieldReadOnly(datamap, null, fieldMetadata, $scope, staticCheck);
            if (isDefaultReadOnly) {
                return true;
            }

            if (genericTicketService.isClosed() && !!fieldMetadata.rendererType) {
                return fieldMetadata.rendererType.equalsAny('checkbox',"datetime","imagepreview","lookup","richtext","textarea") ||
                (fieldMetadata.rendererType === 'default' && !!fieldMetadata.dataType);
            }
            return false;
        }

        $scope.isCheckboxItemReadOnlyField = function (fieldMetadata, option, datamap) {
            return $scope.isReadOnlyField(fieldMetadata, datamap) || $scope.isReadOnlyField(option, datamap);
        }

        $scope.isStaticReadOnlyField = function (fieldMetadata, datamap) {
            //on the html we have some components who are rendered completely differently if they are readonly (ex: richtext and textarea), thus,
            //we have a top section for first checking whether the fields are readonly or not
            //others are implemented in a way that they have some sort of ng-readonly/ng-enabled flag. These do not have a corresponding match on the readonly section, and if we do not exclude 
            //them for the check, they would render a wrong default type
            if (fieldMetadata.rendererType != null && fieldMetadata.rendererType.equalsAny("radio", "checkbox")) {
                //TODO: migrate other renderer types for a completely dinamic evaluation
                return false;
            }

            return $scope.isReadOnlyField(fieldMetadata, datamap);
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

        $scope.headerStyle = function (fieldMetadata) {
            const header = fieldMetadata.header;
            if (!header.parameters) {
                return null;
            }
            if (!header.jscache) {
                header.jscache = {};
            }
            if (header.jscache && header.jscache.headerStyle) {
                return header.jscache.headerStyle;
            }

            if (fieldMetadata.type === "ApplicationCompositionDefinition") {
                return {
                    margin: '0px',
                    width: '100%'
                };
            }

            if (header.parameters["style"]) {
                const style = JSON.parse(header.parameters["style"]);
                header.jscache.headerStyle = style;
                return style;
            }

            const style = {};
            const bgcolor = header.parameters['backgroundcolor'];
            if (bgcolor) {
                style["background"] = bgcolor;
            }
            return style;

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
    }

    window.BaseController = BaseController;

})(angular);
