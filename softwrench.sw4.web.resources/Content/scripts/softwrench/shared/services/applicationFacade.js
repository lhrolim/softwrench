var app = angular.module('sw_layout');

/*
/Just a holder for multiple inner services
*/
app.factory('applicationFacade', function (i18NService,compositionService,printService,tabsService) {



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

});


