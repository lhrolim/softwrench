(function (angular) {
    "use strict";

    function dashboardAuxService($rootScope, $timeout, $q, $log, validationService, contextService, restService, graphicPanelServiceProvider, crudContextHolderService) {
        //#region Utils
        function panelCreated(panel) {
            return function (response) {
                const data = response.data;
                const resultPanel = data.resultObject;
                resultPanel["#edit"] = panel["#edit"];
                $rootScope.$broadcast(DashboardEventConstants.PanelAssociated, resultPanel);
            };
        }

        //#endregion

        //#region Public methods
        function lookupFields(event) {
            const datamap = event.fields;

            const application = datamap.application;
            if (!application) {
                return $q.when();
            }

            return restService.getPromise("Dashboard", "LoadFields", { applicationName: application }).then(function (response) {
                const data = response.data;
                crudContextHolderService.updateEagerAssociationOptions("appFields", data.resultObject);
                datamap["appFields"] = "";
                $.each(data.resultObject, function (key, value) {
                    datamap["appFields"] += value.value + ",";
                });
                const selectedFields = datamap["appFields"];
                if (selectedFields) {
                    datamap["appFields"] = selectedFields.substring(0, selectedFields.length - 1);
                }
                crudContextHolderService.rootDataMap("#modal")["appFields"] = datamap["appFields"];
                $rootScope.$broadcast(DashboardEventConstants.AppFieldsLoaded);

                //data.resultObject.unshift({value:"#allfields",label:"All Fields"});
            });
        }


        function lookupWhereClause(event) {
            const datamap = event.fields;

            const application = datamap.application;
            const alias = datamap.alias;
            if (!application || !alias) {
                return $q.when();
            }

            return restService.getPromise("Dashboard", "LoadPanelWhereClause", { applicationName: application, panelAlias: alias }).then(function (response) {
                const data = response.data;
                datamap["whereClause"] = (data === null || data === "null") ? null : data;
            });
        }

        function savePanel(datamap) {
            const local = datamap;
            local.size = parseInt(local.size);
            if (datamap["multiSort"]) {
                local.defaultSortField = datamap["multiSort"].map(i => {
                    return i.columnName + (i.isAscending ? " asc" : " desc");
                }).join(',');
            }
            const schema = crudContextHolderService.currentSchema("#modal");
            validationService.validatePromise(schema, datamap)
            .then(() => {
                return restService.postPromise("Dashboard", "SaveGridPanel", null, local);
            })
            .then((response) => {
                return panelCreated(local)(response);
            });
        }

        function saveDashboard(datamap, policy) {
            const localDatamap = datamap;
            localDatamap.creationDateSt = localDatamap.creationDate;
            if (!localDatamap.panels) {
                //this will avoid wrong serialization
                delete datamap.panels;
            } else {
                datamap.panels = datamap.panels.map(panRel => {
                    return { id: panRel.id, position: panRel.position, panelId: panRel.panel.id, panelType: panRel.panel.type };
                });
            }

            if (localDatamap.mode === "brandnew") {
                //to avoid inconsistencies if the user selects a value but then switches back to brand new
                localDatamap.id = null;
            } else if (localDatamap.mode) {
                localDatamap.id = localDatamap.dashboardid;
                localDatamap.cloning = true;
            }

            const schema = crudContextHolderService.currentSchema("#modal");
            validationService.validatePromise(schema, datamap)
                .then(() => {
                    return restService.postPromise("Dashboard", "SaveDashboard", null, localDatamap).then(function (response) {
                        const data = response.data;
                        $rootScope.$broadcast(DashboardEventConstants.DashboardSaved, data.resultObject);
                    });
                });
        }


        function filterSelectableModeOptions(item) {
            const options = crudContextHolderService.fetchEagerAssociationOptions("existingDashboards", { schemaId: "#modal" });
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
            const dm = crudContextHolderService.rootDataMap("#modal");
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
            const local = datamap;
            restService.getPromise("Dashboard", "LoadPanel", { panel: local.panel }).then(function (response) {
                const data = response.data;
                $rootScope.$broadcast(DashboardEventConstants.PanelAssociated, data.resultObject);
            });
        }

        function loadPanels(event) {
            const paneltype = event.fields.paneltype;
            if (!paneltype) return;

            restService.getPromise("Dashboard", "LoadPanels", { paneltype: paneltype }).then(function (response) {
                const data = response.data;
                crudContextHolderService.updateEagerAssociationOptions("availablepanels", data.resultObject);
            });
        }

        function addPanelToDashboard(dashboard, panel) {
            const hasPanels = angular.isArray(dashboard.panels);
            const position = hasPanels ? dashboard.panels.length : 0;
            const panelAssociation = { position: position, panel: panel };
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
            const provider = event.fields.provider;
            if (!provider) return;
            const instance = graphicPanelServiceProvider.getService(provider);
            return instance.onProviderSelected(event);
        }

        function createAndAssociateGraphicPanel(datamap) {
            datamap.size = parseInt(datamap.size);
            const instance = graphicPanelServiceProvider.getService(datamap.provider);
            instance.onBeforeAssociatePanel(datamap);
            restService.postPromise("Dashboard", "SaveGraphicPanel", null, datamap).then(panelCreated(datamap));
        }

        function deactivateDashboard(dashboard) {
            return restService.postPromise("Dashboard", "DeactivateDashboard", null, dashboard);
        }

        function loadDashboards(currentDashboardId) {
            const params = !!currentDashboardId ? { dashBoardId: currentDashboardId } : null;
            return restService.getPromise("Dashboard", "LoadDashboard", params)
                .then(function (response) {
                    return response.data;
                });
        }

        function getPanelModalLabel(datamap, schema) {
            if (datamap.id) {
                return "Save Grid Widget";
            }
            return "Add Grid Widget";
        }

        //#endregion

        //#region Service Instance
        const service = {
            addPanelToDashboard,
            createAndAssociateGraphicPanel,
            deactivateDashboard,
            filterSelectableModeOptions,
            filterSelectableDashboards,
            getPanelModalLabel,
            loadDashboards,
            loadPanels,
            lookupFields,
            lookupWhereClause,
            saveDashboard,
            savePanel,
            selectPanel,
            setGraphicProvider,

        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").service("dashboardAuxService",
        ["$rootScope", "$timeout", "$q", "$log", "validationService", "contextService", "restService", "graphicPanelServiceProvider", "crudContextHolderService", dashboardAuxService]);
    //#endregion

})(angular);