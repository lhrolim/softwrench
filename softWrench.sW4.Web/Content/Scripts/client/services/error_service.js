(function (angular) {
    "use strict";

    angular.module('sw_layout').config(function ($provide) {
        $provide.decorator("$exceptionHandler", function ($delegate, $injector) {
            return function (exception, cause) {

                //TODO: Replace $injector with notificationViewModel, after circular dependency is fixed
                const notificationViewModel = $injector.get("notificationViewModel");
                if (exception && exception.startsWith && exception.startsWith("Possibly unhandled rejection")) {
                    return;
                }
                notificationViewModel.processJsError(null, exception, cause);
                $delegate(exception, cause);
                
            };
        });
    });

})(angular);