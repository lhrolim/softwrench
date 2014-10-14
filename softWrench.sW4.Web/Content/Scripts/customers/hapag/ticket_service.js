var app = angular.module('sw_layout');

app.factory('ticketservice', function ($http, redirectService) {

    return {
                
        opendetail: function (datamap, displayables) {

            if (datamap['history'] == 'true') {
                return;
            }            

            var id = datamap['ticketid'];
            var entity = datamap['class'];
            var application = null;
            if (entity == 'SR') {
                application = 'servicerequest';
            } else if (entity == 'CHANGE') {
                application = 'change';
            } else if (entity == 'INCIDENT') {
                application = 'incident';
            } else if (entity == 'PROBLEM') {
                application = 'problem';
            }

            if (application != null) {
                var parameters = { id: id, popupmode: 'browser' };
                redirectService.goToApplicationView(application, 'detail', 'output', null, parameters);
            } else {
                alertService.alert('This Application {0} is not supported'.format(entity));
            }
            
        }
    };


});