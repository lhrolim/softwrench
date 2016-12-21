(function (angular) {
    "use strict";

    angular.module('sw_layout').directive('filterrowrendered', function ($timeout) {
        "ngInject";
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.$last === true) {
                    $timeout(function () {
                        scope.$emit(JavascriptEventConstants.FilterRowRendered);
                    });
                }
            }
        };
    });
  

})(angular);