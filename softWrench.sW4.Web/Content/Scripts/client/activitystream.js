function ActivityStream($scope, $http, $log, $interval, $timeout, redirectService, contextService, $rootScope, alertService) {

    var log = $log.getInstance('sw4.activityStream');
    var jScrollPaneAPI;
    var throttleTimeout;

    $scope.formatDate = function (notificationDate) {
        var currentDate = new Date();
        var nowMils = currentDate.getTime() - (currentDate.getTimezoneOffset() * 60000);

        //Add 'Z' to fix Firefox error
        var notificationMils = new Date(notificationDate + 'Z').getTime();
        var differenceMils = nowMils - notificationMils;
        var dateMessage = moment.duration(differenceMils, "milliseconds").humanize();

        return 'About ' + dateMessage + ' ago'; // (' + notificationDate.replace('T', ' ') + ') [' + notificationMils + ']';
    };

    $scope.markAllRead = function () {
        log.debug('markAllRead');
        var confirmationMessage = "Mark all notifications as read?";
        return alertService.confirm(null, null, function () {
            var controllerToUse = "Notification";
            var actionToUse = "UpdateNotificationReadFlag";
            var parameters = {};
            parameters.role = 'allRole';
        
            var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
            $http.post(rawUrl, angular.toJson($scope.activities)).success(function() {
                    $scope.refreshStream();
            }).error(
                function (data) {
                    var errordata = {
                        errorMessage: "error opening action {0} of controller {1} ".format(actionToUse, controllerToUse),
                        errorStack: data.message
                    }
                    $rootScope.$broadcast("sw_ajaxerror", errordata);
                }
            );
        }, confirmationMessage);
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
        parameters.id = activity.id;        $("#activitystream").toggleClass('open');
        $scope.setPaneHeight();
        jScrollPaneAPI = $('#activitystream .scroll').jScrollPane().data('jsp');

        var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
        $http.post(rawUrl).success(
            function (data) {
                $("#activitystream").toggleClass('open');

                var param = {};
                param.id = activity.id;
                if (activity.application.isEqual('workorder')) {
                    param.id = activity.uId;
                }

                if (!activity.parentApplication) {
                    redirectService.goToApplicationView(activity.application, "editdetail", "input", null, param, null);
                } else {
                    param.id = activity.parentId;
                    redirectService.goToApplicationView(activity.parentApplication, "editdetail", "input", null, param, null, function () { contextService.setActiveTab(activity.application + '_');});
                }

                $scope.refreshStream();
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

                if (typeof jScrollPaneAPI !== 'undefined') {
                    jScrollPaneAPI.reinitialise();
                }
              
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

    $scope.setPaneHeight = function () {
        log.debug('setPaneHeight');

        var headerHeight = $('#activitystream header').height();
        var panePaddingTop = parseInt($('#activitystream .pane').css('padding-top'));
        var panePaddingBottom = parseInt($('#activitystream .pane').css('padding-bottom'));

        $('#activitystream .scroll').height($(window).height() - headerHeight - panePaddingTop - panePaddingBottom);
    }

    //automatically refresh the activity stream every five minutes
    $interval(function () {
        $scope.refreshStream();
    }, 60000 * 5);

    //open and close activity pane
    $('#activitystream .handle').click(function () {
        $("#activitystream").toggleClass('open');
        $scope.setPaneHeight();
        jScrollPaneAPI = $('#activitystream .scroll').jScrollPane().data('jsp');
    });

    //set window height and reinitialize scroll pane if windows is resized
    $(window).bind('resize', function () {
        // IE fires multiple resize events while you are dragging the browser window which
        // causes it to crash if you try to update the scrollpane on every one. So we need
        // to throttle it to fire a maximum of once every 50 milliseconds...
        if (typeof jScrollPaneAPI !== 'undefined') {
            if (!throttleTimeout) {
                throttleTimeout = setTimeout(function () {
                    $scope.setPaneHeight();

                    jScrollPaneAPI.reinitialise();
                    throttleTimeout = null;
                }, 50);
            }
        }
    });

    //prevent window scrolling after reaching end of navigation pane 
    $(document).on('mousewheel', '#activitystream .scroll',
      function (e) {
          var delta = e.originalEvent.wheelDelta;
          this.scrollTop += (delta < 0 ? 1 : -1) * 30;
          e.preventDefault();
      });

    //get the current notifications
    $scope.refreshStream();

    //open notification pane by default, TODO: remove for production
    //$timeout(function () {
    //    $('#activitystream .handle').trigger('click');
    //}, 0);
}
