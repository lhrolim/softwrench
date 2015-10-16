
(function (angular) {
    'use strict';

    function dispatchService($rootScope, applicationService, alertService,contextService) {

        function dispatch(schema, datamap) {
            var localDatamap = datamap;
            if (datamap.fields) {
                localDatamap = datamap.fields;
            }

            var crudData = {
                crud: localDatamap
            };
            var extraParameters = {
                id: localDatamap.ticketid
            }
            return applicationService.invokeOperation("servicerequest", "editdetail", "DispatchWO", crudData, extraParameters);
        }

        var service = {
            dispatch: dispatch
        };

        return service;
    }

    angular.module('maximo_applications').factory('dispatchService', ['$rootScope', 'applicationService', 'alertService','contextService', dispatchService]);
})(angular);
