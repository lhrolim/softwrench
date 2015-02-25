﻿function AboutController($scope, $http, $templateCache, i18NService, restService, contextService,alertService) {

    var data = $scope.resultData;
    if (data != null) {
        $scope.aboutData = data;
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.isDev = function () {
        return contextService.isLocal() || contextService.isDev();
    }

    $scope.dochangeClient = function (newclient) {
        if (contextService.isLocal()) {
            restService.invokePost("Configuration", "ChangeClient", { clientName: newclient }, null, function (s) {
                window.location.reload();
            });
            return;
        }

        alertService.confirmMsg('Are u sure you want to change client to {0}'.format(newclient), function() {
            restService.invokePost("Configuration", "ChangeClient", { clientName: newclient }, null, function(s) {
                window.location.reload();
            });
        });


    }

    $scope.restore = function () {
        if (contextService.isLocal()) {
            restService.invokePost("Configuration", "Restore", null, null, function (s) {
                window.location.reload();
            });
            return;
        }

        alertService.confirmMsg('Are u sure you want to restore to default client', function() {
            restService.invokePost("Configuration", "Restore", null, null, function(s) {
                window.location.reload();
            });
        });
    }


    $scope.keyDisabled = function (key) {
        if (!$scope.isDev()) {
            return true;
        }
        return key != 'Client Name';
    }

    $scope.getFormClass = function (key) {
        return key == 'Client Name' ? 'col-sm-5 col-md-6 col-lg-7' : 'col-sm-10';
    }
}