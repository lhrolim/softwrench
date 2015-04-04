softwrench.controller('MainController', function($scope, $ionicPopup, $state) {
    $scope.data = {};

    $scope.leftButtons = [{
        type: 'button-icon icon ion-navicon',
        tap: function (e) {
            $scope.toggleMenu();
        }
    }];

   
})