var app = angular.module('sw_layout');

app.factory('oferringservice', function ($http, redirectService, formatService, fieldService) {

    "ngInject";

    return {

        /// <summary>
        ///  Open the offering as an ordinary SR detail
        /// 
        /// </summary>
        /// <param name="datamap"></param>
        /// <param name="column"></param>
        openDetail: function (datamap, column) {
            var parameters = { id: datamap['ticketid'], popupmode: 'browser' };
            redirectService.goToApplicationView('servicerequest', 'detail', 'output', null, parameters);
        },


    };


});