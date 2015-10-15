var app = angular.module('sw_layout');

app.directive('notifications', function (contextService, $log) {
    var log = $log.getInstance('sw4.notifications');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/notifications.html'),
        controller: function ($scope, $timeout) {
            $scope.removeMessage = function (message) {
                /// <summary>
                /// Hide the user selected message when the close button is clicked
                /// </summary>
                /// <param name="message" type="object">Notification message</param>
                /// <returns></returns>
                log.debug('removeMessage', message);
                message.display = false;
            }

            $scope.displayMoreInfo = function (message) {
                /// <summary>
                /// Determine is the more info button/link should be displayed
                /// </summary>
                /// <param name="message" type="object">Notification message</param>
                /// <returns></returns>
                return message.exception;
            }

            $scope.getIconClass = function (type) {
                /// <summary>
                /// Determine the correct icon to display
                /// </summary>
                /// <param name="type" type="string">Notification message type</param>
                /// <returns></returns>
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
                /// <summary>
                /// Add the notification message type as a class to the DOM element
                /// </summary>
                /// <param name="type" type="string">Notification message type</param>
                /// <returns></returns>
                return type || '';
            }

            $scope.getTitleText = function (title, type) {
                /// <summary>
                /// Determine the correct title to display
                /// </summary>
                /// <param name="title" type="string">Custom title to be used</param>
                /// <param name="type" type="string">Notification message type</param>
                /// <returns></returns>
                if (title) {
                    return title;
                }
                
                switch (type) {
                    case 'error':
                        return 'Sorry...';
                    case 'success':
                        return 'Success...';
                    default:
                        return 'Just to let you know...';
                }
            }

            $scope.openModal = function (message) {
                /// <summary>
                /// Display the exception data modal when more info is clicked
                /// </summary>
                /// <param name="message" type="object">Notification message</param>
                /// <returns></returns>
                $scope.moreInfo = message.exception;
                $scope.moreInfo.title = message.body;
                $scope.moreInfo.text = ('Error description:\n\n' +
                        'Type: \n{0}\n\n' +
                        'Message: \n{1}\n\n' +
                        'Outline:\n{2}\n\n' +
                        'StackTrace:\n{3}\n\n')
                    .format($scope.moreInfo.type, $scope.moreInfo.title, $scope.moreInfo.outline, $scope.moreInfo.stack);

                $('#errorModal').modal('show');
                $('#errorModal').draggable();
            };

            //Event Handlers
            $scope.$on('sw_notificationmessage', function (event, data) {
                /// <summary>
                /// Receive message data and crete user notification
                /// </summary>
                /// <param name="event" type="object">Angular event data</param>
                /// <param name="data" type="string">The string will be used as the message body</param>
                /// <param name="data" type="object">The object allows more control of the notification, but must be formatted as follows:
                /// data.type: Optional, [dev, error, info (default), null, success]
                /// data.title: Optional, if no value the default title will be used based on the data.type. For consistency the default title should be used.
                /// data.body: Required
                /// data.exception: Optional, when present will display the more info button, the exception is formatted as follow
                /// data.exception.type: Optional
                /// data.exception.outline: Optional
                /// data.exception.stack: Optional
                /// </param>
                /// <returns></returns>
                log.debug(event.name, data);

                //make sure some type of message exists
                if (typeof (data) == 'undefined') {
                    log.error('Unable to create notification, data is missing.');
                    return;
                }

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
                    if (message.type === 'success' && !message.exception) {
                        $timeout(function() {
                            $scope.removeMessage(message);
                        }, contextService.retrieveFromContext('successMessageTimeOut'));
                    }
                } 
            });

            //Init Notifications
            $scope.messages = [];

            //TODO: push test messages
            //var message = {};
            //message.type = 'error';
            //message.body = 'A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond 10.50.100.128:9080.';
            //var exception = {};
            //exception.type = 'System.NullReferenceException';
            //exception.outline = 'Some ountline info...';
            //exception.stack = 'Some stack trace text...';
            //message.exception = exception;
            //$scope.$emit('sw_notificationmessage', message);
        }
    }
});
