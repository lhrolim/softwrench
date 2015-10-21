var app = angular.module('sw_layout');

app.directive('notifications', function (contextService, notificationViewModel, $log) {
    var log = $log.getInstance('sw4.notifications');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/notifications.html'),
        controller: function ($scope, $timeout) {
            $scope.removeMessage = function (message) {
                notificationViewModel.removeNotification(message);
            }

            /// <summary>
            /// Determine is the more info button/link should be displayed
            /// </summary>
            /// <param name="message" type="object">Notification message</param>
            /// <returns></returns>
            $scope.displayMoreInfo = function (message) {
                return message.exception;
            }

            /// <summary>
            /// Determine the correct icon to display
            /// </summary>
            /// <param name="type" type="string">Notification message type</param>
            /// <returns></returns>
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

            $scope.getMessages = function () {
                return notificationViewModel.messages;
            }

            /// <summary>
            /// Add the notification message type as a class to the DOM element
            /// </summary>
            /// <param name="type" type="string">Notification message type</param>
            /// <returns></returns>
            $scope.getMessageClass = function (type) {
                return type || '';
            }

            /// <summary>
            /// Determine the correct title to display
            /// </summary>
            /// <param name="title" type="string">Custom title to be used</param>
            /// <param name="type" type="string">Notification message type</param>
            /// <returns></returns>
            $scope.getTitleText = function (title, type) {
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
                $scope.moreInfo = notificationViewModel.getMoreInfo(message);

                $('#errorModal').modal('show');
                $('#errorModal').draggable();
            };

            //push test messages
            //notificationViewModel.createNotification('success', null, 'A connection attempt failed because the connected party did not properly respond after a period of time.');
            //notificationViewModel.createNotification('error', null, 'A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond 10.50.100.128:9080.', 'System.NullReferenceException', 'Some ountline info...', 'Some stack trace text...');
        }
    }
});
