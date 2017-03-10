(function (angular) {
    "use strict";

    class comboComponent {

        constructor($rootScope, $timeout, i18NService) {
            this.$rootScope = $rootScope;
            this.$timeout = $timeout;
            this.i18NService = i18NService;
        }

        refreshFromAttribute(value, associationOptions, displayable) {
            const showMissingValues = displayable.rendererParameters && "false" !== displayable.rendererParameters["showmissingoption"];

            if (!nullOrEmpty(value) && associationOptions) {
                let valueMissing = true;
                for (let i = 0; i < associationOptions.length; i++) {
                    if (associationOptions[i].value.trim() === ("" + value).trim()) {
                        valueMissing = false;
                        break;
                    }
                }
                if (valueMissing && showMissingValues) {
                    const missingValue = {
                        "type": "AssociationOption",
                        "value": value,
                        "label": value + " ** unknown to softwrench **"
                    };
                    associationOptions.push(missingValue);
                }
            }

        }

    }

    comboComponent.$inject = ['$rootScope', '$timeout', 'i18NService'];

    angular.module('sw_components').service('cmpCombo', comboComponent);

})(angular);