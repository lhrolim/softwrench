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

                return classText;
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

                data.display = true;
                $scope.messages.push(data);
       
                //add automatic timeout for success messages
                if (data.type == 'success') {
                    $timeout(function () {
                        $scope.removeMessage(data);
                        //TODO: test message timeouts
                    }, contextService.retrieveFromContext('successMessageTimeOut'));
                }
            });

            //Init Notifications
            $scope.messages = [];

            //TODO: push test messages
            var message = {};

            message.type = 'dev';
            message.body = '23 JavaScript Errors found.';
            message.more = 'yes';
            //$scope.$emit('sw_notificationmessage', message);

            message = {};
            message.body = 'A connection attempt failed because the connected party did not properly respond after a period of time.';
            message.more = 'yes';
            //$scope.$emit('sw_notificationmessage', message);

            message = {};
            message.type = 'success';
            message.body = 'Service Request 1718 successfully updated.';
            $scope.$emit('sw_notificationmessage', message);

            message = {};
            message.type = 'error';
            message.body = 'A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond 10.50.100.128:9080.';
            message.more = 'yes';
            $scope.$emit('sw_notificationmessage', message);

            $scope.addTestMessage = function () {
                var message = {};
                message.type = 'dev';
                message.body = moment().format();;
                message.more = 'yes';
                $scope.$emit('sw_notificationmessage', message);
            }

            log.debug($scope.messages);
        }
    }
});
