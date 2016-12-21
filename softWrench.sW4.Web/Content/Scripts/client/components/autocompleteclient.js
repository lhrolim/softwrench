(function (angular) {
    "use strict";

angular.module('sw_components')
    .service('cmpAutocompleteClient', ["$rootScope", "$log", "$timeout", "fieldService", "crudContextHolderService", function ($rootScope, $log, $timeout, fieldService, crudContextHolderService) {

    return {

        unblock: function (displayable) {
            const element = $("select[data-associationkey='" + displayable.associationKey + "']");
            const combobox = $(element).data('combobox');
            if (combobox != undefined) {
                combobox.enable();
            }
        },

        block: function (displayable) {
            const element = $("select[data-associationkey='" + displayable.associationKey + "']");
            const combobox = $(element).data('combobox');
            if (combobox != undefined) {
                combobox.disable();
            }
        },

        focus:function(displayable) {
            const element = $("select[data-associationkey='" + displayable.associationKey + "']");
            const combobox = $(element).data('combobox');
            if (combobox != undefined) {
                combobox.setFocus();
            }
        },

        refreshFromAttribute: function (scope, displayable, value, availableoptions, datamapId) {
            const log = $log.getInstance("autocompleteclient#refreshFromAttribute", ["association"]);
            if (!crudContextHolderService.associationsResolved() && !availableoptions) {
                log.info("associations not yet resolved,waiting...");
                return;
            }
            const attribute = displayable.attribute;
            
            var labelValue = value;

            const showMissingValues = displayable.rendererParameters && "false" !== displayable.rendererParameters["showmissingoption"];

            if (!nullOrEmpty(value) && availableoptions) {
                //Fixing SWWEB-1349--> the underlying selects have only the labels, so we need to fetch the entries using the original array instead
                let valueMissing = true;
                for (let i = 0; i < availableoptions.length; i++) {
                    if (availableoptions[i].value.trim() === ("" + value).trim()) {
                        valueMissing = false;
                        labelValue = availableoptions[i].label;
                        break;
                    }
                }
                if (valueMissing && showMissingValues) {
                    const missingValue = {
                        "type": "AssociationOption",
                        "value": value,
                        "label": value + " ** unknown to softwrench **"
                    };
                    availableoptions.push(missingValue);
                }
            } else if (displayable.rendererParameters && "true" === displayable.rendererParameters["selectonlyavailableoption"] && availableoptions && availableoptions.length === 1) {
                labelValue = availableoptions[0].label;
                scope.datamap[displayable.target] = availableoptions[0].value;
            }

            var combo;
            if (displayable.applicationPath) {
                let key = displayable.applicationPath;
                if (datamapId) {
                    key += datamapId;
                }
                key = replaceAll(key, "\\.", "_");
                combo = $("select[data-displayablepath=" + key + "]").data('combobox');
                log.debug("setting autocompleteclient {0} to value {1}".format(key, labelValue));
            } else {
                combo = $('#' + RemoveSpecialChars(attribute)).data('combobox');
                log.debug("setting autocompleteclient {0} to value {1}".format(attribute, labelValue));
            }
            //due to a different timeout order this could be called on FF/IE before the availableoptions has been updated
            if (combo != undefined && availableoptions) {
                combo.refresh(labelValue);
            }
        },

        init: function (bodyElement, datamap, schema, scope) {
            const selects = $('select.combobox-dynamic', bodyElement);
            for (let i = 0; i < selects.length; i++) {
                const select = $(selects[i]);
                const associationKey = select.data('associationkey');
                $log.getInstance("autocompleteclient#init", ["association"]).debug("init autocompleteclient {0}".format(associationKey));
                const parent = $(select.parents("div[rel=input-form-repeat]"));
                if (parent.data('selectenabled') === false || select.data('alreadyconfigured')) {
                    continue;
                }
                const fieldMetadata = fieldService.getDisplayablesByAssociationKey(schema, associationKey);
                let minLength = null;
                let pageSize = 300;
                if (fieldMetadata != null && fieldMetadata.length > 0 && fieldMetadata[0].rendererParameters['minLength'] != null) {
                    minLength = parseInt(fieldMetadata[0].rendererParameters['minLength']);
                }
                if (fieldMetadata != null && fieldMetadata.length > 0 && fieldMetadata[0].rendererParameters['pageSize'] != null) {
                    pageSize = parseInt(fieldMetadata[0].rendererParameters['pageSize']);
                }

                select.data('alreadyconfigured', true);
                select.combobox({
                    minLength: minLength,
                    pageSize: pageSize
                });
            }
        }

    };

}]);

})(angular);