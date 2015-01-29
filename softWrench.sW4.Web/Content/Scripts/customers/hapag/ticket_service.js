﻿var app = angular.module('sw_layout');

app.factory('ticketservice', function ($http, redirectService,alertService) {

    return {


        submitaction: function (datamap, schema) {

            if (datamap.customAction == "0") {
                alertService.alert("please select either agree or disagree");

            } else {

                var parameters = {
                    ticketid: fieldService.getId(datamap, schema),
                    status: datamap.customAction,
                    crud: datamap
                }
                var urlToUse = url("api/data/operation/servicerequest/SubmitAction?platform=web&id=" + parameters.ticketid);
                parameters = addCurrentSchemaDataToJson(parameters, schema);
                var json = angular.toJson(parameters);
                $http.post(urlToUse, json).success(function () {
                    datamap.status = datamap.customAction;
                    alertService.alert("Status changed successfully");
                });
            }
        },
                
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