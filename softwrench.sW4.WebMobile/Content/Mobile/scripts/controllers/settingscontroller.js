softwrench.controller('SettingsController', function ($scope, $state, swdbDAO, contextService) {

    function init() {
        //at this point was already loaded from swdb
        var settings = contextService.fetchFromContext("settings", true);
        $scope.settings = settings;
    }



    $scope.save = function () {
        swdbDAO.save($scope.settings);
        contextService.insertIntoContext("settings", $scope.settings);
        $state.go("login");
    }

    init();

})