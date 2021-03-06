﻿var app = angular.module('sw_layout');

app.factory('relatedrecordservice', function ($http, redirectService, alertService, contextService) {

    "ngInject";

    return {
        
        opendetail: function (datamap, schema) {

            var id = datamap['relatedreckey'];
            var hmachash = datamap['hmachash'];
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
                var parameters = { id: id, hmachash: hmachash, popupmode: 'browser' };
                var module = contextService.retrieveFromContext('currentmodule');
                if (module && module.equalsAny("tom", "itom")) {
                    contextService.insertIntoContext("currentmodulenewwindow", module);    
                }
                
                redirectService.goToApplicationView(application, 'detail', 'output', null, parameters);
            } else {
                alertService.alert('This Application {0} is not supported'.format(entity));
            }
        },

        openChildCI: function (datamap, schema) {
            var id = datamap['child_.ciid'];
            var hmachash = datamap['hmachash'];
            var parameters = { id: id, hmachash: hmachash, popupmode: 'browser' };
            redirectService.goToApplicationView("ci", 'detail', 'output', null, parameters);
        },

        openParentCI: function (datamap, schema) {

            var id = datamap['parent_.ciid'];
            var hmachash = datamap['hmachash'];
            var parameters = { id: id, hmachash: hmachash, popupmode: 'browser' };
            redirectService.goToApplicationView("ci", 'detail', 'output', null, parameters);
        }

    };


});