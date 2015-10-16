
(function (jQuery) {
    'use strict';

    angular.module('sw_layout').factory('submitService', ['$rootScope', 'fieldService', 'contextService', 'checkpointService', 'alertService', 'schemaService', 'attachmentService', submitService]);

    function submitService($rootScope, fieldService, contextService, checkpointService, alertService, schemaService, attachmentService) {

   


        ///used for ie9 form submission
        function submitForm(formToSubmit, parameters, jsonString, applicationName) {
            // remove from session the redirect url... the redirect url will be returned when the form submit response comes from server
            contextService.deleteFromContext("swGlobalRedirectURL");

            for (var i in parameters) {
                formToSubmit.append("<input type='hidden' name='" + i + "' value='" + parameters[i] + "' />");
            }
            if (sessionStorage.mockmaximo == "true") {
                formToSubmit.append("<input type='hidden' name='%%mockmaximo' value='true'/>");
            }


            formToSubmit.append("<input type='hidden' name='currentmodule' value='" + contextService.retrieveFromContext('currentmodule') + "' />");

            formToSubmit.append("<input type='hidden' name='application' value='" + applicationName + "' />");
            formToSubmit.append("<input type='hidden' name='json' value='" + replaceAll(jsonString, "'", "&apos;") + "' />");

            // start spin befor submitting form
            var savingMain = true === $rootScope.savingMain;
            startSpin(savingMain);

            // submit form
            formToSubmit.attr("action", url("/Application/Input"));
            formToSubmit.submit();
        };

        function addSchemaDataToParameters(parameters, schema, nextSchema) {
            parameters["currentSchemaKey"] = schema.schemaId + "." + schema.mode + "." + platform();
            if (nextSchema != null && nextSchema.schemaId != null) {
                parameters.routeParametersDTO["nextSchemaKey"] = nextSchema.schemaId + ".";
                if (nextSchema.mode != null) {
                    parameters.routeParametersDTO["nextSchemaKey"] += nextSchema.mode;
                }
                parameters.routeParametersDTO["nextSchemaKey"] += "." + platform();
            }
            return parameters;
        };


        ///return if a field which is not on screen (but is not a hidden instance), and whose value is null from the datamap, avoiding sending useless (and wrong) data
        function removeNullInvisibleFields(displayables, datamap) {
            var fn = this;
            $.each(displayables, function (key, value) {
                if (fieldService.isNullInvisible(value, datamap)) {
                    delete datamap[value.attribute];
                }
                if (value.displayables != undefined) {
                    fn.removeNullInvisibleFields(value.displayables, datamap);
                }

            });
        }



        function translateFields(displayables, datamap) {
            var fieldsToTranslate = $.grep(displayables, function (e) {
                return e.attributeToServer != null;
            });
            for (var i = 0; i < fieldsToTranslate.length; i++) {
                var field = fieldsToTranslate[i];
                datamap[field.attributeToServer] = datamap[field.attribute];
                delete datamap[field.attribute];
            }
        }

        function getFormToSubmitIfHasAttachement(displayables, datamap) {

            var form = $("#crudbodyform");
            var formId = null;
            $('.richtextbox', form).each(function () {

                if (this.contentWindow.asciiData != null && this.contentWindow.asciiData() != undefined && this.contentWindow.asciiData() != "") {
                    formId = $(this).closest('form');
                }
            });

            var isValidfn = attachmentService.isValid;

            $('input[type="file"]', form).each(function () {
                if (this.value != null && this.value != "" && isValidfn(this.value)) {
                    formId = $(this).closest('form');
                }
            });
            return formId;
        }

        function removeExtraFields(datamap, clone, schema) {
            if (!datamap.extrafields) {
                if (clone) {
                    return jQuery.extend(true, {}, datamap);
                }

                return datamap;
            }

            var data = datamap;
            if (clone) {
                data = jQuery.extend(true, {}, datamap);
            }
            $.each(data, function (key, value) {
                if (data.extrafields[key] != undefined) {
                    if (fieldService.getDisplayableByKey(schema, key) == undefined) {
                        delete data[key];
                    }

                }
            });
            delete data.extrafields;
            return data;
        }


        function createSubmissionParameters(datamap, schema, nextSchemaObj, id,dispatcherComposition) {
            var parameters = {};
            if (sessionStorage.mockmaximo === "true") {
                //this will cause the maximo layer to be mocked, allowing testing of workflows without actually calling the backend
                parameters.mockmaximo = true;
            }
            parameters.routeParametersDTO = {};
            parameters.routeParametersDTO.dispatcherComposition = dispatcherComposition;
            parameters = addSchemaDataToParameters(parameters, schema, nextSchemaObj);
            var checkPointArray = checkpointService.fetchCheckpoint();
            if (checkPointArray && checkPointArray.length > 0) {
                parameters.routeParametersDTO.checkPointData = checkPointArray;
            }
            parameters.applicationName = schema.applicationName;
            parameters.id = id;

            //whether it shall run on batch mode
            var batchDisplayable = schemaService.locateDisplayableByQualifier(schema, "batchselector");
            var batchAttribute = batchDisplayable ? batchDisplayable.attribute : null;
            if (schemaService.isPropertyTrue(schema, "batch")) {
                parameters.batch = true;
            }
            else if (batchAttribute && datamap[batchAttribute]) {
                parameters.batch = datamap[batchAttribute].equalsAny("batch", "true");
            }


            parameters.platform = platform();
            return parameters;
        }



        //Updates fields that were "removed" from an existing record. If the value was originally not null, but is now null,
        //then we update the datamap to " ". This is because the MIF will ignore nulls, causing no change to that field on the ticket.
        function handleDatamapForMIF(schema, originalDatamap, datamap) {
            var displayableFields = fieldService.getDisplayablesOfTypes(schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            for (var i = 0, len = displayableFields.length; i < len; i++) {
                var key = displayableFields[i].target == null ? displayableFields[i].attribute : displayableFields[i].target;

                if ((datamap[key] == null || datamap[key] == undefined) && datamap[key] != originalDatamap[key]) {
                    datamap[key] = " ";
                }
            }
        }

        function submitConfirmation(event, datamap, parameters) {
            var message = "Are you sure you want to save changes to this record?";
            return alertService.confirm(null, null, function () {
                parameters.continue();
            }, message);
        }

        var service = {
            submitForm: submitForm,
            removeNullInvisibleFields: removeNullInvisibleFields,
            translateFields: translateFields,
            getFormToSubmitIfHasAttachement: getFormToSubmitIfHasAttachement,
            removeExtraFields: removeExtraFields,
            createSubmissionParameters: createSubmissionParameters,
            handleDatamapForMIF: handleDatamapForMIF,
            submitConfirmation: submitConfirmation
        };

        return service;

    }
})(jQuery);

