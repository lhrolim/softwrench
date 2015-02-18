var app = angular.module('sw_layout');

app.directive('activityStream', function ($log, $timeout) {
    var log = $log.getInstance('sw4.activityStream');

    return {
        link: function (scope, element, attr) {
            log.debug('activityStream');


        }
    }
});