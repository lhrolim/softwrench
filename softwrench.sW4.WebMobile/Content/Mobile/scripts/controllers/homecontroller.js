/// <reference path="maincontroller.js" />
softwrench.controller('HomeController', function ($scope, synchronizationFacade, $ionicPopup, $ionicLoading) {
    $scope.data = {};

    $scope.fullSynchronize = function () {

        var spin =$ionicLoading.show({
            content: '<i class="icon ion-looping"></i> Loading',
            animation: 'fade-in',
            showBackdrop: true,
            maxWidth: 200,
            showDelay: 10
        });
        var promise = synchronizationFacade.fullSync();
        promise.then(function (message) {
            spin.hide();
            $ionicPopup.alert({
                title: message,
                template: message
            });
        })
            .catch(function (message) {
                spin.hide();
                $ionicPopup.alert(
                    {
                        title: "Error Synchronizing Data",
                        template: message
                    });
            });





    }

})