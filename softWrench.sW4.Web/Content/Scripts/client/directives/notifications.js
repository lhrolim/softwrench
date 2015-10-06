var app = angular.module('sw_layout');

app.directive('notifications', function (contextService, $log) {
    var log = $log.getInstance('sw4.notifications');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/notifications.html'),
        controller: function ($scope) {
           
        }
    }
});
