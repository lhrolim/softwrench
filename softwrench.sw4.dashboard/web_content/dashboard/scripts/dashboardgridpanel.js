var app = angular.module('sw_layout');



app.directive('dashboardgridpanel', function ($timeout, $log, $rootScope, contextService, searchService) {

    function doInit(scope) {
        scope.schema = {
            applicationName: scope.paneldatasource.panel.application,
            schemaid: scope.paneldatasource.panel.schemaRef,
            stereotype: 'list',
            properties: {},
            commandSchema:{}
        };
        scope.application = scope.paneldatasource.application;
        scope.fieldstodisplay = scope.paneldatasource.panel['appFields'];
        scope.datamap = {};
    }

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Shared/dashboard/templates/dashboardgridpanel.html'),
        scope: {
            panelrow: '=',
            panelcol: '@',
            paneldatasource: '=',
            dashboardid:'='
        },

       

        controller: function ($scope, $http) {
            doInit($scope);
            searchService.refreshGrid({}, {
                panelid: $scope.paneldatasource.id,
                pageSize: $scope.paneldatasource.panel['limit'],
                searchSort: $scope.paneldatasource.panel['defaultSortField'],
                fieldstodisplay: $scope.paneldatasource.panel['appFields'],
            });
        },

        link: function (scope, element, attrs) {
            //building a schema object representation to propagate to crud_list.html
            doInit(scope);
        }
    }
});