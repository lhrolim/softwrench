
(function (angular, jQuery) {
    'use strict';



    angular.module('sw_layout').service('submitServiceCommons', ["$injector", "schemaService", "fieldService", "checkpointService", function submitServiceCommons($injector, schemaService,fieldService, checkpointService) {

        function applyTransformationsForSubmission(schemaToSave, originalDatamap, fields) {
            //need an angular.copy to prevent beforesubmit transformation events from modifying the original datamap.
            //this preserves the datamap (and therefore the data presented to the user) in case of a submission failure
            let transformedFields = angular.copy(fields);
            removeNullInvisibleFields(schemaToSave.displayables, transformedFields);
            transformedFields = this.removeExtraFields(transformedFields, true, schemaToSave);
            translateFields(schemaToSave.displayables, transformedFields);
            handleDatamapForMIF(schemaToSave, originalDatamap, transformedFields);
            this.insertAssocationLabelsIfNeeded(schemaToSave, transformedFields);
            return transformedFields;
        }

        function insertAssocationLabelsIfNeeded(schema, datamap) {
            if (schema.properties['addassociationlabels'] !== "true") {
                return;
            }
            const associations = fieldService.getDisplayablesOfTypes(schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            var fn = this;
            $.each(associations, function (key, value) {
                const targetName = value.target;
                const labelName = "#" + targetName + "_label";
                const realValue = fn.getFullObject(value, datamap);
                if (realValue != null && Array.isArray(realValue)) {
                    datamap[labelName] = "";
                    // store result into a string with newline delimiter
                    for (let i = 0; i < realValue.length; i++) {
                        datamap[labelName] += "\\n" + realValue[i].label;
                    }
                }
                else if (realValue != null) {
                    datamap[labelName] = realValue.label;
                }
            });
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


        function isBatch(datamap, schema) {
            //whether it shall run on batch mode
            const batchDisplayable = schemaService.locateDisplayableByQualifier(schema, "batchselector");
            const batchAttribute = batchDisplayable ? batchDisplayable.attribute : null;
            if (schemaService.isPropertyTrue(schema, "batch")) {
                return true;
            } else if (batchAttribute && datamap[batchAttribute]) {
                return datamap[batchAttribute].equalsAny("batch", "true");
            }
            return false;
        }

        ///return if a field which is not on screen (but is not a hidden instance), and whose value is null from the datamap, avoiding sending useless (and wrong) data
        function removeNullInvisibleFields(displayables, datamap) {
            var fn = this;
            displayables.forEach(value => {
                if (fieldService.isNullInvisible(value, datamap)) {
                    delete datamap[value.attribute];
                }
                if (value.displayables != undefined) {
                    removeNullInvisibleFields(value.displayables, datamap);
                }
            });
        }


        function translateFields(displayables, datamap) {
            const fieldsToTranslate = $.grep(displayables, function (e) {
                return e.attributeToServer != null;
            });
            for (let i = 0; i < fieldsToTranslate.length; i++) {
                const field = fieldsToTranslate[i];
                datamap[field.attributeToServer] = datamap[field.attribute];
                delete datamap[field.attribute];
            }
        }

        function createSubmissionParameters(datamap, schema, nextSchemaObj, id, compositionData) {

            var parameters = {
                id,
                userId: datamap[schema.userIdFieldName],
                applicationName: schema.applicationName,
                batch: isBatch(datamap, schema),
                platform: platform(),
                compositionData
            };

            if (compositionData != null) {
                parameters.routeParametersDTO = {
                    dispatcherComposition: compositionData.dispatcherComposition
                };
            }

            if (sessionStorage.mockmaximo === "true") {
                //this will cause the maximo layer to be mocked, allowing testing of workflows without actually calling the backend
                parameters.mockmaximo = true;
            }

            parameters = addSchemaDataToParameters(parameters, schema, nextSchemaObj);
            const checkPointArray = checkpointService.fetchAllCheckpointInfo();
            if (checkPointArray && checkPointArray.length > 0) {
                parameters.routeParametersDTO = {};
                parameters.routeParametersDTO.checkPointData = checkPointArray;
            }

            return parameters;
        }

        function getFormToSubmitIfHasAttachement() {
            const form = $("#crudbodyform");
            var formId = null;
            $('.richtextbox', form).each(function () {

                if (this.contentWindow.asciiData != null && this.contentWindow.asciiData() != undefined && this.contentWindow.asciiData() !== "") {
                    formId = $(this).closest('form');
                }
            });

            //TODO: review circular dependency
            var isValidfn = $injector.get("attachmentService").isValid;

            $('input[type="file"]', form).each(function () {
                if (this.value != null && this.value !== "" && isValidfn(this.value)) {
                    formId = $(this).closest('form');
                }
            });
            return formId;
        }


        function removeExtraFields(datamap, clone, schema) {
            if (!datamap) {
                return null;
            }

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
            $.each(data, function (key) {
                if (data.extrafields[key] != undefined) {
                    if (fieldService.getDisplayableByKey(schema, key) == undefined) {
                        delete data[key];
                    }

                }
            });
            delete data.extrafields;
            return data;
        }


        //Updates fields that were "removed" from an existing record. If the value was originally not null, but is now null,
        //then we update the datamap to " ". This is because the MIF will ignore nulls, causing no change to that field on the ticket.
        function handleDatamapForMIF(schema, originalDatamap, datamap) {
            const displayableFields = fieldService.getDisplayablesOfTypes(schema.displayables, [MetadataConstants.OptionField, MetadataConstants.AssociationField]);
            for (var i = 0, len = displayableFields.length; i < len; i++) {
                const key = displayableFields[i].target == null ? displayableFields[i].attribute : displayableFields[i].target;
                if (originalDatamap && (datamap[key] == null || datamap[key] == undefined) && datamap[key] !== originalDatamap[key]) {
                    datamap[key] = " ";
                }
            }
        }

        const service = {
            addSchemaDataToParameters,
            applyTransformationsForSubmission,
            createSubmissionParameters,
            getFormToSubmitIfHasAttachement,
            insertAssocationLabelsIfNeeded,
            removeExtraFields
        };
        return service;

    }
    ]);


})(angular, jQuery);


