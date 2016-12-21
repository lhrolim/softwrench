
(function (angular) {
    'use strict';

    function dispatchService($rootScope, applicationService, alertService,contextService) {

        function dispatchWO(schema, datamap) {
            var localDatamap = datamap;
            
            var crudData = {
                crud: localDatamap
            };
            var extraParameters = {
                id: localDatamap.ticketid
            }
            return applicationService.invokeOperation("servicerequest", "editdetail", "DispatchWO", crudData, extraParameters);
        }

        function dispatchIncident(schema, datamap) {
            var localDatamap = datamap;

            var crudData = {
                crud: localDatamap
            };
            var extraParameters = {
                id: localDatamap.wonum
            };
            return applicationService.invokeOperation("servicerequest", "editdetail", "DispatchIncident", crudData, extraParameters);
        }

        var service = {
            dispatchWO: dispatchWO,
            dispatchIncident: dispatchIncident
        };

        return service;
    }

    angular.module('maximo_applications').service('dispatchService', ['$rootScope', 'applicationService', 'alertService','contextService', dispatchService]);
})(angular);
