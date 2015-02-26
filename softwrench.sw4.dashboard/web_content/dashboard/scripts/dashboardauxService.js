var app = angular.module('sw_layout');

app.factory('dashboardAuxService', function ($rootScope, contextService, restService) {

    return {
        lookupFields: function (event) {
            var application = event.fields.application;
            if (application == null) {
                return;
            }
            restService.invokeGet('Dashboard', 'LoadFields', { applicationName: application }, function(data) {
                event.scope.associationOptions['appfields'] = data.resultObject;

            });
        },

        loadPanels: function (event) {
            var paneltype = event.fields.paneltype;
            if (paneltype == null) {
                return;
            }
            restService.invokeGet('Dashboard', 'LoadPanels', { paneltype: paneltype }, function (data) {
                event.scope.associationOptions['availablepanels'] = data.resultObject;

            });
        },


    }

});