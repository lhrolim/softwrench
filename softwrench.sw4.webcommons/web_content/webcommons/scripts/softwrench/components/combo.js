(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('cmpCombo', ["$rootScope", "$timeout", "i18NService", function ($rootScope, $timeout, i18NService) {

    return {
        refreshFromAttribute: function (value, associationOptions) {
            if (!nullOrEmpty(value) && associationOptions) {
                var valueMissing = true;
                for (var i = 0; i < associationOptions.length; i++) {
                    if (associationOptions[i].value.trim() === ("" + value).trim()) {
                        valueMissing = false;
                        break;
                    }
                }
                if (valueMissing) {
                    var missingValue = {
                        "type": "AssociationOption",
                        "value": value,
                        "label": value + " ** unknown to softwrench **"
                    }
                    associationOptions.push(missingValue);
                }
            }
        }
    };
}]);

})(angular);