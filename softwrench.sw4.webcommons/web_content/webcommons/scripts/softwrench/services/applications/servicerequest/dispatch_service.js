
(function () {
    'use strict';

    angular.module('maximo_applications').factory('dispatchService', ['$rootScope', 'applicationService', 'alertService', 'commandService', dispatchService]);

    function dispatchService($rootScope, applicationService, alertService, commandService) {

        var service = {
            dispatch: dispatch
        };

        return service;

        function dispatch(schema, datamap) {
            var localDatamap = datamap;
            if (datamap.fields) {
                localDatamap = datamap.fields;
            }

            if (!localDatamap.owner && !localDatamap.ownergroup) {
                alertService.alert("Owner or owner group must be filled before dispatch can be performed.");
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

    }
})();
