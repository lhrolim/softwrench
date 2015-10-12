var app = angular.module('sw_layout');

app.directive('notifications', function (contextService, $log) {
    var log = $log.getInstance('sw4.notifications');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/notifications.html'),
        controller: function ($scope, $timeout) {
            $scope.removeMessage = function (message) {
                log.debug('removeMessage', message);

                message.display = false;
            }

            $scope.displayMoreInfo = function (more) {

                //TODO: determine how more info will work. Pass callback, command, url?

                if (more) {
                    return true;
                } else {
                    return false;
                }
            }

            $scope.getIconClass = function (type) {
                var classText = 'fa ';

                switch (type) {
                    case 'error':
                        classText += 'fa-times-circle';
                        break;
                    case 'success':
                        classText += 'fa-check-circle';
                        break;
                    default:
                        classText += 'fa-info-circle';
                }

                return classText;
            }

            $scope.getMessageClass = function (type) {
                var classText = '';

                if (type != 'undefined') {
                    classText = type;
                } else {
                    classText = '';
                }

                return classText + ' show';
            }

            $scope.getTitleText = function (title, type) {
                var titleText = '';

                if (title) {
                    titleText = title;
                } else {
                    switch (type) {
                        case 'error':
                            titleText += 'Sorry...';
                            break;
                        case 'success':
                            titleText += 'Success...';
                            break;
                        default:
                            titleText += 'Just to let you know...';
                    }
                }

                return titleText;
            }

            //Event Handlers
            $scope.$on('sw_notificationmessage', function (event, data) {
                log.debug(event.name, data);

                //make sure some type of message exists
                if (typeof (data) != 'undefined') {
                    var message = {};

                    //convert simple message to object
                    if (typeof (data) == 'object') {
                        message = data;
                    } else {
                        message.body = data;
                    }

                    //if we have a message
                    if (message.body) {
                        $scope.messages.push(message);

                        //update so the notification will slide in
                        $timeout(function() {
                            message.display = true;
                        }, 0);

                        //add automatic timeout for success messages
                        if (message.type == 'success') {
                            $timeout(function() {
                                $scope.removeMessage(message);
                            }, contextService.retrieveFromContext('successMessageTimeOut'));
                        }
                    } else {
                        log.error('Unable to create notification, message body is missing.');
                    }
                } else {
                    log.error('Unable to create notification, message body is missing.');
                }
            });

            //Init Notifications
            $scope.messages = [];

            //TODO: push test messages
            var message = {};

            message = {};
            message.type = 'error';
            message.body = 'A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond 10.50.100.128:9080.';
            message.more = 'yes';
            $scope.$emit('sw_notificationmessage', message);

            $scope.addTestMessage = function () {
                var message = {};
                message.type = 'success';
                message.body = moment().format() + ' A connection attempt failed because the connected party did not properly respond after a period of time.';
                message.more = 'yes';
                $scope.$emit('sw_notificationmessage', message);
            }

            //$timeout(function () {
                $scope.addTestMessage();
            //}, 3000);

            log.debug($scope.messages);
        }
    }
});
