
(function (angular) {
    'use strict';

    angular.module('sw_layout').factory('notificationViewModel', ['contextService', '$timeout', '$log', notificationViewModel]);
    function notificationViewModel(contextService, $timeout, $log) {
        var log = $log.getInstance('sw4.notificationViewModel');

        //#region Utils
        var vm = {
            messages: []
        };

        //#endregion
        //#region Private Methods

        function getMessages() {
            return vm.messages;
        }

        return {
            /// <summary>
            /// Receive message data and crete user notification
            /// </summary>
            /// <param name="type" type="string">Optional, [dev, error, info (default), null, success]</param>
            /// <param name="title" type="string">Optional, if no value the default title will be used based on the data.type. For consistency the default title should be used</param>
            /// <param name="body" type="string">Required, message text to display</param>
            /// <param name="exceptionType" type="string">Optional, display in more info modal</param>
            /// <param name="exceptionOutline" type="string">Optional, display in more info modal</param>
            /// <param name="exceptionStack" type="string">Optional, display in more info modal</param>
            /// <returns></returns>
            createNotification: function (type, title, body, exceptionType, exceptionOutline, exceptionStack) {

                //build the message object
                var message = {};
                message.type = type;
                message.title = title;
                message.body = body;
                message.display = true;

                //if any exception info is present, create the exception object
                if (exceptionType || exceptionOutline || exceptionStack) {
                    var exception = {};
                    exception.type = exceptionType;
                    exception.outline = exceptionOutline;
                    exception.stack = exceptionStack;
                    message.exception = exception;
                }

                //broad the notification event
                if (message.body) {
                    log.debug('createNotification', message);

                    $timeout(function() {
                        vm.messages.push(message);
                    }, 0);

                    //add automatic timeout for success messages
                    if (message.type === 'success' && !message.exception) {
                        $timeout(function() {
                            //$scope.removeMessage(message);
                            message.display = false;
                        }, contextService.retrieveFromContext('successMessageTimeOut'));
                    }
                }
            },

            /// <summary>
            /// Return the data for the more info modal
            /// </summary>
            /// <param name="message" type="object">Notification message</param>
            /// <returns></returns>
            getMoreInfo: function(message) {
                //setup more info temporary store
                var moreInfo = message.exception;
                moreInfo.title = message.body;
                moreInfo.text = ('Error description:\n\n' +
                    'Type: \n{0}\n\n' +
                    'Message: \n{1}\n\n' +
                    'Outline:\n{2}\n\n' +
                    'StackTrace:\n{3}\n\n')
                .format(moreInfo.type, moreInfo.title, moreInfo.outline, moreInfo.stack);

                return moreInfo;
            },

            /// <summary>
            /// Hide the user selected message
            /// </summary>
            /// <param name="message" type="object">Notification message</param>
            /// <returns></returns>
            removeNotification: function (message) {
                log.debug('removeMessage', message);
                message.display = false;
            },

            messages: getMessages()
        }

        //#endregion
    }
})(angular);
