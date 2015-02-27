var app = angular.module('sw_layout');

app.factory('dashboardAuxService', function ($rootScope,$log, contextService, restService, alertService, modalService) {

    return {
        lookupFields: function (event) {
            var application = event.fields.application;
            if (application == null) {
                return;
            }
            restService.invokeGet('Dashboard', 'LoadFields', { applicationName: application }, function (data) {
                event.scope.associationOptions['appfields'] = data.resultObject;

            });
        },

        createAndAssociatePanel: function (datamap) {

            restService.invokePost('Dashboard', 'CreatePanel', datamap, function (data) {
                modalService.hide();
                $rootScope.dispatch('dash_panelassociated', data.resultObject);
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

        readjustLayout : function(dashboard, row, column) {
            var log = $log.getInstance("dashboardauxService#adjustLayout");
            if (!dashboard.layout) {
                //first will be whole line
                dashboard.layout = "1";
                log.debug("adding first line to layout");
                return dashboard.layout;
            }
            var rows = dashboard.layout.split(',');
            if (row > rows.length) {
                dashboard.layout += ',' + column;
                log.debug("adding new line at column 1");
                if (column != 1) {
                    log.error("trying to add a new line on a column different than 1");
                    throw new Error("unexpected behaviour");
                }
                return dashboard.layout;
            }
            var rowLayout = parseInt(rows[row - 1]);
            rows[row - 1] = "" + ++rowLayout;
            dashboard.layout = rows.join(',');
            return dashboard.layout;
        },

        readjustPositions: function (dashboard, panel, row, column) {

            var rows = dashboard.layout.split(',');
            var newPosition = 0;
            for (var i = 0; i < row-1; i++) {
                newPosition += parseInt(rows[i]);
            }
            newPosition += column -1;


//            var newPosition = (row * column) - 1;

            var panelAssociation = {
                position: newPosition,
                panel: panel

            };
            dashboard.panels = dashboard.panels || [];

            dashboard.panels.splice(newPosition, 0, panelAssociation);

            var panelRel = dashboard.panels;
            for (i = 0; i < panelRel.length; i++) {
                panelRel[i].position = i;
            }
            return dashboard;
        }


    }

});