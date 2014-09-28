var app = angular.module('sw_layout');

app.factory('changeservice', function ($http, redirectService) {

    return {

        opendetail: function (datamap, column) {
            var application = datamap['wonum'] == null ? 'servicerequest' : 'change';
            var id = datamap['wonum'] == null ? datamap['sr_.ticketid'] : datamap['wonum'];
            var parameters = { id: id, popupmode: 'browser' };
            redirectService.goToApplicationView(application, 'detail', 'output', null, parameters);
        }
    };


});