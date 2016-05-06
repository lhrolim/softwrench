
(function (angular) {
    'use strict';

    function globalSearchService($rootScope, redirectService, contextService, checkpointService) {

        function listClick(datamap, schema) {
            var detail = datamap["appschema"];
            var mode = 'input';
            var param = {};
            param.id = datamap['recordid'];
            var application = datamap['appname'];
            contextService.insertIntoContext('detail.cancel.click', 'globalsearch.list', null);
            redirectService.goToApplicationView(application, detail, mode, null, param, null);
        }

        var service = {
            listClick: listClick
        };

        return service;
    }

    angular.module('webcommons_services').factory('globalSearchService', ['$rootScope', 'redirectService', 'contextService', 'checkpointService', globalSearchService]);
})(angular);
