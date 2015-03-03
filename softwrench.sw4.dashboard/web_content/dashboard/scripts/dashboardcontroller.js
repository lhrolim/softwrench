﻿var app = angular.module('sw_layout');


app.directive('dashboardrendered', function ($timeout, $log, $rootScope, eventService) {

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
    '$scope','$log','$timeout', 'modalService', 'fieldService', 'dashboardAuxService','contextService','alertService',
    function ($scope,$log,$timeout, modalService, fieldService, dashboardAuxService,contextService,alertService) {

        $scope.doInit = function () {

            $scope.canCreateOwn = $scope.resultData.canCreateOwn;
            $scope.canCreateShared = $scope.resultData.canCreateShared;

            $scope.canCreteDashboards = $scope.canCreateShared || $scope.canCreateOwn;
            $scope.canCreateBoth = $scope.canCreateShared && $scope.canCreateOwn;

            $scope.dashboards = $scope.resultData.dashboards;
            $scope.newpanelschema = $scope.resultData.newPanelSchema;
            $scope.saveDashboardSchema = $scope.resultData.saveDashboardSchema;
            $scope.profiles = $scope.resultData.profiles;
            $scope.panelschemas = $scope.resultData.panelSchemas;
            $scope.applications = $scope.resultData.applications;
            $scope.currentdashboardid = $scope.resultData.preferredId;
            $scope.dashboard = $scope.getCurrentDashboardById($scope.currentdashboardid);
            var userData = contextService.getUserData();
            $scope.userid = userData.id;
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

        $scope.getCurrentDashboardById = function(id){
            var dashboards = $scope.dashboards;
            for (var i = 0; i < dashboards.length; i++) {
                if (dashboards[i].id == id) {
                    return dashboards[i];
                }
            }
            return dashboards[0];
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
            $scope.creatingDashboard = false;
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

        $scope.getActiveClass = function(tabid) {
            return tabid == $scope.currentdashboardid ? "active" : null;
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

        $scope.$on('dash_changeselecteddashboard', function (event, dashboardid) {
            $scope.currentdashboardid = dashboardid;
        });

        $scope.$on('dash_dashsaved', function (event, dashboard) {
            modalService.hide();
        });

        $scope.$on('dash_panelcreated', function (event, dashboard) {
            modalService.hide();
        });

        $scope.$on('dash_changedashboard', function (event, dashboardid) {
            $scope.currentdashboardid = dashboardid;
        });

        $scope.$on('dash_finishloading', function (event, dashboardid) {
            $timeout(function () {
                $('.compositiondetailtab li>a').each(function () {
                    var $this = $(this);
                    $this.click(function (e) {
                        e.preventDefault();
                        if ($scope.isEditingAnyDashboard) {
                            alertService.alert('Please, finish editing the current dashboard.');
                            return;
                        }

                        
                        $this.tab('show');
                        var dashid = $(this).data('tabid');
                        $scope.currentdashboardid = dashid;
                        $scope.dashboard = $scope.getCurrentDashboardById(dashid);
                        $scope.dashboard = 
                        log.trace('lazy loading dashboard {0}'.format(dashid));
                    });
                });
            }, 0, false);
        });


        $scope.$on('dash_panelassociated', function (event, panel, row, column) {
            var dashboard = $scope.dashboard;
            modalService.hide();
            dashboardAuxService.readjustLayout(dashboard, row, column);
            dashboardAuxService.readjustPositions(dashboard, panel, row, column);
        });

        $scope.isEditing = function (dashboardid) {
            return $scope.currentdashboardid == dashboardid && $scope.isEditingAnyDashboard;
        }

        $scope.editDashboard = function (dashboardId) {
            $scope.isEditingAnyDashboard = true;
        }

        $scope.finishEditingDashboard = function (dashboardId) {
            $scope.isEditingAnyDashboard = false;
        }

        $scope.canEditDashboard = function (dashboard) {
            if ($scope.canCreateBoth) {
                return true;
            }
            return dashboard.createdby == $scope.userid;
        }


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

