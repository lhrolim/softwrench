(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('mockService', ["contextService", function (contextService) {

    return {
        //avoids opening dashboard upon container click
        isMockedContainerDashBoard: function () {
            return contextService.isLocal() && sessionStorage.mockdash == "true";
        },

        isMockMaximo: function () {
            return contextService.isLocal() && sessionStorage.mockmaximo == "true";
        },
    };

}]);

})(angular);