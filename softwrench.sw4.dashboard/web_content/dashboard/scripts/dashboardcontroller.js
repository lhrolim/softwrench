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
            $rootScope.$broadcast('dash_finishloading');
        }
    };
});

app.controller('DashboardController', [
    '$scope', '$log', '$timeout', 'modalService', 'fieldService', 'dashboardAuxService', 'contextService', 'alertService', 'crudContextHolderService',
    function ($scope, $log, $timeout, modalService, fieldService, dashboardAuxService, contextService, alertService, crudContextHolderService) {

        var selectedDashboardIds = [];
        function dashboardSelectedAtLeastOnce(dashboardId) {
            return !!selectedDashboardIds.find(function(id) {
                return dashboardId === id;
            });
        }
        function markDashboardSelected(dashboardId) {
            if (!dashboardSelectedAtLeastOnce(dashboardId)) {
                selectedDashboardIds.push(dashboardId);
            }
        }

        $scope.dashboardSelectedAtLeastOnce = dashboardSelectedAtLeastOnce;

        $scope.$watch("currentdashboardid", function(newValue, oldValue) {
            if (newValue === oldValue) return;
            markDashboardSelected(newValue);
        });

        $scope.doInit = function () {

            $scope.canCreateOwn = $scope.resultData.canCreateOwn;
            $scope.canCreateShared = $scope.resultData.canCreateShared;

            $scope.canCreateDashboards = $scope.canCreateShared || $scope.canCreateOwn;
            $scope.canCreateBoth = $scope.canCreateShared && $scope.canCreateOwn;

            $scope.dashboards = $scope.resultData.dashboards;
            $scope.newpanelschema = $scope.resultData.newPanelSchema;
            $scope.saveDashboardSchema = $scope.resultData.saveDashboardSchema;
            $scope.profiles = $scope.resultData.profiles;
            $scope.panelschemas = $scope.resultData.panelSchemas;
            $scope.applications = $scope.resultData.applications;
            $scope.currentdashboardid = $scope.resultData.preferredId;
            markDashboardSelected($scope.currentdashboardid);
            $scope.dashboard = $scope.getCurrentDashboardById($scope.currentdashboardid);
            var userData = contextService.getUserData();
            $scope.userid = userData.id;
        };

        $scope.createPanel = function () {
            //TODO make it customizable
            var schema = $scope.panelschemas['dashboardgrid'];
            $scope.creatingDashboard = false;

            modalService.show(schema, null, {
                title: "Create Panel", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    scope.associationOptions['applications'] = $scope.applications;
                    //                scope.$digest();
                }
            });
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
                title: "Add Panel", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    crudContextHolderService.updateEagerAssociationOptions("applications", $scope.applications);
//                    scope.$digest();
                }
            });
        };

        $scope.createNewPanel = function () {
            var schema = $scope.panelschemas[$scope.paneltype];

            modalService.show(schema, null, {
                title: "Create Panel", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    crudContextHolderService.updateEagerAssociationOptions("applications", $scope.applications);
                    //                scope.$digest();
                }
            });
        };

        $scope.getActiveClass = function (tabid) {
            return tabid == $scope.currentdashboardid ? "active" : null;
        };

        $scope.doInit();

        $scope.$on('dash_createpanel', function (event, paneltype) {
            var schema = $scope.panelschemas[paneltype];

            modalService.show(schema, null, {
                title: "Create Panel", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    //                    scope.associationOptions['applications'] = $scope.applications;
                    //                scope.$digest();
                }
            });
        });

        $scope.$on('dash_changeselecteddashboard', function (event, dashboardid) {
            $scope.currentdashboardid = dashboardid;
        });

        $scope.$on('dash_dashsaved', function (event, dashboard) {
            modalService.hide();

            // Lazy load of the dashboards - also set focus to the new dashboard
            var preexistingdashboardIdx = $scope.getCurrentIndexById(dashboard.id, true);

            if (preexistingdashboardIdx == -1) {
                $scope.dashboards.push(dashboard);
            } else {
                $scope.dashboards[preexistingdashboardIdx] = (dashboard);
            }

            $scope.dashboard = dashboard;
            $scope.currentdashboardid = dashboard.id;

            $scope.newDashboard = false;
            $scope.isEditingAnyDashboard = false;
        });

        $scope.$on('dash_panelcreated', function (event, dashboard) {
            modalService.hide();
        });

        $scope.$on('dash_changedashboard', function (event, dashboardid) {
            $scope.currentdashboardid = dashboardid;
        });

        $scope.$on('dash_finishloading', function (event, dashboardid) {
            $timeout(function () {
                $('.dashboarddetailtab li>a').each(function () {
                    var $this = $(this);
                    $this.click(function (e) {
                        e.preventDefault();
                        if ($scope.isEditingAnyDashboard) {
                            alertService.alert('Please, finish editing the current dashboard.');
                            e.stopImmediatePropagation();
                            return false;
                        }
                        $this.tab('show');
                        var dashid = $(this).data('tabid');

                        $scope.currentdashboardid = dashid;
                        $scope.dashboard = $scope.getCurrentDashboardById(dashid);

                        var log = $log.getInstance('dashboardrendered');
                        log.trace('lazy loading dashboard {0}'.format(dashid));
                    });
                });
            }, 0, false);
        });

        $scope.$on('dash_panelassociated', function (event, panel) {
            modalService.hide();
            dashboardAuxService.addPanelToDashboard($scope.dashboard, panel);
        });

        $scope.getEditButtonClass = function () {
            var buttonClass = 'btn-default';

            //TODO: if dashboard has changes
            //if (haschanges) {
            //    buttonClass = 'btn-primary';
            //}

            return buttonClass;
        };

        //**************************************************************************************creation***********************************************************
        $scope.viewDashboard = function (event, id) {
            if ($scope.isEditingAnyDashboard) {
                return;
            }
            $scope.newDashboard = false;

            $scope.currentdashboardid = id;
            $scope.dashboard = $scope.getCurrentDashboardById(id);

            $scope.$broadcast("sw:dashboard:selected", id);
            var log = $log.getInstance('dashboardrendered');
            log.trace('lazy loading dashboard {0}'.format(id));
        };

        $scope.createNewDashboard = function () {
            if ($scope.isEditingAnyDashboard) {
                return;
            }

            // Update display to show a new dashboard
            $scope.newDashboard = true;
            $scope.dashboard = {};

            var schema = $scope.saveDashboardSchema;

            modalService.show(schema, null, {
                title: "New Dashboard",
                cssclass: "dashboardmodal",
                //                onloadfn: function (scope) {
                //                    scope.associationOptions['applications'] = ;
                //                }
            });
        };

        $scope.isNewDashboard = function () {
            return $scope.newDashboard;
        };

        $scope.finishCreatingDashboard = function () {
            var log = $log.getInstance("dashboardController#saveDashboard");

            if (!$scope.canCreateBoth) {
                //this is personal only
                log.debug('saving personal dashboard');
                dashboardAuxService.saveDashboard($scope.dashboard);
                return;
            }
            var datamap = $scope.dashboard;
            datamap.canCreateBoth = $scope.canCreateBoth;

            modalService.show($scope.saveDashboardSchema, datamap, {
                title: "Save Dashboard", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    scope.associationOptions['profiles'] = $scope.profiles;
                }
            });
        };

        //**************************************************************************************edition***********************************************************

        $scope.isEditing = function (dashboardid) {
            return $scope.currentdashboardid == dashboardid && $scope.isEditingAnyDashboard;
        };

        $scope.cancelEditing = function () {
            $scope.isEditingAnyDashboard = false;
        };

        $scope.editDashboard = function (dashboardId) {
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