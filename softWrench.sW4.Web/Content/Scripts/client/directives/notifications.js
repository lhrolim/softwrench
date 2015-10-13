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

            $scope.displayMoreInfo = function (message) {
                if (message.exception) {
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

            $scope.openModal = function () {
                $('#errorModal').modal('show');
                $("#errorModal").draggable();
            };

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
                    } 
                } else {
                    log.error('Unable to create notification, data is missing.');
                }
            });

            $scope.$on('sw_ajaxerror', function (event, data) {
                log.debug(event.name, data);

                if (typeof (data) != 'undefined') {
                    var message = {};

                    message.type = 'error';
                    message.body = data.exceptionMessage;
                    message.exception = data;
                    //message.more = 'yes';

                    //if we have a message
                    if (message.body) {
                        $scope.messages.push(message);

                        //update so the notification will slide in
                        $timeout(function () {
                            message.display = true;
                        }, 0);
                    }
                } else {
                    log.error('Unable to create notification, data is missing.');
                }
            });

            //Init Notifications
            $scope.messages = [];

            //TODO: push test messages
            var message = {};

            message.type = 'error';
            message.body = 'A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond 10.50.100.128:9080.';
            $scope.$emit('sw_notificationmessage', message);

            message = {};
            message.exceptionMessage = "Object reference not set to an instance of an object.";
            message.exceptionType = "System.NullReferenceException";
            message.message = "An error has occurred.";
            message.stackTrace = " at softWrench.sW4.Util.ExceptionUtil.StackTraceLines(Exception e) in C:\git\softwrench\softWrench.sW4\Util\ExceptionUtil.cs:line 33 at softWrench.sW4.Util.ExceptionUtil.FirstStackTraceLine(Exception e) in C:\git\softwrench\softWrench.sW4\Util\ExceptionUtil.cs:line 73 at softWrench.sW4.Util.ExceptionUtil.FirstProjectStackTraceLine(Exception e) in C:\git\softwrench\softWrench.sW4\Util\ExceptionUtil.cs:line 50 at softWrench.sW4.Web.Common.ErrorDto..ctor(Exception rootException) in C:\git\softwrench\softWrench.sW4.Web\Common\ErrorDto.cs:line 29 at softWrench.sW4.Web.Common.GenericExceptionFilter.BuildErrorDto(Exception e) in C:\git\softwrench\softWrench.sW4.Web\Common\GenericExceptionFilter.cs:line 44 at softWrench.sW4.Web.Common.GenericExceptionFilter.OnException(HttpActionExecutedContext context) in C:\git\softwrench\softWrench.sW4.Web\Common\GenericExceptionFilter.cs:line 51 at System.Web.Http.Tracing.Tracers.ExceptionFilterAttributeTracer.<>c__DisplayClass4.<OnException>b__1() at System.Web.Http.Tracing.ITraceWriterExtensions.TraceBeginEnd(ITraceWriter traceWriter, HttpRequestMessage request, String category, TraceLevel level, String operatorName, String operationName, Action`1 beginTrace, Action execute, Action`1 endTrace, Action`1 errorTrace) at System.Web.Http.Tracing.Tracers.ExceptionFilterAttributeTracer.OnException(HttpActionExecutedContext actionExecutedContext) at System.Web.Http.Filters.ExceptionFilterAttribute.System.Web.Http.Filters.IExceptionFilter.ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken) at System.Web.Http.ApiController.<>c__DisplayClass8.<>c__DisplayClassa.<InvokeActionWithExceptionFilters>b__6(IExceptionFilter filter) at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext() at System.Threading.Tasks.TaskHelpers.IterateImpl(IEnumerator`1 enumerator, CancellationToken cancellationToken)";
            $scope.$emit('sw_ajaxerror', message);
        }
    }
});
