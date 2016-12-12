
(function (angular) {
    'use strict';

    angular.module('sw_layout').factory('crudlistViewmodel', ['$rootScope', crudListViewModel]);



    function crudListViewModel($rootScope) {


        //TODO: refactor it to use classes instead...
        /**
         * 
         * @param {} applicationListResultObj represents a ApplicaitonListResult from server side
         * @param {} panelId the panelid, usually null, but could be #modal or a dashboard id
         * @returns {} 
         */
        function initGridFromServerResult(applicationListResultObj, panelId) {
            return $rootScope.$broadcast(JavascriptEventConstants.GRID_REFRESHED, applicationListResultObj, panelId);
        }

        function initGridFromDatamapAndSchema(datamap, schema, panelId) {

            const applicationListResultObj = {
                datamap: datamap,
                schema: schema
            }

            return $rootScope.$broadcast(JavascriptEventConstants.GRID_REFRESHED, applicationListResultObj, panelId);
        }

        const service = {
            initGridFromDatamapAndSchema,
            initGridFromServerResult
        };

        return service;
    }
})(angular);
