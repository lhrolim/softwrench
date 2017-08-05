(function (angular) {
    "use strict";

    angular.module("softwrench").directive("messagesection", ["$log", "$rootScope", function ($log, $rootScope) {

        var directive = {
            restrict: "E",
            templateUrl: getResourcePath("Content/Mobile/templates/directives/messagesection.html"),
            scope: {

            },
            controller: ["$scope", function ($scope) {

                $scope.$on('$stateChangeSuccess',
                 function (event, toState, toParams, fromState, fromParams) {
                     $scope.hasValidationError = false;
                     $scope.validationArray = null;
                 });

            }],


            link: function (scope, element, attrs) {

                scope.$on("sw_validationerrors", function (event, validationArray) {
                    $log.get("messagesection#sw_validationerrors").debug('sw_validationerrors#enter');

                    if (validationArray == null || validationArray.length == 0) {
                        return;
                    }

                    scope.hasValidationError = true;
                    scope.validationArray = validationArray;

                });

                scope.removeValidationAlert = function () {
                    scope.hasValidationError = false;
                    scope.validationArray = null;
                };
            }
        };

        return directive;

    }]);

})(angular);

