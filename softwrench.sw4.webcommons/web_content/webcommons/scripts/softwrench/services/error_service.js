(function (angular) {
    "use strict";

    angular.module('sw_layout').config(function ($provide) {
        $provide.decorator("$exceptionHandler", function ($delegate, $injector) {
            return function (exception, cause) {

                //TODO: Replace $injector with notificationViewModel, after circular dependency is fixed
                var notificationViewModel = $injector.get("notificationViewModel");
                notificationViewModel.processJsError(null, exception, cause);

                $delegate(exception, cause);
            };
        });
    });

})(angular);