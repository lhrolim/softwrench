(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('cmpAutocompleteClient', ["$rootScope", "$log", "$timeout", "fieldService", function ($rootScope, $log, $timeout, fieldService) {

    return {

        unblock: function (displayable) {
            var element = $("select[data-associationkey='" + displayable.associationKey + "']");
            var combobox = $(element).data('combobox');
            if (combobox != undefined) {
                combobox.enable();
            }
        },

        block: function (displayable) {
            var element = $("select[data-associationkey='" + displayable.associationKey + "']");
            var combobox = $(element).data('combobox');
            if (combobox != undefined) {
                combobox.disable();
            }
        },

        focus:function(displayable) {
            var element = $("select[data-associationkey='" + displayable.associationKey + "']");
            var combobox = $(element).data('combobox');
            if (combobox != undefined) {
                combobox.setFocus();
            }
        },

        refreshFromAttribute: function (scope,displayable, value, availableoptions) {
            var attribute = displayable.attribute;

            var log = $log.getInstance("autocompleteclient#refreshFromAttribute", ["association"]);
            var labelValue = value;
            if (!nullOrEmpty(value) && availableoptions) {
                //Fixing SWWEB-1349--> the underlying selects have only the labels, so we need to fetch the entries using the original array instead
                var valueMissing = true;
                for (var i = 0; i < availableoptions.length; i++) {
                    if (availableoptions[i].value.trim() === ("" + value).trim()) {
                        valueMissing = false;
                        labelValue = availableoptions[i].label;
                    }
                }
                if (valueMissing) {
                    var missingValue = {
                        "type": "AssociationOption",
                        "value": value,
                        "label": value + " ** unknown to softwrench **"
                    }
                    availableoptions.push(missingValue);
                }
            } else if (displayable.rendererParameters && "true" === displayable.rendererParameters["selectonlyavailableoption"] && availableoptions && availableoptions.length === 1) {
                labelValue = availableoptions[0].label;
                scope.datamap[displayable.target] = availableoptions[0].value;
            }
            var combo = $('#' + RemoveSpecialChars(attribute)).data('combobox');
            log.debug("setting autocompleteclient {0} to value {1}".format(attribute, labelValue));
            //due to a different timeout order this could be called on FF/IE before the availableoptions has been updated
            if (combo != undefined && availableoptions) {
                combo.refresh(labelValue);
            }
        },

        init: function (bodyElement, datamap, schema, scope) {
            var selects = $('select.combobox-dynamic', bodyElement);
            for (var i = 0; i < selects.length; i++) {
                var select = $(selects[i]);
                var associationKey = select.data('associationkey');
                $log.getInstance("autocompleteclient#init", ["association"]).debug("init autocompleteclient {0}".format(associationKey));
                var parent = $(select.parents("div[rel=input-form-repeat]"));
                if (parent.data('selectenabled') === false || select.data('alreadyconfigured')) {
                    continue;
                }

                var fieldMetadata = fieldService.getDisplayablesByAssociationKey(schema, associationKey);
                var minLength = null;
                var pageSize = 300;
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