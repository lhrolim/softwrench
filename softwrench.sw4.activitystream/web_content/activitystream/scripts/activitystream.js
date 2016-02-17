(function (angular) {
    "use strict";

angular.module('sw_layout').directive('activitystream', function (contextService) {
    "ngInject";

    return {
        restrict: 'E',

        // with previous config {replace: true, transclude: false} (for some bewildering reason)
        // the child elements of the directive's element couldn't access it's controller's methods
        // that way the activity stream's handle tab couldn't toggle it's open/close state 
        // like so (in jade for simplicity's sake):
        // activitystream
        //   div //- parent div could access the methods exposed in $scope by controller
        //      div(ng-click='acontrollermethod()') //- child div couldn't access the methods
        //        replace: true,
        //        transclude: false,

        templateUrl: contextService.getResourceUrl('/Content/Shared/activitystream/templates/activitystream.html'),
        scope: {
            activities: '='
        },

        link: function (scope) {
            scope.$name = 'crudbody';

            function handleResize() {
                var activityWidth = 0;

                //if pane is open get width
                if ($('#activitystream').hasClass('open')) {
                    activityWidth = $('#activitystream').width();
                }

                //var gridOffset = activityWidth + gridPadding;
                //var headerOffset = activityWidth;

                //update widths
                $('.site-header').width($('.site-header').css('width', 'calc(100% - ' + activityWidth + 'px)'));

                if ($('.site-header').css('position') == 'fixed') {
                    $('#affixpagination').width($('#affixpagination').css('width', 'calc(100% - ' + activityWidth + 'px)'));
                } else {
                    $('#affixpagination').width($('#affixpagination').css('width', '100%'));
                }

                $('.listgrid-thead').width($('.listgrid-thead').css('width', 'calc(100% - ' + activityWidth + 'px)'));
                $('.content').width($('.content').css('width', 'calc(100% - ' + activityWidth + 'px)'));
            }

            var handler = window.debounce(handleResize, 300);
            angular.element(window).on("resize", handler);
            scope.$on("$destroy", function () {
                angular.element(window).off("resize", handler);
            });
        },

        controller: function ($scope, $http, $log, $interval, $timeout, redirectService,
            contextService, $rootScope, alertService) {

            var log = $log.getInstance('sw4.activityStream');

            $scope.hiddenToggle = false;
            $scope.enableFilter = false;
            $scope.availableProfiles = [];

            $scope.activityStreamEnabled = function () {
                return contextService.fetchFromContext('activityStreamFlag', false, true);
            };

            if ($scope.activityStreamEnabled()) {
                $('html').addClass('activitystream');
            };


            $scope.hasmultipleprofiles = function () {
                return this.getMultiplesProfiles().length > 1;
            };

            $scope.getMultiplesProfiles = function () {
                return $scope.availableProfiles || [];
            };

            $scope.changeCurrentProfile = function () {
                this.refreshStream();
            };

            $scope.clearFilter = function () {
                log.debug('clearFilter');

                $scope.filterText = '';
                $(window).trigger('resize');
            }

            $scope.displayHidden = function (activity) {
                //always show unhidden
                if (!activity.isRead) {
                    return true;
                }

                return $scope.hiddenToggle;
            };

            $scope.deciveType = function () {
                return DeviceDetect.catagory.toLowerCase();
            };

            $scope.formatDate = function (notificationDate) {
                var currentDate = new Date();
                var nowMils = currentDate.getTime();

                var notificationMils = new Date(notificationDate).getTime();
                var differenceMils = nowMils - notificationMils;
                var dateMessage = moment.duration(differenceMils, "milliseconds").humanize();

                return 'About ' + dateMessage + ' ago'; // + ' (' + notificationMils + ')';
            };

            $scope.getAllHidden = function () {
                log.debug('getAllHidden');

                if ($scope.activities != null && $scope.activities.length > 0) {
                    if ($scope.activities.length === $scope.readCount) {
                        return true;
                    }
                } else {
                    return false;
                }
            };

            $scope.markAllRead = function () {
                log.debug('markAllRead');

                var confirmationMessage = "Mark all notifications as read?";
                return alertService.confirm(null, null, function () {
                    var controllerToUse = "Notification";
                    var actionToUse = "UpdateNotificationReadFlag";
                    var parameters = {};
                    parameters.securityGroup = $scope.activityProfile;

                    var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                    $http.post(rawUrl, angular.toJson($scope.activities)).success(function () {
                        log.debug('Mark All Read Complete');
                        $scope.refreshStream();
                    });
                }, confirmationMessage);
            };

            $scope.markRead = function (activity) {
                log.debug('markRead', activity);

                var controllerToUse = "Notification";
                var actionToUse = "UpdateNotificationReadFlag";

                var parameters = {};
                parameters.securityGroup = $scope.activityProfile;
                parameters.application = activity.application;
                parameters.id = activity.id;
                parameters.rowstamp = activity.rowstamp;
                parameters.isread = !activity.isRead;

                var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                $http.post(rawUrl).success(
                    function (data) {
                        log.debug('Mark Read Complete');
                        $scope.refreshStream();
                    }).error(
                    function (data) {
                        var errordata = {
                            errorMessage: "error opening action {0} of controller {1} ".format(actionToUse, controllerToUse),
                            errorStack: data.message
                        }
                        $rootScope.$broadcast("sw_ajaxerror", errordata);
                        alertService.notifyexception(errordata);
                    });
            };

            $scope.buildActivityNotificationParam = function (activity, parent) {
                var param = {
                    id: activity.uId,
                    applicationName: activity.application
                };
                if (activity.parentApplication) {
                    param.id = activity.parentUId;
                    param.applicationName = activity.parentApplication;
                }
                return param;
            };

            $scope.disposeActivityStream = function (activity, parent) {
                //if the header is not fixed (mobile), hide the actity pane
                if ($('.site-header').css('position') !== 'fixed') {
                    $scope.toggleActivityStream();
                }
                if (activity.parentApplication && "true".equalIc(parent)) {
                    $timeout(function () {
                        //this timeout is needed because a digest is already in progress
                        redirectService.redirectToTab(activity.application + '_');
                    }, 0, false);
                }
                $scope.refreshStream();
            };

            $scope.openLink = function (activity, parent) {
                log.debug('openLink');
                var redirectParameters = $scope.buildActivityNotificationParam(activity, parent);

                redirectService.goToApplicationView(redirectParameters.applicationName, "editdetail", "input", null, redirectParameters)
                    .then(function (data) {
                        var parameters = {
                            securityGroup: $scope.activityProfile,
                            application: activity.application,
                            id: activity.id,
                            rowstamp: activity.rowstamp
                        };
                        var rawUrl = url("/api/generic/Notification/UpdateNotificationReadFlag?" + $.param(parameters));
                        return $http.post(rawUrl);
                    }).then(function (data) {
                        $scope.disposeActivityStream(activity, parent);
                    });

            };

            $scope.refreshStream = function (silent) {
                log.debug('refreshStream');

                var controllerToUse = "Notification";
                var actionToUse = "GetNotifications";

                var parameters = {
                    currentProfile: $scope.activityProfile
                };

                var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                $http.get(rawUrl, { avoidspin: true }).success(
                    function (data) {
                        $scope.readCount = data.readCount;
                        $scope.activities = data.notifications;
                        $scope.refreshRate = data.refreshRate;
                        $scope.availableProfiles = data.availableProfiles;
                        $scope.activityProfile = data.selectedProfile;

                        $scope.statusAllHidden = $scope.getAllHidden();

                        //TODO: remove for production
                        //$scope.activities = demoNotifications;

                        log.debug($scope.activities);
                    });
            };

            $scope.setPaneHeight = function () {
                log.debug('setPaneHeight');

                var headerHeight = $('#activitystream header').height();
                var panePaddingTop = parseInt($('#activitystream .pane').css('padding-top'));
                var panePaddingBottom = parseInt($('#activitystream .pane').css('padding-bottom'));

                return $(window).height() - headerHeight - panePaddingTop - panePaddingBottom;
            };

            $scope.toggleFilter = function () {
                log.debug('toggleFilter');

                $scope.enableFilter = !$scope.enableFilter;
                $scope.filterText = '';
                $(window).trigger('resize');
            };

            $scope.toggleHidden = function () {
                log.debug('toggleHidden');

                $scope.hiddenToggle = !$scope.hiddenToggle;

                //resize/position elements
                $(window).trigger('resize');
            };

            $scope.toggleActivityStream = function () {
                //open and close activity pane
                $('#activitystream').toggleClass('open');

                //resize/position elements
                $(window).trigger('resize');
            };

            $scope.$watch('filterText', function () {
                $(window).trigger('resize');
            });

            //get the current notifications, then automatically refresh
            var refreshLoop = function () {
                log.debug('refreshLoop', $scope.refreshRate);

                var refreshTimeout;

                if (typeof $scope.refreshRate == 'undefined' || $scope.refreshRate === 0) {
                    //refresh every five minutes if the refreshRate is not set
                    refreshTimeout = 5;
                } else {
                    //use the refreshRate from the backend
                    refreshTimeout = $scope.refreshRate;
                }

                $scope.refreshStream(true);
                $timeout(refreshLoop, 1000 * 60 * refreshTimeout);
                log.debug('refreshTimeout', refreshTimeout);
            };
            refreshLoop();

            //open notification pane by default, TODO: remove for production
            //$timeout(function() {
            //    $('#activitystream .handle').trigger('click');
            //}, false);

            //var demoNotifications = [];

            //var newNotification = {};
            //newNotification.application = "commlog";
            //newNotification.createBy = "JBREIDENTHAL";
            //newNotification.flag = "created";
            //newNotification.icon = "fa-envelope-o";
            //newNotification.id = "22967305";
            //newNotification.isRead = false;
            //newNotification.label = "communication";
            //newNotification.notificationDate = "2015-09-11T09:51:24";
            //newNotification.parentApplication = "servicerequest";
            //newNotification.parentId = "79788";
            //newNotification.parentLabel = "service request";
            //newNotification.parentUId = 62671;
            //newNotification.rowstamp = 185626997;
            //newNotification.summary = "UPDATE: SR##79788## -DWL -DWLRR7 User on Rowan Rentless rig";
            //newNotification.targetSchema = null;
            //newNotification.uId = 23162490;
            //demoNotifications.push(newNotification);
            //for (var i = 0; i < 21; i++) {
            //    var newObject = $.extend({}, newNotification);
            //    demoNotifications.push(newObject);
            //}
        }
    }
});

})(angular);
