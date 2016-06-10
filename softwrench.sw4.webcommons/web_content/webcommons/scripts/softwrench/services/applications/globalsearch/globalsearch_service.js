
(function (angular) {
    'use strict';

    function globalSearchService($rootScope, redirectService, contextService, checkpointService) {

        function listClick(datamap, schema) {
            const detail = datamap["appschema"];
            const mode = 'input';
            const param = {};
            param.id = datamap['recordid'];
            const application = datamap['appname'];
            contextService.insertIntoContext('detail.cancel.click', 'globalsearch.list', null);
            redirectService.goToApplicationView(application, detail, mode, null, param, null);
        }

        const service = {
            listClick: listClick
        };
        return service;
    }

    angular.module('webcommons_services').factory('globalSearchService', ['$rootScope', 'redirectService', 'contextService', 'checkpointService', globalSearchService]);
})(angular);
