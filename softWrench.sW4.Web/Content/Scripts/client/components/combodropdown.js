var app = angular.module('sw_layout');

app.factory('cmpComboDropdown', function ($rootScope, $timeout, i18NService) {

    return {

        block:function(attribute) {
            var element = $("select[data-associationkey='" + attribute + "']");
            $(element).multiselect('destroy');
        },

        unblock: function (attribute) {
            var element = $("select[data-associationkey='" + attribute + "']");
            $(element).multiselect('refresh');
        },

        refresh: function (element) {
            if (element.find('option').size() > 0) {

                element.multiselect({
                    nonSelectedText: 'Select One',
                    includeSelectAllOption: true,
                    enableCaseInsensitiveFiltering: true,
                    disableIfEmpty:true
                });
            }
        },

        refreshFromAttribute: function (attribute) {
            var combo = $('#' + RemoveSpecialChars(attribute));
            this.refresh(combo);
        },

        getSelectedTexts: function (fieldMetadata) {
            var combo = $('#' + RemoveSpecialChars(fieldMetadata.attribute));
            var custombuttontext = fieldMetadata.rendererParameters['custombuttontext'];
            var selectedTexts = new Array();

            combo.find(':selected').each(function () {
                selectedTexts.push($(this).text());
            });
            if (selectedTexts.length > 0) {
                var selected = i18NService.get18nValue('combodropdown.selected', 'selected') + ': ';
                var text = fieldMetadata.label + ' ' + selected + selectedTexts.length;
                if (!nullOrUndef(custombuttontext)) {
                    text = custombuttontext + ' ' + selected + selectedTexts.length;
                }
                combo[0].setAttribute('custombuttontext', text);
            }
            return selectedTexts;
        },

        init: function (bodyElement) {
            var fn = this;
            $timeout(
                function () {

                    $('.multiselect', bodyElement).each(function (index) {
                        fn.refresh($(this));
                    });

                }, 0, false);
        }



    }

});


