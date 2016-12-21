(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .directive('onFinishRender', function ($timeout) {

            "ngInject";

            return {
                restrict: 'A',
                link: function (scope, element, attr) {
                    if (scope.$last === true) {
                        $timeout(function () {
                            scope.$emit('ngRepeatFinished');
                        });
                    }
                }
            };
        });


})(angular);