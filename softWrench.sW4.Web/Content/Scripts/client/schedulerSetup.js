(function (angular) {
    "use strict";

angular.module("sw_layout").controller("SchedulerSetupController", SchedulerSetupController);
function SchedulerSetupController($scope, $http, $templateCache, i18NService) {
    "ngInject";

    function toList(data) {
        if (data != null) {
            $scope.listObjects = data;
        }
        switchMode(false);
    };

    function toDetail() {
        switchMode(true);
    };

    $scope.contextPath = function (path) {
        return url(path);
    };

    $scope.edit = function (data) {
        $scope.scheduler = data;
        toDetail();
    };

    $scope.cancel = function () {
        toList(null);
    };

    $scope.pause = function (data) {
        callApi(data, "Pause");
    };

    $scope.schedule = function (data) {
        callApi(data, "Schedule");
    };

    $scope.execute = function (data) {
        callApi(data, "Execute");
    };

    $scope.changeCron = function (data) {
        callApi(data, "ChangeCron");
    };

    function callApi(data, jobCommand) {
        if (data != null) {
            var urlAux = "api/scheduler?name=" + data.name + "&jobCommand=" + jobCommand;
            if (jobCommand == "ChangeCron" && data.cron != null)
                urlAux += "&cron=" + data.cron;

            $http({
                method: "GET",
                url: url(urlAux)
            })
            .then(function (response) {
                const dataAux = response.data;
                toList(dataAux.resultObject);
            })
            .catch(function (response) {
                const dataAux = response.data;
                $scope.title = dataAux || i18NService.get18nValue('general.requestfailed', 'Request failed');
            });
        }
    }

    function switchMode(mode) {
        $scope.isDetail = mode;
        $scope.isList = !mode;
    }

    function initSchedulerSetup() {
        var data = $scope.resultData;
        if (data != null) {
            $scope.listObjects = data;
            toList(null);
        }
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    initSchedulerSetup();
};

window.SchedulerSetupController = SchedulerSetupController;

})(angular);