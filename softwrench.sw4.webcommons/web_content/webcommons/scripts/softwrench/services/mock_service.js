var app = angular.module('sw_layout');

app.factory('mockService', function (contextService) {

    return {

        //avoids opening dashboard upon container click
        isMockedContainerDashBoard: function () {
            return contextService.isLocal() && sessionStorage.mockdash == "true";
        },

        isMockMaximo: function () {
            return contextService.isLocal() && sessionStorage.mockmaximo == "true";
        },


    };

});


