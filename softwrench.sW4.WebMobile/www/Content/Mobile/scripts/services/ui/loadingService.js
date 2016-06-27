(function (mobileServices, ionic) {
    "use strict";

    function loadingService($ionicLoading, $q, $timeout, $ionicPlatform) {
        //#region Utils

        const loadingOptions = {
            template: "<ion-spinner icon='spiral'></ion-spinner><br><span>Loading<span>",
            animation: "fade-in"
        };

        //#endregion

        //#region Public methods

        function showDefault() {
            return $ionicLoading.show(loadingOptions);
        }

        function hide() {
            ionic.requestAnimationFrame(() => {
                $ionicLoading.hide();
            });
        }

        //#endregion

        //#region Service Instance
        const service = {
            showDefault,
            hide

        };

        $ionicPlatform.registerBackButtonAction(e=> {
            service.hide();
        }, 501);

        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("loadingService", ["$ionicLoading", "$q", "$timeout", "$ionicPlatform", loadingService]);

    //#endregion

})(mobileServices, ionic);