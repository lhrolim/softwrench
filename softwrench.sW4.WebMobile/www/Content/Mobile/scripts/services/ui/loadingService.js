(function (mobileServices, ionic) {
    "use strict";

    function loadingService($ionicLoading, $q, $timeout, $ionicPlatform) {
        //#region Utils

        const loadingOptions = {
            template: "<ion-spinner icon='spiral'></ion-spinner><br><span>Loading<span>",
            animation: "fade-in"
        };

        var deregisterFn;


        //#endregion

        //#region Public methods

        function showDefault() {
            deregisterFn = $ionicPlatform.registerBackButtonAction(e=> {
                this.hide();
            }, 501);
            return $ionicLoading.show(loadingOptions);
        }

        function hide() {
            if (deregisterFn) {
                deregisterFn();
            }

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

      

        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("loadingService", ["$ionicLoading", "$q", "$timeout", "$ionicPlatform", loadingService]);

    //#endregion

})(mobileServices, ionic);