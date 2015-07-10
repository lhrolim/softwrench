softwrench.controller('SettingsController', function ($scope, routeService, swdbDAO, contextService) {

    function init() {
        var settings = contextService.fetchFromContext("settings", true);
        if (settings) {
            $scope.settings = settings;
        } else {
            $scope.settings = {};
        }
        
        
    }

    $scope.goToLogin = function () {
        routeService.go("login");
    };

    $scope.save = function () {
        //TODO: handle settings method correctly, appending http, testing, etc...
        //SWML-39

        swdbDAO.instantiate("Settings", $scope.settings).success(function(settingsToSave) {
            swdbDAO.save(settingsToSave);
            contextService.insertIntoContext("settings", $scope.settings);
            contextService.insertIntoContext("serverurl", $scope.settings.serverurl);
            routeService.go("login");
        });
    }

    init();

})