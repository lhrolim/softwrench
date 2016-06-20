(function (mobileServices, ionic) {
    "use strict";

    function loadingService($ionicLoading, $q, $timeout) {
        //#region Utils

        const loadingOptions = {
            //template: "<i class='icon ion-looping'></i> Loading", -> ionicon-animations not added; using spinner instead
            template: "<ion-spinner icon='spiral'></ion-spinner><br><span>Loading<span>",
            animation: "fade-in",
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
        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("loadingService", ["$ionicLoading", "$q", "$timeout", loadingService]);

    //#endregion

})(mobileServices, ionic);