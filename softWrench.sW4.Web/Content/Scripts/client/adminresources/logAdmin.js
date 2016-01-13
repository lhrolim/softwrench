(function (angular) {
    "use strict";

angular.module("sw_layout").controller("LogAdminController", LogAdminController);
function LogAdminController($scope, $http, i18NService, redirectService) {
    "ngInject";

    $scope.filter = function (data) {
        var logname = data.logname;
        $scope.logs = $.grep($scope.initiallogs, function (element) {
            return element.name.toLowerCase().indexOf(logname.toLocaleLowerCase()) != -1;
        });
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.changeLevel = function (logName, level) {
        var parameters = {
            logName: logName,
            newLevel: level,
            pattern: ''
        };
        var urlToInvoke = redirectService.getActionUrl('LogAdmin', 'ChangeLevel', parameters);
        $http.post(urlToInvoke).success(function (data) {
            init(data.resultObject);
        });
    };

    $scope.changeAll = function (level) {
        var parameters = {
            pattern: $scope.logname,
            newLevel: level,
            logName: ''
        };
        var urlToInvoke = redirectService.getActionUrl('LogAdmin', 'ChangeLevel', parameters);
        $http.post(urlToInvoke).success(function (data) {
            init(data.resultObject);
        });
    };

    $scope.viewAppenderContent = function (selectedappender) {
        $scope.selectedappender = selectedappender;
        var parameters = {
            value: selectedappender.value
        };
        var urlToInvoke = redirectService.getActionUrl('LogAdmin', 'GetAppenderTxtContent', parameters);
        $http.get(urlToInvoke).
        success(function (data, status, headers, config) {
            $scope.appendercontent = data.resultObject;
        }).
        error(function (data, status, headers, config) {
            $scope.appendercontent = "Error " + status;
        });
    };

    $scope.downloadFile = function (selectedappender) {
        var parameters = {};
        parameters.fileName = selectedappender.name + ".txt";
        parameters.contentType = 'text/plain';
        parameters.path = selectedappender.value;
        parameters.setFileNameWithDate = true;
        window.location = removeEncoding(url("/Application/DownloadFile" + "?" + $.param(parameters)));
    };

    function init(data) {
        var logs = data.logs;
        var appenders = data.appenders;
        $scope.logs = logs;
        $scope.appenders = appenders;
        $scope.initiallogs = logs;
        $scope.logname = "";
        $scope.chooselog = 'changeviewlog';
        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("LogAdmin.html") != -1) {
                init($scope.resultData);
            }
        });
    };

    $scope.setDefaultAppender = function () {
        if (nullOrUndef($scope.selectedappender)) {
            for (var i = 0; i < $scope.appenders.length; i++) {
                if ($scope.appenders[i].name == 'MaximoAppender') {
                    $scope.selectedappender = $scope.appenders[i];
                    $scope.viewAppenderContent($scope.selectedappender);
                    break;
                }
            }
        }
    };

    init($scope.resultData);
}

window.LogAdminController = LogAdminController;

})(angular);