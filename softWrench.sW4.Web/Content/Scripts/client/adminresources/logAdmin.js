﻿(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("LogAdminController", LogAdminController);
    function LogAdminController($scope, $http, i18NService, redirectService) {
        "ngInject";
        $scope.logrotation = "0";
        $scope.filenumber = "1";
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
            const parameters = {
                logName: logName,
                newLevel: level,
                pattern: ''
            };
            const urlToInvoke = redirectService.getActionUrl('LogAdmin', 'ChangeLevel', parameters);
            return $http.post(urlToInvoke).then(function (response) {
                init(response.data.resultObject);
            });
        };

        $scope.changeAll = function (level) {
            const parameters = {
                pattern: $scope.logname,
                newLevel: level,
                logName: ''
            };
            const urlToInvoke = redirectService.getActionUrl('LogAdmin', 'ChangeLevel', parameters);
            return $http.post(urlToInvoke).then(function (response) {
                init(response.data.resultObject);
            });
        };

        $scope.viewAppenderContent = function (selectedappender, tempfile) {
            $scope.selectedappender = selectedappender;
            const parameters = {
                value: tempfile === undefined ? selectedappender.value : tempfile
            };
            const urlToInvoke = redirectService.getActionUrl('LogAdmin', 'GetAppenderTxtContent', parameters);
            $http.get(urlToInvoke).
                then(function (response, status, headers, config) {
                    $scope.appendercontent = response.data.resultObject;
                });
        };

        $scope.downloadFile = function (selectedappender, logrotation, filenumber) {
            var filePath = '';

            if (logrotation !== '0') {
                filePath = selectedappender.value + "." + filenumber;
            } else {
                filePath = selectedappender.value;
            }
            const parameters = {};
            parameters.fileName = selectedappender.name + ".txt";
            parameters.contentType = 'text/plain';
            parameters.path = filePath;
            parameters.setFileNameWithDate = true;
            window.location = removeEncoding(url("/Application/DownloadFile" + "?" + $.param(parameters)));
        };

        $scope.downloadZipFile = function (selectedappender) {
            const parameters = {};
            parameters.fileName = selectedappender.name + ".zip";
            parameters.contentType = 'text/plain';
            parameters.path = selectedappender.value;
            parameters.setFileNameWithDate = true;
            window.location = removeEncoding(url("/Application/DownloadLogFilesZipBundle" + "?" + $.param(parameters)));
        };

        function init(data) {
            const logs = data.logs;
            const appenders = data.appenders;
            $scope.logs = logs;
            $scope.appenders = appenders;
            $scope.initiallogs = logs;
            $scope.logname = "";
            $scope.chooselog = 'viewdownloadlog';

            $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
                if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("LogAdmin.html") != -1) {
                    init($scope.resultData);
                }
            });

            $scope.setDefaultAppender();
        };

        $scope.setDefaultAppender = function () {
            const rotation = $scope.logrotation;
            if (nullOrUndef($scope.selectedappender)) {
                for (let i = 0; i < $scope.appenders.length; i++) {
                    if ($scope.appenders[i].name == 'MaximoAppender') {
                        $scope.selectedappender = $scope.appenders[i];
                        $scope.viewAppenderContent($scope.selectedappender);
                        break;
                    }
                }
            }
        };

        $scope.readLogFile = function (logrotation, filenumber) {

            if (nullOrUndef($scope.selectedappender)) {
                for (let i = 0; i < $scope.appenders.length; i++) {
                    if ($scope.appenders[i].name == 'MaximoAppender') {
                        $scope.selectedappender = $scope.appenders[i];
                        $scope.viewAppenderContent($scope.selectedappender);
                        break;
                    }
                }
            } else {
                if (logrotation === '1') {
                    $scope.viewAppenderContent($scope.selectedappender, $scope.selectedappender.value + "." + filenumber);
                } else {
                    $scope.viewAppenderContent($scope.selectedappender);
                }

            }
        };

        init($scope.resultData);
    }

    window.LogAdminController = LogAdminController;

})(angular);