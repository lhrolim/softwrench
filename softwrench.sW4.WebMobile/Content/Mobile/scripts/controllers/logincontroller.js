softwrench.controller('LoginController', function($scope, $ionicPopup, $state,loginService) {
    $scope.data = {};

    $scope.login = function () {
        
        loginService.login($scope.data.username, $scope.data.password).success(function(data) {
            $state.go('main.home');
        }).error(function(data) {
            var alertPopup = $ionicPopup.alert({
                title: 'Login failed!',
                template: 'Please check your credentials!'
            });
        });
    }


    $scope.settings = function () {
        $state.go('settings');
    }
})