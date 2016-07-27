(function (angular) {
    "use strict";

    angular.module("softwrench").directive("autosizeTextarea", [function () {
        const directive = {

            link: function (scope, element, attrs) {
                var width = $(element).width();

                scope.$watch(attrs.ngModel, function (newValue) {
                    var lines = 1;

                    if (!!newValue) {

                        //number of character per line, based on 30 characters on a 360px wide device
                        var characters = Math.ceil(width * (32 / 201));
                        lines = Math.ceil(newValue.length / characters);
                    }

                    element[0].rows = lines;
                });
            }
        };

        return directive;
    }]);

})(angular);