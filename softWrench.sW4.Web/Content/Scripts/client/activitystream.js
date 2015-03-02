var app = angular.module('sw_layout');

app.directive('activitystream', function(contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/notifications/activitystream.html'),
        scope: {
            activities: '='
        },

        link: function(scope) {
            scope.$name = 'crudbody';
        },
        controller: function($scope, $http, $log, $interval, $timeout, redirectService,
            contextService, $rootScope, alertService) {

            var log = $log.getInstance('sw4.activityStream');
            var jScrollPaneAPI;
            var throttleTimeout;
            $scope.hiddenToggle = false;
            var rootAvoidingSpin;
     
            $scope.activityStreamEnabled = function () {
                return contextService.fetchFromContext("notificationStreamFlag", false, true);
            };

            $scope.displayHidden = function (activity) {
                //always show unhidden
                if (!activity.isRead) {
                    return true;
                }

                return $scope.hiddenToggle;
            }

            $scope.deciveType = function () {
                return DeviceDetect.catagory.toLowerCase();
            }

            $scope.formatDate = function(notificationDate) {
                var currentDate = new Date();
                var nowMils = currentDate.getTime() - (currentDate.getTimezoneOffset() * 60000);

                //add 'Z' to datetime fix Firefox error
                var notificationMils = new Date(notificationDate + 'Z').getTime();
                var differenceMils = nowMils - notificationMils;
                var dateMessage = moment.duration(differenceMils, "milliseconds").humanize();

                return 'About ' + dateMessage + ' ago'; // (' + notificationDate.replace('T', ' ') + ') [' + notificationMils + ']';
            };

            $scope.getAllHidden = function () {
                log.debug('getAllHidden');

                //if activities is unset, return false
                if (typeof $scope.activities !== 'undefined') {

                    //if no messages, return false
                    if ($scope.activities.length === 0) {
                        return false;
                    }

                    //loop through all activity, if all are hidden return true
                    return $scope.activities.every(function (e) {
                        return e.isRead;
                    });
                } else {
                    return false;
                }
            }

            $scope.markAllRead = function() {
                log.debug('markAllRead');

                var confirmationMessage = "Mark all notifications as read?";
                return alertService.confirm(null, null, function() {
                    var controllerToUse = "Notification";
                    var actionToUse = "UpdateNotificationReadFlag";
                    var parameters = {};
                    parameters.role = 'allRole';

                    var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                    $http.post(rawUrl, angular.toJson($scope.activities)).success(function () {
                        log.debug('Mark All Read Complete');
                        $scope.refreshStream();
                    }).error(
                        function(data) {
                            var errordata = {
                                errorMessage: "error opening action {0} of controller {1} ".format(actionToUse, controllerToUse),
                                errorStack: data.message
                            }
                            $rootScope.$broadcast("sw_ajaxerror", errordata);
                        }
                    );
                }, confirmationMessage);
            }

            $scope.markRead = function (activity) {
                log.debug('markRead', activity);

                var controllerToUse = "Notification";
                var actionToUse = "UpdateNotificationReadFlag";

                var parameters = {};
                parameters.role = 'allRole';
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
                    });
            }

            $scope.openLink = function(activity, parent) {
                log.debug('openLink');

                var controllerToUse = "Notification";
                var actionToUse = "UpdateNotificationReadFlag";
                var parameters = {};
                parameters.role = 'allRole';
                parameters.application = activity.application;
                parameters.id = activity.id;
                parameters.rowstamp = activity.rowstamp;

                var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                $http.post(rawUrl).success(
                    function(data) {
                        $scope.toggleActivityStream();

                        var param = {};
                        param.id = activity.id;
                        if (activity.application.isEqual('workorder')) {
                            param.id = activity.uId;
                        }

                        if (!activity.parentApplication) {
                            redirectService.goToApplicationView(activity.application, "editdetail", "input", null, param, null);
                        } else {
                            param.id = activity.parentId;
                            if (parent!= null && parent.equalIc('true')) {
                                redirectService.goToApplicationView(activity.parentApplication, "editdetail", "input", null, param, null);
                            }else 
                                redirectService.goToApplicationView(activity.parentApplication, "editdetail", "input", null, param, null, function() { contextService.setActiveTab(activity.application + '_'); });
                        }

                        $scope.refreshStream();
                    }).error(
                    function(data) {
                        var errordata = {
                            errorMessage: "error opening action {0} of controller {1} ".format(actionToUse, controllerToUse),
                            errorStack: data.message
                        }
                        $rootScope.$broadcast("sw_ajaxerror", errordata);
                    }
                );
            }

            $scope.refreshStream = function(silent) {
                log.debug('refreshStream');

                var controllerToUse = "Notification";
                var actionToUse = "GetNotifications";

                var parameters = {};
                parameters.role = 'allRole';

                //turn off spinner, if not alread off
                if (!$rootScope.avoidspin) {
                    rootAvoidingSpin = false;
                    $rootScope.avoidspin = silent;
                } else {
                    rootAvoidingSpin = true;
                }

                var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                $http.get(rawUrl).success(
                    function(data) {
                        $scope.activities = data;
                        $scope.statusAllHidden = $scope.getAllHidden();

                        //resize the scroll pane if needed
                        if (typeof jScrollPaneAPI !== 'undefined') {
                            jScrollPaneAPI.reinitialise();
                        }

                        //restore spinner to orginal value
                        $rootScope.avoidspin = rootAvoidingSpin;
                        log.debug($scope.activities);
                    }).error(
                    function(data) {
                        var errordata = {
                            errorMessage: "error opening action {0} of controller {1} ".format(actionToUse, controllerToUse),
                            errorStack: data.message
                        }
                        
                        //restore spinner to orginal value
                        $rootScope.avoidspin = rootAvoidingSpin;
                        $rootScope.$broadcast("sw_ajaxerror", errordata);
                    }
                );
            }

            $scope.setPaneHeight = function() {
                log.debug('setPaneHeight');

                var headerHeight = $('#activitystream header').height();
                var panePaddingTop = parseInt($('#activitystream .pane').css('padding-top'));
                var panePaddingBottom = parseInt($('#activitystream .pane').css('padding-bottom'));

                $('#activitystream .scroll').height($(window).height() - headerHeight - panePaddingTop - panePaddingBottom);
            }

            $scope.toggleHidden = function () {
                log.debug('toggleHidden');

                $scope.hiddenToggle = !$scope.hiddenToggle;

                //resize the scroll pane if needed
                if (typeof jScrollPaneAPI !== 'undefined') {
                    $timeout(function () {
                        jScrollPaneAPI.reinitialise();
                    }, 0);
                }
            }

            //automatically refresh the activity stream every five minutes
            $interval(function () {
                $scope.refreshStream(true);
            }, 1000 * 60 * 5);

            $scope.toggleActivityStream = function() {
                //open and close activity pane
                $("#activitystream").toggleClass('open');
                $scope.setPaneHeight();
                jScrollPaneAPI = $('#activitystream .scroll').jScrollPane().data('jsp');
            };

            //set window height and reinitialize scroll pane if windows is resized
            $(window).bind('resize', function() {
                // IE fires multiple resize events while you are dragging the browser window which
                // causes it to crash if you try to update the scrollpane on every one. So we need
                // to throttle it to fire a maximum of once every 50 milliseconds...
                if (typeof jScrollPaneAPI !== 'undefined') {
                    if (!throttleTimeout) {
                        throttleTimeout = setTimeout(function() {
                            $scope.setPaneHeight();

                            jScrollPaneAPI.reinitialise();
                            throttleTimeout = null;
                        }, 50);
                    }
                }
            });

            //prevent window scrolling after reaching end of navigation pane 
            $(document).on('mousewheel', '#activitystream .scroll',
                function(e) {
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
    }
});