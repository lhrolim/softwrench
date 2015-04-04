softwrench.controller('HomeController', function($scope, $ionicPopup, $state) {
    $scope.data = {};

    $scope.login = function () {


        $state.go('tab.dash');
//        LoginService.loginUser($scope.data.username, $scope.data.password).success(function(data) {
//            
//        }).error(function(data) {
//            var alertPopup = $ionicPopup.alert({
//                title: 'Login failed!',
//                template: 'Please check your credentials!'
//            });
//        });
    }
})