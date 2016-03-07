(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('cmpComboDropdown', ["$rootScope", "$timeout", "i18NService", "fieldService", "dispatcherService", function ($rootScope, $timeout, i18NService, fieldService, dispatcherService) {

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
                element.multiselect('refresh');
            }
        },

        refreshFromAttribute: function (attribute) {
            var combo = $('#' + RemoveSpecialChars(attribute));
            this.refresh(combo);
        },

        refreshList: function (attribute) {
            var combo = $('#' + RemoveSpecialChars(attribute));
            combo.multiselect('rebuild');
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

        init: function (bodyElement,schema) {
            var fn = this;
            $timeout(
                function () {

                    $("select.multiselect", bodyElement).each(function (index) {
                        var element = $(this);
                        var associationKey = element.data('associationkey');
                        var fieldMetadata = fieldService.getDisplayablesByAssociationKey(schema, associationKey)[0];
                        var buttontext = fieldMetadata.rendererParameters["buttontext"];
                        var maxHeight = fieldMetadata.rendererParameters["maxheight"];
                        var filterDisabled = "false" === fieldMetadata.rendererParameters["filterenabled"];

                        if (!buttontext) {
                            buttontext = "Select Values";
                        }
                        
                        var multiselectOptions = {
                            nonSelectedText: buttontext,
                            includeSelectAllOption: true,
                            enableCaseInsensitiveFiltering: !filterDisabled,
                            disableIfEmpty: true,
                            numberDisplayed: 1,
                            maxHeight: maxHeight
                        };

                        var customDropdownFunctionString = fieldMetadata.rendererParameters["customdropdownfunction"];
                        if (customDropdownFunctionString) {
                            var customDropdownFunction = dispatcherService.loadServiceByString(customDropdownFunctionString);
                            if (customDropdownFunction) {
                                customDropdownFunction(fieldMetadata, element, multiselectOptions);
                            }
                        }

                        element.multiselect(multiselectOptions);
                        fn.refresh(element);
                    });

                }, 0, false);
        }

    };

}]);

})(angular);