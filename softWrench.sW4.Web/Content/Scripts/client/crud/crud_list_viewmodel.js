
(function (angular) {
    'use strict';

    class crudListViewModel {

        constructor($rootScope) {
            this.$rootScope = $rootScope;
        }

        //TODO: refactor it to use classes instead...
        /**
         * 
         * @param {} applicationListResultObj represents a ApplicaitonListResult from server side
         * @param {} panelId the panelid, usually null, but could be #modal or a dashboard id
         * @returns {} 
         */
        initGridFromServerResult(applicationListResultObj, panelId) {
            return this.$rootScope.$broadcast(JavascriptEventConstants.GRID_REFRESHED, applicationListResultObj, panelId);
        }

        initGridFromDatamapAndSchema(datamap, schema, panelId) {

            const applicationListResultObj = {
                datamap,
                schema
            }

            return this.$rootScope.$broadcast(JavascriptEventConstants.GRID_REFRESHED, applicationListResultObj, panelId);
        }

    }

    crudListViewModel.$inject = ['$rootScope'];

    angular.module('sw_layout').service('crudlistViewmodel', crudListViewModel);
    


})(angular);
