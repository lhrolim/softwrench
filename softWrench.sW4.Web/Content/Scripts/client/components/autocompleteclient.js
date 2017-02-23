var app = angular.module('sw_layout');

app.factory('cmpAutocompleteClient', function ($rootScope, $timeout, fieldService, contextService) {

    "ngInject";

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
            $(element).data('combobox').disable();
        },

        refreshFromAttribute: function (attribute) {
            var combo = $('#' + RemoveSpecialChars(attribute)).data('combobox');
            if (combo != undefined) {
                combo.refresh();
            }
        },

        init: function (bodyElement, datamap, schema, scope) {
            var selects = $('select.combobox-dynamic', bodyElement);
            for (var i = 0; i < selects.length; i++) {
                var select = $(selects[i]);
                var associationKey = select.data('associationkey');
                var parent = $(select.parents("div[rel=input-form-repeat]"));
                if (parent.data('selectenabled') == false || select.data('alreadyconfigured')) {
                    continue;
                }

                var fieldMetadata = fieldService.getDisplayablesByAssociationKey(schema, associationKey);
                var minLength = null;
                var pageSize = contextService.isLocal() ? 30 : 300;
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



    }

});


