/// <reference path="maincontroller.js" />
softwrench.controller('HomeController', function ($scope, synchronizationFacade, $ionicPopup) {
    $scope.data = {};

    $scope.fullSynchronize = function () {

        var promise = synchronizationFacade.fullSync();
        promise.then(function(message) {
            $ionicPopup.alert({
                title: message,
                template: message
            });
        });



    }

//    $scope.leftButtons = [{
//        type: 'button-icon icon ion-navicon',
//        tap: function (e) {
//            $scope.toggleMenu();
//        }
//    }];
})