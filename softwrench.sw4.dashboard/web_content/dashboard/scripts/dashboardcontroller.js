var app = angular.module('sw_layout');

app.controller('DashboardController', [
    '$scope', 'modalService', 'fieldService',
    function ($scope, modalService, fieldService) {

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
            $scope.newtitle = "New Dashboard*";
            $scope.creatingpersonal = true;
        }

        $scope.cancel = function () {
            $scope.creating = false;
        }

        $scope.addpanel = function () {
            modalService.show($scope.newpanelschema, null, {
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

