(function (angular) {
    "use strict";

    function dashboardAuxService($rootScope, $log, contextService, restService, graphicPanelServiceProvider,crudContextHolderService) {
        //#region Utils
        function panelCreated(datamap) {
            return function(response) {
                var data = response.data;
                if (datamap.row && datamap.column) {
                    $rootScope.$broadcast('dash_panelassociated', data.resultObject, datamap.row, datamap.column);
                } else {
                    $rootScope.$broadcast('dash_panelcreated', data.resultObject);
                }
            };
        }
        //#endregion

        //#region Public methods
        function lookupFields(event) {
            var application = event.fields.application;
            if (application == null) {
                return;
            }
            restService.getPromise('Dashboard', 'LoadFields', { applicationName: application }).then(function (response) {
                var data = response.data;
                crudContextHolderService.updateEagerAssociationOptions("appfields", data.resultObject);
                event.scope.datamap['appfields'] = "";
                $.each(data.resultObject, function (key, value) {
                    event.scope.datamap['appfields'] += value.value + ",";
                });
                var selectedFields = event.scope.datamap['appfields'];
                if (selectedFields) {
                    event.scope.datamap['appfields'] = selectedFields.substring(0, selectedFields.length - 1);
                }

                //data.resultObject.unshift({value:"#allfields",label:"All Fields"});
                //event.scope.datamap['appfields'] = "#allfields";
            });
        }

        function locatePanelFromMatrix(dashboard, row, column) {
            var rows = dashboard.layout.split(',');
            var newPosition = 0;

            // Convert 2D coordinate to 1D array. 
            for (var i = 0; i < row; i++) {
                newPosition += parseInt(rows[i]);
            }
            newPosition += column;

            return dashboard.panels[newPosition];
        }

        function createAndAssociateGridPanel(datamap) {
            restService.postPromise('Dashboard', 'CreateGridPanel', null, datamap).then(panelCreated(datamap));
        }

        function saveDashboard(datamap, policy) {
            datamap.creationDateSt = datamap.creationDate;
            if (datamap.panels == null) {
                //this will avoid wrong serialization
                delete datamap.panels;
            }

            restService.postPromise('Dashboard', 'SaveDashboard', null, datamap).then(function (response) {
                var data = response.data;
                $rootScope.$broadcast('dash_dashsaved', data.resultObject);
            });
        }

        function selectPanel(datamap) {
            restService.getPromise("Dashboard", "LoadPanel", { panel: datamap.panel }).then(function (response) {
                var data = response.data;
                $rootScope.$broadcast('dash_panelassociated', data.resultObject, datamap.row, datamap.column);
            });
        }

        function loadPanels(event) {
            var paneltype = event.fields.paneltype;
            if (!paneltype) return;

            restService.getPromise('Dashboard', 'LoadPanels', { paneltype: paneltype }).then(function (response) {
                var data = response.data;
                crudContextHolderService.updateEagerAssociationOptions("availablepanels", data.resultObject);
            });
        }

        function readjustLayout(dashboard, row, column) {
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
                if (column !== 1) {
                    log.error("trying to add a new line on a column different than 1");
                    throw new Error("unexpected behaviour");
                }
                return dashboard.layout;
            }
            var rowLayout = parseInt(rows[row - 1]);
            rows[row - 1] = "" + ++rowLayout;
            dashboard.layout = rows.join(',');
            return dashboard.layout;
        }

        function readjustPositions(dashboard, panel, row, column) {
            var rows = dashboard.layout.split(',');
            var newPosition = 0;
            for (var i = 0; i < row - 1; i++) {
                newPosition += parseInt(rows[i]);
            }
            newPosition += column - 1;

            //var newPosition = (row * column) - 1;

            var panelAssociation = {
                position: newPosition,
                panel: panel

            };
            dashboard.panels = dashboard.panels || [];

            dashboard.panels.splice(newPosition, 0, panelAssociation);

            angular.forEach(dashboard.panels, function (p, index) {
                p.position = index;
            });
            return dashboard;
        }

        function setGraphicProvider(event) {
            // delegate call to provider api
            var instance = graphicPanelServiceProvider.getService(event.fields.provider);
            return instance.onProviderSelected(event);
        }

        function createAndAssociateGraphicPanel(datamap) {
            var instance = graphicPanelServiceProvider.getService(datamap.provider);
            instance.onBeforeAssociatePanel(datamap);
            restService.postPromise("Dashboard", "CreateGraphicPanel", null, datamap).then(panelCreated(datamap));
        }
        //#endregion

        //#region Service Instance
        var service = {
            lookupFields: lookupFields,
            locatePanelFromMatrix: locatePanelFromMatrix,
            createAndAssociateGridPanel: createAndAssociateGridPanel,
            saveDashboard: saveDashboard,
            selectPanel: selectPanel,
            loadPanels: loadPanels,
            readjustLayout: readjustLayout,
            readjustPositions: readjustPositions,
            setGraphicProvider: setGraphicProvider,
            createAndAssociateGraphicPanel: createAndAssociateGraphicPanel
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").factory("dashboardAuxService", ["$rootScope", "$log", "contextService", "restService", "graphicPanelServiceProvider", "crudContextHolderService", dashboardAuxService]);
    //#endregion

})(angular);