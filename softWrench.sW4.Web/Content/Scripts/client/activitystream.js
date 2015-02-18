function ActivityStream($scope, $http, $log) {

    var log = $log.getInstance('sw4.activityStream');
    //xszlog.debug('ActivityStream Controller');

    var controllerToUse = "Notification";
    var actionToUse = "GetNotifications";

    var parameters = {};
    parameters.role = 'allRole';

    var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
    $http.get(rawUrl).success(
        function (data) {
            //var test = data;
            //$rootScope.$broadcast("sw_redirectactionsuccess", data);

            $scope.activities = data;
            log.debug($scope.activities);
        }).error(
        function (data) {
            var errordata = {
                errorMessage: "error opening action {0} of controller {1} ".format(actionToUse, controllerToUse),
                errorStack: data.message
            }
            $rootScope.$broadcast("sw_ajaxerror", errordata);
        }
    );
}