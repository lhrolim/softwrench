(function (angular) {
    "use strict";

    function dashboardAuxService($rootScope, $log, contextService, restService, graphicPanelServiceProvider,crudContextHolderService) {
        //#region Utils
        function panelCreated(datamap) {
            return function(response) {
                var data = response.data;
                $rootScope.$broadcast('dash_panelassociated', data.resultObject);
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

        function createAndAssociateGridPanel(datamap) {
            var local = datamap.fields || datamap;
            restService.postPromise('Dashboard', 'CreateGridPanel', null, local).then(panelCreated(local));
        }

        function saveDashboard(datamap, policy) {
            var localDatamap = datamap.fields || datamap;
            localDatamap.creationDateSt = localDatamap.creationDate;
            if (!!localDatamap.panels) {
                //this will avoid wrong serialization
                delete datamap.panels;
            }

            restService.postPromise('Dashboard', 'SaveDashboard', null, localDatamap).then(function (response) {
                var data = response.data;
                $rootScope.$broadcast('dash_dashsaved', data.resultObject);
            });
        }

        function selectPanel(datamap) {
            var local = datamap.fields || datamap;
            restService.getPromise("Dashboard", "LoadPanel", { panel: local.panel }).then(function (response) {
                var data = response.data;
                $rootScope.$broadcast('dash_panelassociated', data.resultObject);
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

        function setGraphicProvider(event) {
            // delegate call to provider api
            var instance = graphicPanelServiceProvider.getService(event.fields.provider);
            return instance.onProviderSelected(event);
        }

        function createAndAssociateGraphicPanel(paramDatamap) {
            var datamap = paramDatamap.fields || paramDatamap;
            var instance = graphicPanelServiceProvider.getService(datamap.provider);
            instance.onBeforeAssociatePanel(datamap);
            restService.postPromise("Dashboard", "CreateGraphicPanel", null, datamap).then(panelCreated(datamap));
        }
        //#endregion

        //#region Service Instance
        var service = {
            lookupFields: lookupFields,
            createAndAssociateGridPanel: createAndAssociateGridPanel,
            saveDashboard: saveDashboard,
            selectPanel: selectPanel,
            loadPanels: loadPanels,
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