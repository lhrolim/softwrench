(function (angular) {
    "use strict";

    angular.module("webcommons_services").directive("passwordValidation", ["passwordValidationService", "$q", function (passwordValidationService, $q) {
        const directive = {
            restrict: "A",
            require: "ngModel",
            link: function (scope, element, attrs, ngModel) {
                const async = attrs.hasOwnProperty("passwordAsyncValidation");
                scope.passwordValidation = { messages: [] };

                if (async) { // async validation: useful when config is not available to the client
                    ngModel.$asyncValidators.password = function(model, view) {
                        if (ngModel.$isEmpty(model)) return $q.when();

                        return passwordValidationService.validatePasswordAsync(view, { username: attrs["passwordUsernameValue"] })
                            .then(m => {
                                scope.passwordValidation.messages = m;
                                return m.length <= 0
                                    ? $q.when()
                                    : $q.reject();
                            });
                    };
                } else { // sync validation (default): useful when config is available to the client 
                    ngModel.$validators.password = function (model, view) {
                        if (ngModel.$isEmpty(model)) return true;
                        scope.passwordValidation.messages = passwordValidationService.validatePassword(view, { username: attrs["passwordUsernameValue"] });
                        return scope.passwordValidation.messages.length <= 0;
                    };
                }
            }
        };
        return directive;
    }]);

})(angular);