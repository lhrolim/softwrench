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
            scope: true,

            link: function (scope) {
                scope.$name = 'crudbody';
            },

            controller: ["$scope", "$http", "$log", "$interval", "$timeout", "redirectService", "contextService", "$rootScope", "alertService", "sidePanelService", "userService",
                function ($scope, $http, $log, $interval, $timeout, redirectService, contextService, $rootScope, alertService, sidePanelService, userService) {

                    var log = $log.getInstance('sw4.activityStream');

                    $scope.panelid = "activitystream";
                    $scope.hiddenToggle = false;
                    $scope.enableFilter = false;
                    $scope.availableProfiles = [];

                    var activityStreamEnabled = function () {
                        return contextService.fetchFromContext('activityStreamFlag', false, true);
                    };

                    if (!activityStreamEnabled()) {
                        sidePanelService.hide($scope.panelid);
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
                        return alertService.confirm(confirmationMessage).then(function () {
                            var controllerToUse = "Notification";
                            var actionToUse = "UpdateNotificationReadFlag";
                            var parameters = {};
                            parameters.securityGroup = $scope.activityProfile;

                            var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                            $http.post(rawUrl, angular.toJson($scope.activities)).then(function () {
                                log.debug('Mark All Read Complete');
                                $scope.refreshStream();
                            });
                        });
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
                        $http.post(rawUrl).then(function () {
                            log.debug('Mark Read Complete');
                            $scope.refreshStream();
                        }).catch(function (response) {
                            const data = response.data;
                            var errordata = {
                                errorMessage: "error opening action {0} of controller {1} ".format(actionToUse, controllerToUse),
                                errorStack: data.message
                            }
                            $rootScope.$broadcast(JavascriptEventConstants.ErrorAjax, errordata);
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
                        $http.get(rawUrl, { avoidspin: true }).then(function (response) {
                            const data = response.data;
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
                        return sidePanelService.calculateScrollPanelHeight($scope.panelid);
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
                    };

                    $scope.toggleActivityStream = function () {
                        sidePanelService.toggle($scope.panelid);
                    };

                    // returns the style of the indicator of unread activities
                    $scope.getUnreadStyle = function () {
                        return { top: sidePanelService.getContext($scope.panelid).top - 70 + "px" };
                    }

                    // hides the activitystream if user do not have the role
                    if (!userService.hasRole(["sysadmin"]) && !userService.hasRole(["ROLE_NOTIFICATIONS"])) {
                        sidePanelService.hide($scope.panelid);
                        return;
                    }

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

                    if (sidePanelService.getExpandedPanelFromPreference() === $scope.panelid && !sidePanelService.isOpened($scope.panelid)) {
                        $scope.toggleActivityStream();
                    }
                }]
        }
    });

})(angular);
