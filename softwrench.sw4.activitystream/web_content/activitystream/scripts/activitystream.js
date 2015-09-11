var app = angular.module('sw_layout');

app.directive('activitystream', function(contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Shared/activitystream/templates/activitystream.html'),
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
            $scope.enableFilter = false;
     
            $scope.activityStreamEnabled = function () {
                return contextService.fetchFromContext('activityStreamFlag', false, true);
            };

            if ($scope.activityStreamEnabled()) {
                $('html').addClass('activitystream');
            }

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
            }

            $scope.deciveType = function () {
                return DeviceDetect.catagory.toLowerCase();
            }

            $scope.formatDate = function(notificationDate) {
                var currentDate = new Date();
                var nowMils = currentDate.getTime();

                var notificationMils = new Date(notificationDate).getTime();
                var differenceMils = nowMils - notificationMils;
                var dateMessage = moment.duration(differenceMils, "milliseconds").humanize();

                return 'About ' + dateMessage + ' ago'; // + ' (' + notificationMils + ')';
            };

            $scope.getAllHidden = function () {
                log.debug('getAllHidden');

                if ($scope.activities.length > 0) {
                    if ($scope.activities.length === $scope.readCount) {
                        return true;
                    }
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
                    parameters.role = contextService.getUserData().roles[0].label;

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
                parameters.role = contextService.getUserData().roles[0].label;
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
                parameters.role = contextService.getUserData().roles[0].label;
                parameters.application = activity.application;
                parameters.id = activity.id;
                parameters.rowstamp = activity.rowstamp;

                var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                $http.post(rawUrl).success(
                    function (data) {

                        //if the header is not fixed (mobile), hide the actity pane
                        if ($('.site-header').css('position') != 'fixed') {
                            $scope.toggleActivityStream();
                        }

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
                parameters.role = 'default';

                var rawUrl = url("/api/generic/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));
                $http.get(rawUrl, { avoidspin: true }).success(
                    function (data) {
                        $scope.readCount = data.readCount;
                        $scope.activities = data.notifications;
                        $scope.refreshRate = data.refreshRate;
                        $scope.statusAllHidden = $scope.getAllHidden();

                        //TODO: remove for production
                        //$scope.activities = demoNotifications;

                        //resize the scroll pane if needed
                        if (typeof jScrollPaneAPI !== 'undefined') {
                            jScrollPaneAPI.reinitialise();
                        }

                        //restore spinner to orginal value
                        log.debug($scope.activities);
                    }).error(
                    function(data) {
                        var errordata = {
                            errorMessage: "error opening action {0} of controller {1} ".format(actionToUse, controllerToUse),
                            errorStack: data.message
                        }
                        
                        //restore spinner to orginal value
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

            $scope.toggleFilter = function () {
                log.debug('toggleFilter');

                $scope.enableFilter = !$scope.enableFilter;
                $scope.clearFilter();
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

            $scope.toggleActivityStream = function () {
                //open and close activity pane
                $('#activitystream').toggleClass('open');

                //resize/position elements
                $(window).trigger('resize');

                //reclac the activity pane height
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
            $(document).on('mousewheel', '#activitystream .scroll', function (e) {
                var delta = e.originalEvent.wheelDelta;
                this.scrollTop += (delta < 0 ? 1 : -1) * 30;
                e.preventDefault();
            });

            $scope.$watch('filterText', function () {
                $(window).trigger('resize');
            });

            //get the current notifications, then automatically refresh
            var refreshLoop = function () {
                log.debug('refreshLoop', $scope.refreshRate);

                var refreshTimeout;

                if (typeof $scope.refreshRate == 'undefined' || $scope.refreshRate == 0) {
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
            //$timeout(function () {
            //    $('#activitystream .handle').trigger('click');
            //}, 0);

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

            //newNotification = {};
            //newNotification.application = "servicerequest";
            //newNotification.createBy = "RKLIBERT";
            //newNotification.flag = "changed";
            //newNotification.icon = "fa-ticket";
            //newNotification.id = "79788";
            //newNotification.isRead = false;
            //newNotification.label = "service request";
            //newNotification.notificationDate = "2015-09-11T09:38:34";
            //newNotification.parentApplication = null;
            //newNotification.parentId = null;
            //newNotification.parentLabel = null;
            //newNotification.parentUId = -1;
            //newNotification.rowstamp = 185625112;
            //newNotification.summary = "DWLRR7 User on Rowan Rentless rig";
            //newNotification.targetSchema = "editdetail";
            //newNotification.uId = 62671;
            //demoNotifications.push(newNotification);
        }
    }
});

$(window).resize(function () {
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
});
