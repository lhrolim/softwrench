(function (angular) {
    "use strict";

    var app = angular.module('sw_layout');
    app.directive('dashboardrendered', function ($timeout, $log, $rootScope, eventService) {
        "ngInject";
        return {
            //TODO: extract directive
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.$last === false) {
                    return;
                }
                var log = $log.getInstance('dashboardrendered');
                log.debug("finished rendering dashboards");

                $rootScope.$broadcast('sw_titlechanged', 'Dashboard');
                $rootScope.$broadcast('dash_finishloading');
            }
        };
    });

    app.controller("DashboardController", [
        "$scope", "$log", "$timeout", "modalService", "fieldService", "dashboardAuxService", "contextService", "alertService", "crudContextHolderService", "redirectService",
    function ($scope, $log, $timeout, modalService, fieldService, dashboardAuxService, contextService, alertService, crudContextHolderService, redirectService) {

        // #region Utils (state control)
        var selectedDashboardIds = [];
        function dashboardSelectedAtLeastOnce(dashboardId) {
            return !!selectedDashboardIds.find(function (id) {
                return dashboardId === id;
            });
        }
        function markDashboardSelected(dashboardId) {
            if (!dashboardSelectedAtLeastOnce(dashboardId)) {
                selectedDashboardIds.push(dashboardId);
            }
        }
        function markDashboardNotSelected(dashboardId) {
            selectedDashboardIds = selectedDashboardIds.filter(function (id) {
                return id !== dashboardId;
            });
        }

        var dashboardCheckpoint = {};
        function createDashboardCheckPoint(dashboard) {
            var checkpoint = angular.copy(dashboard);
            dashboardCheckpoint[dashboard.id] = checkpoint;
        }
        function restoreDashboardCheckpoint(dashboardId) {
            $scope.currentdashboardid = dashboardId;
            var checkpoint = angular.copy(dashboardCheckpoint[dashboardId]);

            // for some reason, simple assgnment doesn't work -> has to be done property by property
            // $scope.dashboard = checkpoint;
            angular.forEach(checkpoint, function(val, key) {
                $scope.dashboard[key] = val;
            });
        }
        function isDashboardChanged(dashboard) {
            var checkpoint = dashboardCheckpoint[dashboard.id];
            return !angular.equals(dashboard, checkpoint);
        }
        function deleteDashboardCheckpoint(dashboardId) {
            delete dashboardCheckpoint[dashboardId];
        }
        function positionComparator(panelA, panelB) {
            return panelA.position - panelB.position;
        }
        function sortPanels(dashboard) {
            dashboard.panels = dashboard.panels.sort(positionComparator);
        }

        //#endregion

        $scope.dashboardSelectedAtLeastOnce = dashboardSelectedAtLeastOnce;

        $scope.$watch("currentdashboardid", function (newValue, oldValue) {
            if (newValue === oldValue) return;
            markDashboardSelected(newValue);
        });

        $scope.doInit = function () {
            // permissions
            $scope.canCreateOwn = $scope.resultData.permissions.canCreateOwn;
            $scope.canCreateShared = $scope.resultData.permissions.canCreateShared;
            $scope.canCreateDashboards = $scope.canCreateShared || $scope.canCreateOwn;
            $scope.canCreateBoth = $scope.canCreateShared && $scope.canCreateOwn;

            $scope.canDeleteOwn = $scope.resultData.permissions.canDeleteOwn;
            $scope.canDeleteShared = $scope.resultData.permissions.canDeleteShared;

            // schemas
            $scope.newpanelschema = $scope.resultData.schemas.newPanelSchema;
            $scope.saveDashboardSchema = $scope.resultData.schemas.saveDashboardSchema;
            $scope.panelschemas = $scope.resultData.schemas.panelSchemas;

            // data
            $scope.dashboards = $scope.resultData.dashboards;
            $scope.profiles = $scope.resultData.profiles;
            $scope.applications = $scope.resultData.applications;
            $scope.currentdashboardid = $scope.resultData.preferredId;

            markDashboardSelected($scope.currentdashboardid);
            $scope.dashboard = $scope.getCurrentDashboardById($scope.currentdashboardid);
            $scope.dashboards.forEach(sortPanels);
            $scope.dashboards.forEach(createDashboardCheckPoint);
            var userData = contextService.getUserData();
            $scope.userid = userData.id;
        };

        $scope.getCurrentDashboardById = function (id) {
            var dashboards = $scope.dashboards;
            for (var i = 0; i < dashboards.length; i++) {
                if (dashboards[i].id == id) {
                    return dashboards[i];
                }
            }

            return dashboards[0];
        };

        $scope.getCurrentIndexById = function (id) {
            var dashboards = $scope.dashboards;
            for (var i = 0; i < dashboards.length; i++) {
                if (dashboards[i].id == id) {
                    return i;
                }
            }

            return -1;
        };

        $scope.cancelDashboard = function () {
            $scope.creatingDashboard = false;
        };

        $scope.addpanel = function () {
            var dm = {};
            $scope.dashboard.panels = $scope.dashboard.panels || [];
            dm.numberofpanels = $scope.dashboard.panels.length;
            modalService.show($scope.newpanelschema, dm, {
                title: "Add Widget", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    crudContextHolderService.updateEagerAssociationOptions("applications", $scope.applications);
                    // scope.$digest();
                }
            });
        };

        function panelTypeLabel(paneltype) {
            if (!paneltype) return "";
            if (paneltype.contains("graphic")) return "Graphic";
            else if (paneltype.contains("grid")) return "Grid";
            else return "";
        }

        $scope.createNewPanel = function () {
            var schema = $scope.panelschemas[$scope.paneltype];
            var title = "Create " + panelTypeLabel($scope.paneltype) + " Widget";

            modalService.show(schema, null, {
                title: title, cssclass: "dashboardmodal", onloadfn: function (scope) {
                    crudContextHolderService.updateEagerAssociationOptions("applications", $scope.applications);
                    //  scope.$digest();
                }
            });
        };

        $scope.getActiveClass = function (tabid) {
            return tabid == $scope.currentdashboardid ? "active" : null;
        };

        //#region refresh
        $scope.refreshInterval = {
            // hardcoded with 'expected' values > TODO: change (fetch from somewhere) when requirements are clearer
            config: {
                'min'    : 5,
                'max'    : 60,
                'default': 5,
                'unit'   : "minute"
            }
        };

        $scope.refreshDashboards = function (delay) {
            dashboardAuxService.loadDashboards($scope.currentdashboardid).then(function(data) {
                $scope.dashboards.forEach(function(dashboard) {
                    markDashboardNotSelected(dashboard.id);
                });
                $scope.resultData = data.resultObject;
                $scope.doInit();
            });
        };

        //#endregion

        $scope.doInit();

        $scope.$on("dash_createpanel", function (event, paneltype) {
            var schema = $scope.panelschemas[paneltype];
            var title = "Create " + panelTypeLabel(paneltype) + " Widget";

            modalService.show(schema, null, {
                title: title, cssclass: "dashboardmodal", onloadfn: function (scope) {
                    // scope.associationOptions['applications'] = $scope.applications;
                    // scope.$digest();
                }
            });
        });

        $scope.$on("dash_changeselecteddashboard", function (event, dashboardid) {
            $scope.currentdashboardid = dashboardid;
        });

        $scope.$on("dash_dashsaved", function (event, dashboard) {
            modalService.hide();

            // Lazy load of the dashboards - also set focus to the new dashboard
            var preexistingdashboardIdx = $scope.getCurrentIndexById(dashboard.id, true);

            if (preexistingdashboardIdx === -1) {
                $scope.dashboards.push(dashboard);
            } else {
                $scope.dashboards[preexistingdashboardIdx] = (dashboard);
            }

            $scope.dashboard = dashboard;
            $scope.currentdashboardid = dashboard.id;
            createDashboardCheckPoint($scope.dashboard);

            $scope.newDashboard = false;
            $scope.isEditingAnyDashboard = false;
        });

        $scope.$on("dash_panelcreated", function (event, dashboard) {
            modalService.hide();
        });

        $scope.$on("dash_changedashboard", function (event, dashboardid) {
            $scope.currentdashboardid = dashboardid;
        });

        $scope.$on("dash_finishloading", function (event, dashboardid) {
            $timeout(function () {
                $(".dashboarddetailtab li>a").each(function () {
                    var $this = $(this);
                    $this.click(function (e) {
                        e.preventDefault();
                        if ($scope.isEditingAnyDashboard) {
                            alertService.alert("Please, finish editing the current dashboard.");
                            e.stopImmediatePropagation();
                            return false;
                        }
                        $this.tab("show");
                        var dashid = $(this).data("tabid");

                        $scope.currentdashboardid = dashid;
                        $scope.dashboard = $scope.getCurrentDashboardById(dashid);

                        var log = $log.getInstance("dashboardrendered");
                        log.trace("lazy loading dashboard {0}".format(dashid));
                    });
                });
            }, 0, false);
        });

        $scope.$on("dash_panelassociated", function (event, panel) {
            modalService.hide();
            dashboardAuxService.addPanelToDashboard($scope.dashboard, panel);
        });

        $scope.getEditButtonClass = function () {
            var buttonClass = "btn-default";

            //TODO: if dashboard has changes
            //if (haschanges) {
            //    buttonClass = 'btn-primary';
            //}

            return buttonClass;
        };

        //#region creation

        $scope.viewDashboard = function (event, id) {
            if ($scope.isEditingAnyDashboard) {
                return;
            }
            $scope.newDashboard = false;

            $scope.currentdashboardid = id;
            $scope.dashboard = $scope.getCurrentDashboardById(id);

            $scope.$broadcast("sw:dashboard:selected", id);
            var log = $log.getInstance("dashboardrendered", ["dashboard"]);
            log.trace("lazy loading dashboard {0}".format(id));
        };

        $scope.createNewDashboard = function () {
            if ($scope.isEditingAnyDashboard) {
                return;
            }

            // Update display to show a new dashboard
            $scope.newDashboard = true;
            $scope.dashboard = {};

            var schema = $scope.saveDashboardSchema;

            return redirectService.openAsModal(schema.applicationName, schema.schemaId, {
                title: "New Dashboard",
                cssclass: "dashboardmodal",
            });


//            modalService.show(schema, null, {
//                
//                // onloadfn: function (scope) {
//                //  scope.associationOptions['applications'] = ;
//                // }
//            });
        };

        $scope.isNewDashboard = function () {
            return $scope.newDashboard;
        };

        $scope.finishCreatingDashboard = function () {
            var log = $log.getInstance("dashboardController#saveDashboard");

            if (!$scope.canCreateBoth) {
                //this is personal only
                log.debug("saving personal dashboard");
                dashboardAuxService.saveDashboard($scope.dashboard);
                return;
            }
            var datamap = $scope.dashboard;
            datamap.canCreateBoth = $scope.canCreateBoth;

            modalService.show($scope.saveDashboardSchema, datamap, {
                title: "Save Dashboard", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    scope.associationOptions["profiles"] = $scope.profiles;
                }
            });
        };

        //#endregion

        //#region edition

        $scope.isEditing = function (dashboardid) {
            return $scope.currentdashboardid === dashboardid && $scope.isEditingAnyDashboard;
        };

        $scope.cancelEditing = function (dashboard) {
            if (!isDashboardChanged(dashboard)) {
                $scope.isEditingAnyDashboard = false;
                return;
            }
            alertService.confirm2(
                    "There are unsaved changes to the current dashboard. " +
                    "Any unsaved changes will be discarded. " +
                    "Are you sure sure you want to cancel editting.")
                .then(function() {
                    restoreDashboardCheckpoint(dashboard.id);
                    $scope.isEditingAnyDashboard = false;
                });
        };

        $scope.editDashboard = function (dashboard) {
            $scope.isEditingAnyDashboard = true;
        };

        $scope.finishEditingDashboard = function (dashboardId) {
            $scope.dashboard.policy = "personal";
            dashboardAuxService.saveDashboard($scope.dashboard);
        };

        $scope.canEditDashboard = function (dashboard) {
            if ($scope.canCreateBoth) {
                return true;
            }
            return dashboard.createdby == $scope.userid;
        };

        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue && $scope.resultObject.crudSubTemplate != null &&
                $scope.resultObject.crudSubTemplate.indexOf("/Shared/dashboard/templates/Dashboard.html") != -1) {
                $scope.doInit();
            }
        });

        function panelDatamap(panel) {
            var datamap = angular.copy(panel);
            if (!panel.type.contains("graphic") && !panel.type.contains("Graphic")) {
                return datamap;
            }
            angular.forEach(panel.configurationDictionary, function (value, key) {
                datamap[key] = value;
            });
            datamap["#edit"] = true;

            return datamap;
        }

        function panelSimpleType(panel) {
            return panel.type.toLowerCase().replace("panel", "");
        }

        $scope.editPanel = function (panel, dashboard) {
            var schema = $scope.panelschemas[panelSimpleType(panel)];
            var title = "Edit " + panelTypeLabel($scope.paneltype) + " Widget";
            var datamap = panelDatamap(panel);

            modalService.show(schema, datamap, {
                title: title, cssclass: "dashboardmodal", onloadfn: function (scope) {
                    crudContextHolderService.updateEagerAssociationOptions("applications", $scope.applications);
                }
            });
        };

        //#endregion

        //#region delete

        $scope.canDeleteDashboard = function (dashboard) {
            return $scope.canDeleteShared || (dashboard.createdBy === contextService.getUserData().userId && $scope.canDeleteOwn);
        };

        $scope.deleteDashboard = function (dashboard) {
            alertService
                .confirm2("Are you sure you want to remove the dashboard '" + dashboard.title + "' ?")
                .then(function () {
                    return dashboardAuxService.deactivateDashboard(dashboard);
                })
                .then(function () {
                    $scope.dashboards = $scope.dashboards.filter(function (d) {
                        return d.id !== dashboard.id;
                    });
                    markDashboardNotSelected(dashboard.id);
                    deleteDashboardCheckpoint(dashboard.id);
                    if ($scope.currentdashboardid === dashboard.id) {
                        $scope.viewDashboard(null, $scope.dashboards.length > 0 ? $scope.dashboards[0].id : null);
                    }
                });
        };

        $scope.removePanel = function (panelDataSource, dashboard) {
            return alertService.confirm2("Are you sure you want to remove the widget '" + panelDataSource.panel.title + "' from this dashboard")
                .then(function () {
                    // remove panel's association
                    dashboard.panels = dashboard.panels.filter(function (p) {
                        return p.position !== panelDataSource.position;
                    });
                    // update positions
                    dashboard.panels.forEach(function (p) {
                        if (p.position > panelDataSource.position) {
                            p.position--;
                        }
                    });
                    // save dashboard
                    return dashboardAuxService.saveDashboard(dashboard);
                });
        };

        //#endregion
    }
    ]);

    app.controller('DashboardController2', [
        '$scope', '$rootScope',
        function ($scope, $rootScope) {
            $scope.createNewPanel = function () {
                //this is a inner controller for the button section
                $rootScope.$broadcast("dash_createpanel", $scope.datamap.paneltype);
            }
        }
    ]);

})(angular);