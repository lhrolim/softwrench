
(function (angular) {
    'use strict';

    class crudListViewModel {

        constructor($rootScope, crudContextHolderService) {
            this.$rootScope = $rootScope;
            this.crudContextHolderService = crudContextHolderService;
        }

        /**
         * 
         * @param {} applicationListResultObj represents a ApplicaitonListResult from server side
         * @param {} panelId the panelid, usually null, but could be #modal or a dashboard id
         * @returns {} 
         */
        initGridFromServerResult(applicationListResultObj, panelId) {
            
            this.crudContextHolderService.gridLoaded(applicationListResultObj, panelId);
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

    crudListViewModel.$inject = ['$rootScope', 'crudContextHolderService'];

    angular.module('sw_layout').service('crudlistViewmodel', crudListViewModel);
    


})(angular);
