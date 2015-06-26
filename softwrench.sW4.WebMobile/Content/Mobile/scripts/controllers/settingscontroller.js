softwrench.controller('SettingsController', function ($scope, routeService, swdbDAO, contextService) {

    function init() {
        //at this point was already loaded from swdb
        var settings = contextService.fetchFromContext("settings", true);
        $scope.settings = settings;
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