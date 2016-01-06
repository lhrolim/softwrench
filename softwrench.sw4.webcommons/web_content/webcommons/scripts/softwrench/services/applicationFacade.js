(function (angular) {
    "use strict";

angular.module('sw_layout')
/*
/Just a holder for multiple inner services
*/
.factory('applicationFacade', ["compositionService", "printService", "tabsService", function (compositionService, printService, tabsService) {

    return {
        tabsService: function () {
            return tabsService;
        },

        compositionService: function () {
            return compositionService;
        },
        printService: function () {
            return printService;
        },

    };

}]);

})(angular);