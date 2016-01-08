(function (angular) {
    "use strict";


angular.module('sw_layout')
   .directive('dashboardgridpanel', function ($timeout, $log, $rootScope, contextService, searchService) {
    "ngInject";

    function doInit(scope) {
        scope.schema = {
            applicationName: scope.paneldatasource.panel.application,
            schemaId: scope.paneldatasource.panel.schemaRef,
            stereotype: 'list',
            properties: {},
            commandSchema:{}
        };
        scope.application = scope.paneldatasource.panel.application;
        // TODO: We'll need to define an unique name for it. 
        scope.metadataid = scope.paneldatasource.panel.alias || null;
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
                metadataid: $scope.metadataid,
                pageSize: $scope.paneldatasource.panel['limit'],
                searchSort: $scope.paneldatasource.panel['defaultSortField'],
                fieldstodisplay: $scope.paneldatasource.panel['appFields'],
            });
        },

        link: function (scope, element, attrs) {
            //building a schema object representation to propagate to crud_list.html
            doInit(scope);
        }
    };
});

})(angular);