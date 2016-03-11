(function (angular) {
    'use strict';

    function worklogService($http, redirectService, crudContextHolderService) {
        
        function editWorklog(datamap, metadata, schema) {
            var test = crudContextHolderService.currentSchema();
            return redirectService.openAsModal("worklog", "detail", {}, datamap);
        }

        var service = {
            editWorklog: editWorklog
        };

        return service;
    }

    angular.module("sw_layout").factory("worklogService", ["$http", "redirectService", "crudContextHolderService", worklogService]);


})(angular);