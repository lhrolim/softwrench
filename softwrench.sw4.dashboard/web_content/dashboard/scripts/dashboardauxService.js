(function (angular) {
    "use strict";

    function dashboardAuxService($rootScope, $log, contextService, restService, graphicPanelServiceProvider, crudContextHolderService) {
        //#region Utils
        function panelCreated(panel) {
            return function (response) {
                var data = response.data;
                var resultPanel = data.resultObject;
                resultPanel["#edit"] = panel["#edit"];
                $rootScope.$broadcast("dash_panelassociated", resultPanel);
            };
        }
        //#endregion

        //#region Public methods
        function lookupFields(event) {
            var application = event.fields.application;
            if (!application) return;

            restService.getPromise("Dashboard", "LoadFields", { applicationName: application }).then(function (response) {
                var data = response.data;
                crudContextHolderService.updateEagerAssociationOptions("appfields", data.resultObject);
                event.scope.datamap["appfields"] = "";
                $.each(data.resultObject, function (key, value) {
                    event.scope.datamap["appfields"] += value.value + ",";
                });
                var selectedFields = event.scope.datamap["appfields"];
                if (selectedFields) {
                    event.scope.datamap["appfields"] = selectedFields.substring(0, selectedFields.length - 1);
                }

                //data.resultObject.unshift({value:"#allfields",label:"All Fields"});
                //event.scope.datamap['appfields'] = "#allfields";
            });
        }

        function createAndAssociateGridPanel(datamap) {
            var local = datamap.fields || datamap;
            local.size = parseInt(local.size);

            restService.postPromise("Dashboard", "SaveGridPanel", null, local).then(panelCreated(local));
        }

        function saveDashboard(datamap, policy) {
            var localDatamap = datamap.fields || datamap;
            localDatamap.creationDateSt = localDatamap.creationDate;
            if (!localDatamap.panels) {
                //this will avoid wrong serialization
                delete datamap.panels;
            }

            if (localDatamap.mode === "brandnew") {
                //to avoid inconsistencies if the user selects a value but then switches back to brand new
                localDatamap.id = null;
            } else if (localDatamap.mode) {
                localDatamap.id = localDatamap.dashboardid;
                localDatamap.cloning = true;
            }

            return restService.postPromise("Dashboard", "SaveDashboard", null, localDatamap).then(function (response) {
                var data = response.data;
                $rootScope.$broadcast("dash_dashsaved", data.resultObject);
            });
        }
        function filterSelectableModeOptions(item) {
            var options = crudContextHolderService.fetchEagerAssociationOptions("existingDashboards", { schemaId: "#modal" });
            if (!options || options.length === 0) {
                return false;
            }

            if (item.value === "brandnew") {
                return true;
            }

            return options.some(function (dashboard) {
                if (item.value === "restore") {
                    //at least one inactive needs to be restored
                    return !dashboard.extrafields.active;
                }
                else if (item.value === "clone") {
                    //at least one active needs to be cloned
                    return dashboard.extrafields.active;
                }
            });

        }

        function filterSelectableDashboards(item) {
            var dm = crudContextHolderService.rootDataMap("#modal");
            if (!dm) {
                return true;
            }
            if (dm.mode === "restore") {
                return item.extrafields.active === false;
            } else if (dm.mode === "clone") {
                return item.extrafields.active === true;
            }
        }

        function selectPanel(datamap) {
            var local = datamap.fields || datamap;
            restService.getPromise("Dashboard", "LoadPanel", { panel: local.panel }).then(function (response) {
                var data = response.data;
                $rootScope.$broadcast("dash_panelassociated", data.resultObject);
            });
        }

        function loadPanels(event) {
            var paneltype = event.fields.paneltype;
            if (!paneltype) return;

            restService.getPromise("Dashboard", "LoadPanels", { paneltype: paneltype }).then(function (response) {
                var data = response.data;
                crudContextHolderService.updateEagerAssociationOptions("availablepanels", data.resultObject);
            });
        }

        function addPanelToDashboard(dashboard, panel) {
            var hasPanels = angular.isArray(dashboard.panels);
            var position = hasPanels ? dashboard.panels.length : 0;

            var panelAssociation = { position: position, panel: panel }

            if (!hasPanels) {
                dashboard.panels = [panelAssociation];
                return dashboard;
            }

            if (!panel["#edit"]) { // create: add panel relation
                dashboard.panels.push(panelAssociation);
            } else { // edit: update panels
                dashboard.panels.forEach(function (p) {
                    if (p.panel.id !== panel.id) return;
                    p.panel = panel;
                });
                delete panel["#edit"];
            }

            return dashboard;
        }

        function setGraphicProvider(event) {
            // delegate call to provider api
            var instance = graphicPanelServiceProvider.getService(event.fields.provider);
            return instance.onProviderSelected(event);
        }

        function createAndAssociateGraphicPanel(paramDatamap) {
            var datamap = paramDatamap.fields || paramDatamap;
            datamap.size = parseInt(datamap.size);

            var instance = graphicPanelServiceProvider.getService(datamap.provider);
            instance.onBeforeAssociatePanel(datamap);
            restService.postPromise("Dashboard", "SaveGraphicPanel", null, datamap).then(panelCreated(datamap));
        }

        function deactivateDashboard(dashboard) {
            return restService.postPromise("Dashboard", "DeactivateDashboard", null, dashboard);
        }
        //#endregion

        //#region Service Instance
        var service = {
            lookupFields: lookupFields,
            createAndAssociateGridPanel: createAndAssociateGridPanel,
            saveDashboard: saveDashboard,
            filterSelectableModeOptions: filterSelectableModeOptions,
            filterSelectableDashboards: filterSelectableDashboards,
            selectPanel: selectPanel,
            loadPanels: loadPanels,
            setGraphicProvider: setGraphicProvider,
            createAndAssociateGraphicPanel: createAndAssociateGraphicPanel,
            addPanelToDashboard: addPanelToDashboard,
            deactivateDashboard: deactivateDashboard
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").factory("dashboardAuxService",
        ["$rootScope", "$log", "contextService", "restService", "graphicPanelServiceProvider", "crudContextHolderService", dashboardAuxService]);
    //#endregion

})(angular);