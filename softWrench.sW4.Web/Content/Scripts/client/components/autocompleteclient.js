var app = angular.module('sw_layout');

app.factory('cmpAutocompleteClient', function ($rootScope, $timeout) {

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

        init: function (bodyElement, scope) {
            var selects = $('select.combobox-dynamic', bodyElement);
            for (var i = 0; i < selects.length; i++) {
                var select = $(selects[i]);
                var associationKey = select.data('associationkey');
                var parent = $(select.parents("div[rel=input-form-repeat]"));
                if (parent.data('selectenabled') == false || select.data('alreadyconfigured')) {
                    continue;
                }
                select.data('alreadyconfigured', true);
                select.combobox();
                //                if (scope.blockedassociations[associationKey] == true) {
                //                    combo.disable();
                //                }
            }
        }



    }

});


