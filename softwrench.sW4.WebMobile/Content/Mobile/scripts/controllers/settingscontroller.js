softwrench.controller('SettingsController', function ($scope, $state, swdbDAO, contextService) {

    function init() {
        //at this point was already loaded from swdb
        var settings = contextService.fetchFromContext("settings", true);
        $scope.settings = settings;
    }



    $scope.save = function () {
        //TODO: handle settings method correctly, appending http, testing, etc...
        //SWML-39

        swdbDAO.instantiate("Settings", $scope.settings).success(function(settingsToSave) {
            swdbDAO.save(settingsToSave);
            contextService.insertIntoContext("settings", $scope.settings);
            contextService.insertIntoContext("serverurl", $scope.settings.serverurl);
            $state.go("login");
        });
    }

    init();

})