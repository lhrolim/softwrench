(function(angular) {
    "use strict";

    angular.module("softwrench")
        .directive("swOperationIcon", [ function() {
            return {
                restrict: "E",
                templateUrl: getResourcePath("Content/Mobile/templates/directives/swoperationicon.html"),
                scope: {
                    operation: "=syncOperation"
                }
            }
        }]);

})(angular);