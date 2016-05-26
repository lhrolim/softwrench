(function (angular) {
    'use strict';

    angular.module('webcommons_services').factory('validationService', ['$log', 'i18NService', 'fieldService', '$rootScope', 'dispatcherService', 'expressionService', 'eventService', 'compositionCommons', 'schemaService', 'alertService', 'crudContextHolderService', "passwordValidationService", validationService]);

    function validationService($log, i18NService, fieldService, $rootScope, dispatcherService, expressionService, eventService, compositionCommons, schemaService, alertService, crudContextHolderService, passwordValidationService) {
      
        /**
         * Similar to the validate method, but only returning the array of items, for custom handling
         * 
         * @param {Array<FieldMetadata>} displayables 
         * @param {Datamap} datamap 
         * @returns {Array<String>} 
         */
        function getInvalidLabels(displayables, datamap) {
            var validationArray = [];
            displayables.forEach(displayable => {
                var label = displayable.label;
                if (fieldService.isNullInvisible(displayable, datamap)) {
                    return;
                }
                var isRequired = !!displayable.requiredExpression ? expressionService.evaluate(displayable.requiredExpression, datamap) : false;
                if (isRequired && nullOrEmpty(datamap[displayable.attribute])) {
                    validationArray.push(label);
                }
                if (!!displayable.displayables) {
                    // validating section
                    const innerArray = this.validate(displayable.displayables, datamap);
                    validationArray = validationArray.concat(innerArray);
                }
            });

            return validationArray;
        };

        function validateInlineComposition(compositionDisplayable, mainDatamap) {
            const listSchema = compositionDisplayable.schema.schemas.list;
            const rows = mainDatamap[compositionDisplayable.relationship];
            var result = [];
            if (!rows) {
                return result;
            }
            for (let i = 0; i < rows.length; i++) {
                const row = rows[i];
                const mergedDatamap = compositionCommons.buildMergedDatamap(row, mainDatamap);
                mergedDatamap["#rownum"] = i;
                result = result.concat(this.validate(listSchema, listSchema.displayables, mergedDatamap, null, true));
            }
            return result;
        }

        function validateCurrent(panelId) {
            const schema = crudContextHolderService.currentSchema(panelId);
            var datamap = crudContextHolderService.rootDataMap(panelId);
            if (datamap.fields) {
                datamap = datamap.fields;
            }

            return this.validate(schema, schema.displayables, datamap);
        }

        function validate(schema, displayables, datamap, angularformerrors, innerValidation) {
            angularformerrors = angularformerrors || {};
            const log = $log.get("validationService#validate");
            let validationArray = [];
            const allDisplayables = schemaService.flattenDisplayables(displayables);

            const getNgModel = (errors, type, name) => {
                const typeError = errors[type];
                if (!typeError) return null;
                // in case it's an array of NgModel or an array of NgForm
                const error = typeError.find(e => e.$name === name) || typeError[0][name];
                return error;
            };

            const isInvalidEmail = (errors, name) => {
                const error = getNgModel(errors, "email", name);
                return !!error && error.$invalid;
            };

            allDisplayables.forEach(displayable => {
                var label = displayable.label;
                log.debug("performing validation on field {0}".format(label));
                if (fieldService.isNullInvisible(displayable, datamap)) {
                    return;
                }
                var isRequired = !!displayable.requiredExpression
                    ? expressionService.evaluate(displayable.requiredExpression, datamap)
                    : false;

                var defaultTitleValue = schema.title ? schema.title : schema.applicationName;
                var applicationName = i18NService.get18nValue(schema.applicationName + ".name", defaultTitleValue);

                if (displayable.rendererType === "email" && angularformerrors.email && isInvalidEmail(angularformerrors, displayable.attribute)) {
                    //TODO: make this a bit more generic
                    validationArray.push(i18NService.get18nValue("messagesection.validation.requiredExpression", "Invalid email at field {0} for {1}", [label, applicationName]));
                    return;
                }

                // validate password: if it is required or is not required but was filled any way
                if (displayable.rendererType === "password") {
                    const password = getNgModel(angularformerrors, "password", displayable.attribute);
                    if (!!password) {
                        const passwordValue = password.$viewValue;
                        const username = datamap[displayable.rendererParameters["usernameFieldName"]];
                        if (isRequired || !nullOrEmpty(passwordValue)) {
                            const passwordValidation = passwordValidationService.validatePassword(passwordValue, { username: username });
                            validationArray = validationArray.concat(passwordValidation.map(v => `<br><strong>${label}</strong>: ${v}`));
                        }
                    }
                }

                if (fieldService.isInlineComposition(displayable) || (fieldService.isListOnlyComposition(displayable) && schemaService.hasEditableProperty(displayable.schema.schemas.list))) {
                    validationArray = validationArray.concat(this.validateInlineComposition(displayable, datamap));
                }

                if (isRequired && nullOrEmpty(datamap[displayable.attribute])) {
                    if (!nullOrEmpty(datamap.docinfoid)) {
                        //Existing Attachment
                        return;
                    }
                    const qualifier = !!datamap["#rownum"] ? `(${datamap["#rownum"]})` : "";

                    if (displayable.rendererType === "upload" && nullOrEmpty(datamap.newattachment_path)) {
                        validationArray.push(i18NService.get18nValue("messagesection.validation.requiredExpression", "<strong>{0}{1}</strong> is required", [label, qualifier]));
                        return;
                    }

                    if (label.endsWith("s") || label.endsWith("S")) {
                        validationArray.push(i18NService.get18nValue("messagesection.validation.requiredExpression", "<strong>{0}{1}</strong> are required", [label, qualifier]));
                    } else {
                        validationArray.push(i18NService.get18nValue("messagesection.validation.requiredExpression", "<strong>{0}{1}</strong> is required", [label, qualifier]));
                    }
                }
            });
            
            if (innerValidation == undefined || !innerValidation) {
                const customErrorArray = eventService.beforesubmit_onvalidation(schema, datamap);
                if (customErrorArray != null && Array.isArray(customErrorArray)) {
                    validationArray = validationArray.concat(customErrorArray);
                }
                if (validationArray.length > 0) {
                    //TODO: replace ntification message with in-line error messages
                    alertService.notifymessage("error", validationArray.join(", "));

                    //TODO: scroll to the first error message
                    $("html, body").animate({ scrollTop: 0 }, "fast");
                }
            }
            return validationArray;
        };

        var service = {
            getInvalidLabels: getInvalidLabels,
            validate: validate,
            validateCurrent :validateCurrent,
            validateInlineComposition: validateInlineComposition,
        };

        return service;
    }

})(angular);