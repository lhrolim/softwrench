(function (angular) {
    'use strict';

    angular.module('webcommons_services').service('validationService', ['$log','$q', 'i18NService', 'fieldService', '$rootScope', 'dispatcherService', 'expressionService', 'eventService', 'compositionCommons', 'schemaService', 'alertService', 'crudContextHolderService', "passwordValidationService", validationService]);

    function validationService($log,$q, i18NService, fieldService, $rootScope, dispatcherService, expressionService, eventService, compositionCommons, schemaService, alertService, crudContextHolderService, passwordValidationService) {

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

        function getValidationPattern(validationType) {
            let regexPattern = '';
            switch (validationType) {
                case "number":
                    regexPattern = new RegExp(/^\d+$/);
                    break;

                case "phonenumber":
                    regexPattern = new RegExp(/^\d+$/);
                    break;

                case "email":
                    regexPattern = new RegExp(/^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/, 'i');
                    break;
            }

            return regexPattern;
        }

        function getValidationPatterString(validationType) {
            switch (validationType) {
                case "number":
                    return "^\\d+$";
                case "phonenumber":
                    return "^\\d+$";

                case "email":
                    return "^(([^<>()\\[\\]\\.,;:\\s@\\\"]+(\\.[^<>()\\[\\]\\.,;:\\s@\\\"]+)*)|(\\\".+\\\"))@(([^<>()[\\]\\.,;:\\s@\\\"]+\\.)+[^<>()[\\]\\.,;:\\s@\\\"]{2,})$";
            }
            return "";
        }

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
           
            return this.validate(schema, schema.displayables, datamap);
        }

        function validatePromise(schema,datamap,crudFormErrors) {
            if (schema == null || datamap == null) {
                return $q.when();
            }
            if (sessionStorage.mockclientvalidation === "true") {
                //ignoring validation due to presence of flag
                return $q.when();
            }

            const arr = this.validate(schema, schema.displayables, datamap,crudFormErrors);
            const deferred =$q.defer();
            if (arr.length > 0) {
                return $q.reject(arr);
            } 
            return $q.when();
            
        }

        function validate(schema, displayables, datamap, angularformerrors={}, innerValidation=false) {
            const log = $log.get("validationService#validate",["validation"]);

            if (!innerValidation) {
                $log.get("validationService#validate",["save"]).info(`init validation for schema ${schema.schemaId} of application ${schema.applicationName}`);
            }
            

            let validationArray = [];
            const allDisplayables = schemaService.flattenDisplayables(displayables, null, datamap);

            /**
             * Resolves a NgModel that has validation errors from a form's validation error array.
             * 
             * @param {Array<NgForm.$error>} validation errors from an NgForm 
             * @param {String} type type of the validation error e.g. "email", "password"
             * @param {String} name name of the form field 
             * @returns {NgModel} controller instance with validation error (null if it has no error).
             */
            const getNgModel = (errors, type, name) => {
                const typedError = errors[type];
                if (!typedError) return null;
                // in case it's an array of NgModel or an array of NgForm
                const error = /*[ngModel]*/ typedError.find(e => e.$name === name) || /*[ngForm]*/ typedError[0][name];
                return !error && angular.isArray(typedError) && !!typedError[0].$error // search did not match but has child forms
                    ? getNgModel(typedError[0].$error, type, name) // recursively search child forms
                    : error; // end of search
            };

            const isInvalidEmail = (errors, name) => {
                const error = getNgModel(errors, "email", name);
                return !!error && error.$invalid;
            };

            allDisplayables.forEach(displayable => {
                var label = displayable.label;
                log.debug("performing validation on field {0}".format(label));
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

                    if (!label.equalIc("alias") && (label.endsWith("s") || label.endsWith("S"))) {
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

        const service = {
            getInvalidLabels,
            validate,
            validateCurrent,
            validatePromise,
            validateInlineComposition,
            getValidationPattern,
            getValidationPatterString
        };

        return service;
    }

})(angular);