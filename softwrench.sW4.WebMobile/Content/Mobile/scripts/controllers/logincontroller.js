softwrench.controller('LoginController', function($scope, $ionicPopup, routeService, loginService) {
    $scope.data = {};

    $scope.login = function () {
        
        loginService.login($scope.data.username, $scope.data.password).then(function(data) {
            routeService.go('main.home');
        }).catch(function(data) {
            var alertPopup = $ionicPopup.alert({
                title: 'Login failed!',
                template: 'Please check your credentials!'
            });
        });
    }


    $scope.settings = function () {
        routeService.go("settings");
    }
})