var app = angular.module('sw_layout');

app.factory('dashboardAuxService', function ($rootScope, contextService, restService,alertService) {

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

        validatePanels: function (event) {
            var paneltype = event.fields.paneltype;
            if (paneltype == null) {
                return;
            }
            if (paneltype == "dashboardgraphic") {
                alertService.alert("Graphic panels are not yet supported");
                event.fields.paneltype = "";
                return;
            }
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