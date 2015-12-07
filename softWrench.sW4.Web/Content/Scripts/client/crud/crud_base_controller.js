﻿//idea took from  https://www.exratione.com/2013/10/two-approaches-to-angularjs-controller-inheritance/
function BaseController($scope, i18NService, fieldService, commandService, formatService, layoutservice,expressionService) {

    /* i18N functions */
    $scope.i18NLabelTooltip = function (fieldMetadata) {
        return i18NService.getI18nLabelTooltip(fieldMetadata, $scope.schema);
    };
    //to allow overriding
    $scope.i18NLabel = $scope.i18NLabel || function (fieldMetadata) {
        return i18NService.getI18nLabel(fieldMetadata, $scope.schema);
    };

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

    $scope.getFormattedValue = function (value, column, datamap) {
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


    $scope.getFieldClass = function (fieldMetadata) {
        if (fieldMetadata.rendererParameters && fieldMetadata.rendererParameters["class"]) {
            return fieldMetadata.rendererParameters["class"];
        }

        return layoutservice.getFieldClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() });
    }

    $scope.getLabelClass = function (fieldMetadata) {
        return layoutservice.getLabelClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() })
    }

    $scope.getInputClass = function (fieldMetadata) {
        return layoutservice.getInputClass(fieldMetadata, $scope.datamap, $scope.schema, $scope.displayables, { sectionparameters: $scope.sectionParameters, isVerticalOrientation: this.isVerticalOrientation() })
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



}