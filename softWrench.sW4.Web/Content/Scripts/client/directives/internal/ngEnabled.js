(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .directive("ngEnabled", function() {

            return {
                restrict: 'A',
                link: function(scope, element, attrs) {
                    scope.$watch(attrs.ngEnabled, function(val) {
                        if (val)
                            element.removeAttr("disabled");
                        else
                            element.attr("disabled", "disabled");
                    });
                }
            };
        });


})(angular);