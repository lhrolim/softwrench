var app = angular.module('sw_layout');

app.factory('relatedrecordservice', function ($http, redirectService, alertService, contextService) {

    "ngInject";

    return {
        
        opendetail: function (datamap, schema) {

            var id = datamap['relatedreckey'];
            var entity = datamap['relatedrecclass'];
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
                var module = contextService.retrieveFromContext('currentmodule');
                if (module && module.equalsAny("tom", "itom")) {
                    contextService.insertIntoContext("currentmodulenewwindow", module);    
                }
                
                redirectService.goToApplicationView(application, 'detail', 'output', null, parameters);
            } else {
                alertService.alert('This Application {0} is not supported'.format(entity));
            }
        }
    };


});