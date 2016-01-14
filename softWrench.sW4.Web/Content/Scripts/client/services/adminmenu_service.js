﻿(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('adminMenuService', ["menuService", "redirectService", "contextService", "logoutService", "crudContextHolderService",
        function (menuService, redirectService, contextService, logoutService, crudContextHolderService) {

    return {
        doAction: function (title, controller, action, parameters, target) {
            menuService.setActiveLeaf(target);
            crudContextHolderService.clearCrudContext();
            return redirectService.redirectToAction(title, controller, action, parameters);
        },

        loadApplication: function (applicationName, schemaId, mode, id) {
            var parameters = {
                Id: id
            }
            redirectService.goToApplicationView(applicationName, schemaId, mode, null, parameters, null);
        },

        myProfile: function () {
            var crudContext = {
                detail_next: "0",
                detail_previous: "-1"
            };
            contextService.insertIntoContext("crud_context", crudContext);
            var id = contextService.getUserData().maximoPersonId;
            this.loadApplication('Person', 'myprofiledetail', 'input', id);
        },

        logout: function () {
            console.log('logout');
            logoutService.logout();
        }
    };
}]);

})(angular);