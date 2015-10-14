(function (angular) {
    'use strict';

    angular.module('webcommons_services').factory('validationService', ['$log', 'i18NService', 'fieldService', '$rootScope', 'dispatcherService', 'expressionService', 'eventService', 'compositionCommons', 'schemaService', 'alertService', validationService]);

    function validationService($log, i18NService, fieldService, $rootScope, dispatcherService, expressionService, eventService, compositionCommons, schemaService, alertService) {

      

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
                var mergedDatamap = compositionCommons.buildMergedDatamap(row, mainDatamap);
                mergedDatamap["#rownum"] = i;
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

                if (fieldService.isInlineComposition(displayable) || (fieldService.isListOnlyComposition(displayable) && schemaService.hasEditableProperty(displayable.schema.schemas.list))) {
                    validationArray = validationArray.concat(this.validateInlineComposition(displayable, datamap));
                }

                if (isRequired && nullOrEmpty(datamap[displayable.attribute])) {
                    if (!nullOrEmpty(datamap.docinfoid)) {
                        //Existing Attachment
                        continue;
                    }
                    var qualifier = "";
                    if (datamap["#rownum"]) {
                        qualifier = "(" + datamap["#rownum"] + ")";
                    }

                    if (displayable.rendererType == "upload" && nullOrEmpty(datamap.newattachment_path)) {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', '<strong>{0}{1}</strong> is required', [label, qualifier]));
                        continue;
                    }

                    if (label.endsWith('s') || label.endsWith('S')) {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', '<strong>{0}{1}</strong> are required', [label, qualifier]));
                    } else {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', '<strong>{0}{1}</strong> is required', [label, qualifier]));
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
                    //TODO: replace ntification message with in-line error messages
                    alertService.notifymessage('error', validationArray.join(', '));

                    //TODO: scroll to the first error message
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                }
            }
            return validationArray;
        };

        var service = {
            getInvalidLabels: getInvalidLabels,
            validate: validate,
            validateInlineComposition: validateInlineComposition,

        };

        return service;

      

    }

})(angular);