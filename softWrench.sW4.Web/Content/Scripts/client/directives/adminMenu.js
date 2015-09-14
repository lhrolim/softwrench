var app = angular.module('sw_layout');

app.directive('adminMenu', function (contextService, $log) {
    var log = $log.getInstance('sw4.adminMenu');

    return {
        controller: function ($scope) {
            $scope.showClassicMenu = function () {
                return contextService.fetchFromContext('UIShowClassicAdminMenu', false, true);
            };
        }
    }
});
