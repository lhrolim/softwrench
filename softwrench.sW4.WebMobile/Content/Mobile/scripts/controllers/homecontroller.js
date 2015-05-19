/// <reference path="maincontroller.js" />
softwrench.controller('HomeController', function ($scope, synchronizationFacade, $ionicPopup, $ionicLoading) {
    $scope.data = {};

    $scope.fullSynchronize = function () {

        $ionicLoading.show({
            content: '<i class="icon ion-looping"></i> Loading',
            animation: 'fade-in',
            showBackdrop: true,
            maxWidth: 200,
            showDelay: 10
        });
        var promise = synchronizationFacade.fullSync();
        promise.then(function (message) {
            $ionicPopup.alert({
                title: "Synchronization Suceeded",
                template: message
            });
        }).catch(function () {
            $ionicPopup.alert({
                title: "Error Synchronizing Data",
            });
        }).finally(function (message) {
            $ionicLoading.hide();
        });


    }

})