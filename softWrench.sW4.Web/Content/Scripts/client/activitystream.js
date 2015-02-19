function ActivityStream($scope, $http, $log, redirectService) {

    var log = $log.getInstance('sw4.activityStream');


    $scope.formatDate = function (notificationDate) {
        var currentDate = new Date();
        var nowMils = currentDate.getTime() - (currentDate.getTimezoneOffset() * 60000);
        var notificationMils = new Date(notificationDate).getTime();

        var differenceMils = nowMils - notificationMils;
        var dateMessage = moment.duration(differenceMils, "milliseconds").humanize();

        return 'About ' + dateMessage + ' ago.';
    };

    $scope.markAllRead = function () {
        log.debug('markAllRead');

        //TODO: mark all notifications read (confirmation alert?)
    }

    $scope.openLink = function (activity) {
        log.debug('openLink');

        //TODO: mark current notification as read and open link
        var controllerToUse = "Notification";
        var actionToUse = "UpdateNotificationReadFlag";
        var parameters = {};
        parameters.role = 'allRole';
        parameters.application = activity.application;
        parameters.id = activity.id;

        var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
        $http.post(rawUrl).success(
            function (data) {
                var param = {};
                if (!activity.parentApplication) {
                    param.id = activity.id;
                    redirectService.goToApplicationView(activity.application, "editdetail", "input", null, param, null);
                } else {
                    param.id = activity.parentId;
                    redirectService.goToApplicationView(activity.parentApplication, "editdetail", "input", null, param, null);
                }
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
    

    $scope.refreshStream = function () {
        log.debug('refreshStream');

        var controllerToUse = "Notification";
        var actionToUse = "GetNotifications";

        var parameters = {};
        parameters.role = 'allRole';

        var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
        $http.get(rawUrl).success(
            function (data) {
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

    //automatically refresh the activity stream every five minutes
    $interval(function () {
        $scope.refreshStream();
    }, 60000 * 5);

    //get the current notifications
    $scope.refreshStream();

    //open and close activity pane
    $('#activitystream .handle').click(function () {
        $("#activitystream").toggleClass('open');
    });

    //set the activity pane the same height as the window
    $(window).resize(function () {
        $('#activitystream').height($(window).height());
    });
}
