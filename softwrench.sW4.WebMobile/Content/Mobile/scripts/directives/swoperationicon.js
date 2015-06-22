(function() {
    "use strict";

    angular.module("softwrench")
        .directive("swOperationIcon", [ function() {
            return {
                restrict: "E",
                templateUrl: "Content/Mobile/templates/directives/swoperationicon.html",
                scope: {
                    operation: "=syncOperation"
                }
            }
        }]);

})();