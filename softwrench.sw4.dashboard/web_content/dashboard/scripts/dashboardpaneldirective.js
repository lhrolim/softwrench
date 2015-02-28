var app = angular.module('sw_layout');

app.directive('dashboardpanel', function ($timeout, $log, $rootScope, contextService, dashboardAuxService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Shared/dashboard/templates/dashboardpaneldirective.html'),
        scope: {
            panelrow: '=',
            panelcol: '@',
            paneldatasource: '@'
        }
    }
});