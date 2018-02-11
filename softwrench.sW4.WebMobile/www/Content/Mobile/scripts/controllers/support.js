﻿(function (angular) {
    "use strict";

    angular.module("softwrench")
        .controller("SupportController", ["$scope", "supportService", "$ionicPopup", "dynamicScriptsCacheService", "securityService", "routeService", "swdbDAO", "$log", "$roll", function ($scope, supportService, $ionicPopup, dynamicScriptsCacheService, securityService, routeService, swdbDAO, $log, $roll) {

            const log = $log.get("SupportController");

            $scope.vm = {
                loglevel: sessionStorage["loglevel"] || "WARN",
                sqlClientCollapsed: true,
                logFileCollapsed:true
            }

            $scope.toggleSQLClientState = function() {
                $scope.vm.sqlClientCollapsed = !$scope.vm.sqlClientCollapsed;
            }

            $scope.toggleLogFile = function () {
                $scope.vm.logFileCollapsed = !$scope.vm.logFileCollapsed;
                if (!$scope.vm.logFileCollapsed) {
                    $roll.readCurrent().then(filecontent => {
                        $scope.vm.logoutput = filecontent;
                    });
                }
            }

            $scope.changeloglevel = function (callback) {
                sessionStorage.loglevel = callback.item.value;
                if (callback.item.value.equalIc('debug')) {
                    persistence.debug = true;
                    $scope.vm.logsql = true;
                } else {
                    persistence.debug = false;
                }
            }

            $scope.changeLogSQl = function (){
                persistence.debug = $scope.vm.logsql;
            }

            $scope.sendLogFiles = function() {
                supportService.getLogReportingModal(false).then(modal => {
                    modal.show();
                });
            };

            $scope.reset = function () {
                const confirm = $ionicPopup.confirm({
                    title: "Reset configurations",
                    template: `Are you sure that you want to reset configurations? All non synchronized work will be lost`
                });
                return confirm.then(res => {
                    dynamicScriptsCacheService.clearEntries();
                    window.location.reload(true);
                    securityService.logout(true).then(() => {
                        return routeService.go("login");
                    }).then(l => {

                    });
                });
                supportService.requestLogReporting();
            };

            $scope.queryModel = {};
            $scope.queryModel.sqlQuery = "";
            $scope.queryModel.limitRows = true;
            $scope.queryModel.limitRowsSize = 10;
            $scope.queryModel.resultMessage = "";
            $scope.queryModel.resultMessageStyle = { color: "green" };

            const formatQuerySuccess = function(query, result) {
                let msg = "";
                msg += `Query (${query}) run with success`;
                if (!result || !result.length) {
                    msg += ".";
                    return msg;
                }

                msg += ":\n";
                angular.forEach(result, (row) => {
                    msg += JSON.stringify(row) + "\n";
                });
                return msg;
            }

            const formatQueryError = function (query, err) {
                let msg = "";
                msg += `Query (${query}) run with error.`;
                return msg;
            }

            $scope.runSQL = function () {
                if (!$scope.queryModel.sqlQuery) {
                    return;
                }

                const query = $scope.queryModel.sqlQuery.trim();
                swdbDAO.executeQuery(query).then((result) => {
                    $scope.queryModel.resultMessageStyle.color = "green";
                    if (query.toLowerCase().startsWith("select")) {
                        $scope.queryModel.resultMessage = `${result.length} results found.`;
                    } else {
                        $scope.queryModel.resultMessage = "Success.";
                    }
                    log.warn(formatQuerySuccess(query, result));
                }, (err) => {
                    $scope.queryModel.resultMessageStyle.color = "red";
                    $scope.queryModel.resultMessage = `Error: ${err}.`;
                    log.error(formatQueryError(query, err));
                });
            }
        }]);

})(angular);