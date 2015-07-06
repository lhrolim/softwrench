
(function () {
    'use strict';

    angular.module('webcommons_services').factory('validationService', ['$log', 'i18NService', 'fieldService', '$rootScope', 'dispatcherService', 'expressionService', 'eventService', 'compositionService', validationService]);

    function validationService($log, i18NService, fieldService, $rootScope, dispatcherService, expressionService, eventService, compositionService) {

        var service = {
            getInvalidLabels: getInvalidLabels,
            validate: validate,
            validateInlineComposition: validateInlineComposition,
            setDirty: setDirty,
            getDirty: getDirty,
            clearDirty: clearDirty
        };

        return service;

        function getInvalidLabels(displayables, datamap) {
            /// <summary>
            ///  Similar to the validate method, but only returning the array of items, for custom handling
            /// </summary>
            /// <param name="displayables"></param>
            /// <param name="datamap"></param>
            /// <param name="innerValidation"></param>
            /// <returns type=""></returns>
            var validationArray = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var label = displayable.label;
                if (fieldService.isNullInvisible(displayable, datamap)) {
                    continue;
                }
                var isRequired = false;
                if (displayable.requiredExpression != null) {
                    isRequired = expressionService.evaluate(displayable.requiredExpression, datamap);
                }
                if (isRequired && nullOrEmpty(datamap[displayable.attribute])) {
                    validationArray.push(label);
                }
                if (displayable.displayables != undefined) {
                    //validating section
                    var innerArray = this.validate(displayable.displayables, datamap);
                    validationArray = validationArray.concat(innerArray);
                }
            }

            return validationArray;
        };

        function validateInlineComposition(compositionDisplayable, mainDatamap) {
            var listSchema = compositionDisplayable.schema.schemas.list;
            var rows = mainDatamap[compositionDisplayable.relationship];
            var result = [];
            if (!rows) {
                return result;
            }
            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                var mergedDatamap =compositionService.buildMergedDatamap(row, mainDatamap);
                result = result.concat(this.validate(listSchema, listSchema.displayables, mergedDatamap, null, true));
            }
            return result;
        }



        function validate(schema, displayables, datamap, angularformerrors, innerValidation) {
            angularformerrors = instantiateIfUndefined(angularformerrors);
            var log = $log.get("validationService#validate");
            var validationArray = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var label = displayable.label;
                log.debug("performing validation on field {0}".format(label));
                if (fieldService.isNullInvisible(displayable, datamap)) {
                    continue;
                }
                var isRequired = false;
                if (displayable.requiredExpression != null) {
                    isRequired = expressionService.evaluate(displayable.requiredExpression, datamap);
                }

                var defaultTitleValue = schema.title ? schema.title : schema.applicationName;
                var applicationName = i18NService.get18nValue(schema.applicationName + ".name", defaultTitleValue);

                if (displayable.rendererType == "email" && angularformerrors.email && !angularformerrors.email[0][displayable.attribute].$valid) {
                    //TODO: make this a bit more generic
                    validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', 'Invalid email at field {0} for {1}', [label, applicationName]));
                    continue;
                }

                if (fieldService.isInlineComposition(displayable) || (fieldService.isListOnlyComposition(displayable) && compositionService.hasEditableProperty(displayable.schema.schemas.list))) {
                    validationArray = validationArray.concat(this.validateInlineComposition(displayable, datamap));
                }

                if (isRequired && nullOrEmpty(datamap[displayable.attribute])) {
                    if (!nullOrEmpty(datamap.docinfoid)) {
                        //Existing Attachment
                        continue;
                    }
                    if (displayable.rendererType == "upload" && nullOrEmpty(datamap.newattachment_path)) {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', 'Field {0} is required', [label]));
                        continue;
                    }

                    if (label.endsWith('s') || label.endsWith('S')) {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', 'Field {0} are required', [label]));
                    } else {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', 'Field {0} is required', [label]));
                    }
                }
                if (displayable.displayables != undefined) {
                    log.debug("validating section {0}".format(label));
                    //validating section
                    var innerArray = this.validate(schema, displayable.displayables, datamap, angularformerrors, true);
                    validationArray = validationArray.concat(innerArray);
                }
            }
            if (innerValidation == undefined || !innerValidation) {
                var customErrorArray = eventService.beforesubmit_onvalidation(schema, datamap);
                if (customErrorArray != null && Array.isArray(customErrorArray)) {
                    validationArray = validationArray.concat(customErrorArray);
                }
                if (validationArray.length > 0) {
                    $rootScope.$broadcast('sw_validationerrors', validationArray,$rootScope.showingModal);
                }
            }
            return validationArray;
        };


        function setDirty() {
            $rootScope.isDirty = true;
        };

        function getDirty() {
            return $rootScope.isDirty;
        };

        function clearDirty() {
            $rootScope.isDirty = false;
        }



    }
})();




