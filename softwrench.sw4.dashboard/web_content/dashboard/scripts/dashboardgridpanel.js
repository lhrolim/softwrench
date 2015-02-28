var app = angular.module('sw_layout');

app.directive('dashboardgridpanel', function ($timeout, $log, $rootScope, contextService, redirectService, dashboardAuxService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Shared/dashboard/templates/dashboardgridpanel.html'),
        scope: {
            panelrow: '=',
            panelcol: '@',
            paneldatasource: '@'
        },

        controller: function ($scope, $http, $rootScope) {

            $scope.getPanelSourceData = function () {
                // TODO: See if we can pass the object as json instead of converting it. 
                var dashboardPanelInfo = angular.fromJson($scope.paneldatasource);

                if (dashboardPanelInfo != null) {
                    var title = dashboardPanelInfo.panel['title'];
                    var schemaReference = dashboardPanelInfo.panel['schemaRef'];
                    var application = dashboardPanelInfo.panel['application'];

                    var controller = 'data';
                    var redirectUrl = redirectService.getApplicationUrl(application, schemaReference, null, title);

                    // call service and apply data into crud list object.
                    $http.get(redirectUrl)
                        .success(
                            function (data) {
                                // store data into individual panel
                                $scope.dashboardpanel = data;
                        })
                        .error(
                            function (data) {
                                var errordata = {
                                    errorMessage: "error opening action {0} of controller {1} ".format(action, controller),
                                    errorStack: data.message 
                                }

                                $rootScope.$broadcast("sw_ajaxerror", errordata);
                        });
                }
            }

            $scope.getPanelSourceData();
        },

        link: function (scope, element, attrs) {

        }
    }
});