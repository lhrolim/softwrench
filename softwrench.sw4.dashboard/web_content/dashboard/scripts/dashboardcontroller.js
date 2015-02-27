var app = angular.module('sw_layout');

app.controller('DashboardController', [
    '$scope', 'modalService', 'fieldService', 'dashboardAuxService',
    function ($scope, modalService, fieldService, dashboardAuxService) {

        $scope.doInit = function () {

            $scope.canCreateOwn = $scope.resultData.canCreateOwn;
            $scope.canCreateShared = $scope.resultData.canCreateShared;

            $scope.canCreteDashboards = $scope.canCreateShared || $scope.canCreateOwn;
            $scope.canCreateBoth = $scope.canCreateShared && $scope.canCreateOwn;

            $scope.dashboards = $scope.resultData.dashboards;
            $scope.preferredId = $scope.resultData.preferredId;
            $scope.newpanelschema = $scope.resultData.newPanelSchema;
            $scope.panelschemas = $scope.resultData.panelSchemas;
            $scope.applications = $scope.resultData.applications;
        };

        $scope.create = function () {
            $scope.creating = true;
            $scope.creatingpersonal = true;
            $scope.dashboard = {
                title: "New Dashboard *",
                panels: []
            };
        }

        $scope.cancel = function () {
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

        $scope.getRows = function () {
            if (!$scope.dashboard || !$scope.dashboard.layout) {
                return 0;
            }
            return $scope.dashboard.layout.split(',');
        }

        $scope.getColumnsOfRow = function (row) {
            if (!$scope.dashboard || !$scope.dashboard.layout) {
                return 0;
            }
            return parseInt($scope.dashboard.layout.split(',')[row-1]);
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

        $scope.$on('dash_panelassociated', function (event, panel, row, column) {
            var dashboard = $scope.dashboard;
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
            $rootScope.$broadcast("dash_createpanel", $scope.datamap.paneltype);

        }

    }
]);

