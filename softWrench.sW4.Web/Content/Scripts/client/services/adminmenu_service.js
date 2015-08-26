﻿var app = angular.module('sw_layout');

app.factory('adminMenuService', function (menuService, redirectService, contextService, schemaCacheService) {
    return {
        doAction: function (title, controller, action, parameters, target) {
            menuService.setActiveLeaf(target);
            redirectService.redirectToAction(title, controller, action, parameters);
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

            sessionStorage.removeItem("swGlobalRedirectURL");
            if (contextService.isLocal()) {
                //clear local everytime to make development easier
                schemaCacheService.wipeSchemaCacheIfNeeded();
            }
            contextService.clearContext();
            sessionStorage['ctx_loggedin'] = false;
        }
    };
});
