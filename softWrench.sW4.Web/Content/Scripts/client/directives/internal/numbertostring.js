(function (angular) {
    "use strict";

    angular.module('sw_layout').directive('numberToString', function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                // Some non-string values coming from the rootdatamap are breaking the string binding, so we need to format them to string
                // https://controltechnologysolutions.atlassian.net/browse/SWWEB-2042 : first issue
                ngModel.$formatters.push(function (value) {
                    if (value == null) {
                        return null;
                    }
                    return '' + value;
                });
            }
        };
    });


})(angular);