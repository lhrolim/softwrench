(function (angular) {
    "use strict";

    angular.module("sw_mobile_services").constant("routeConstants", {
        events: {
            'sameStateTransition': "sw:$state:transition:same"
        }
    });

})(angular);