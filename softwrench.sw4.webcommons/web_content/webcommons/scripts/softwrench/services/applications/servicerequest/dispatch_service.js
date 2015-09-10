
(function (angular) {
    'use strict';

    function dispatchService($rootScope, applicationService, alertService) {

        function dispatch(schema, datamap) {
            var localDatamap = datamap;
            if (datamap.fields) {
                localDatamap = datamap.fields;
            }

            if (!localDatamap.owner && !localDatamap.ownergroup) {
                alertService.alert("Owner or owner group must be filled before this action can be performed.");
                return 0;
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

    angular.module('maximo_applications').factory('dispatchService', ['$rootScope', 'applicationService', 'alertService', dispatchService]);
})(angular);
