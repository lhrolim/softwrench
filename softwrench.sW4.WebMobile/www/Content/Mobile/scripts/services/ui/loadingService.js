(function (mobileServices, ionic) {
    "use strict";

    function loadingService($ionicLoading, $q, $timeout, $ionicPlatform) {
        //#region Utils


        var deregisterFn=[];


        //#endregion

        //#region Public methods

        function showDefault() {
            return this.show("Loading");
        }

        function show(message) {
            deregisterFn.push($ionicPlatform.registerBackButtonAction(e=> {
                this.hide();
            }, 501));
            const options = {
                template: `<ion-spinner icon='spiral'></ion-spinner><br><span>${message}<span>`,
                animation: "fade-in"
            };
            return $ionicLoading.show(options);
        }

        function hide() {
            if (deregisterFn) {
                deregisterFn.forEach((deregisterFn) => deregisterFn());
            }

            ionic.requestAnimationFrame(() => {
                $ionicLoading.hide();
            });
        }

        //#endregion

        //#region Service Instance
        const service = {
            showDefault,
            show,
            hide
        };

      

        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("loadingService", ["$ionicLoading", "$q", "$timeout", "$ionicPlatform", loadingService]);

    //#endregion

})(mobileServices, ionic);