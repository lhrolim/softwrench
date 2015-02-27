var app = angular.module('sw_layout');

app.controller('DashboardController', [
    '$scope','$log', 'modalService', 'fieldService', 'dashboardAuxService',
    function ($scope,$log, modalService, fieldService, dashboardAuxService) {

        $scope.doInit = function () {

            $scope.canCreateOwn = $scope.resultData.canCreateOwn;
            $scope.canCreateShared = $scope.resultData.canCreateShared;

            $scope.canCreteDashboards = $scope.canCreateShared || $scope.canCreateOwn;
            $scope.canCreateBoth = $scope.canCreateShared && $scope.canCreateOwn;

            $scope.dashboards = $scope.resultData.dashboards;
            $scope.preferredId = $scope.resultData.preferredId;
            $scope.newpanelschema = $scope.resultData.newPanelSchema;
            $scope.saveDashboardSchema = $scope.resultData.saveDashboardSchema;
            $scope.profiles = $scope.resultData.profiles;
            $scope.panelschemas = $scope.resultData.panelSchemas;
            $scope.applications = $scope.resultData.applications;
        };

        $scope.create = function () {
            $scope.creatingDashboard = true;
            $scope.creatingpersonal = true;
            $scope.dashboard = {
                title: "New Dashboard",
                panels: []
            };
        }

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
        }

        $scope.saveDashboard = function () {
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
        }

        $scope.cancelDashboard = function () {
            $scope.creating = false;
        }

        $scope.addpanel = function () {
            var dm = {};
            dm.numberofpanels = $scope.dashboard.panels.length;
            modalService.show($scope.newpanelschema, dm, {
                title: "Add Panel", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    scope.associationOptions['applications'] = $scope.applications;
                    //                scope.$digest();
                }
            });
        }

        $scope.createNewPanel = function () {
            var schema = $scope.panelschemas[$scope.paneltype];

            modalService.show(schema, null, {
                title: "Create Panel", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    scope.associationOptions['applications'] = $scope.applications;
                    //                scope.$digest();
                }
            });
        }

   

        $scope.doInit();

        $scope.$on('dash_createpanel', function (event, paneltype) {
            var schema = $scope.panelschemas[paneltype];

            modalService.show(schema, null, {
                title: "Create Panel", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    scope.associationOptions['applications'] = $scope.applications;
                    //                scope.$digest();
                }
            });
        });

        $scope.$on('dash_dashsaved', function (event, dashboard) {
            modalService.hide();
        });

        $scope.$on('dash_panelcreated', function (event, dashboard) {
            modalService.hide();
        });

        $scope.$on('dash_panelassociated', function (event, panel, row, column) {
            var dashboard = $scope.dashboard;
            modalService.hide();
            dashboardAuxService.readjustLayout(dashboard, row, column);
            dashboardAuxService.readjustPositions(dashboard, panel, row, column);
        });

        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue && $scope.resultObject.crudSubTemplate.indexOf("/Shared/dashboard/templates/Dashboard.html") != -1) {
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

