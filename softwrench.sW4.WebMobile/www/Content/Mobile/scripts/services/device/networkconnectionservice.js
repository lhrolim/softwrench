(function (mobileServices) {
    "use strict";

    /**
     * Wrapper around $cordovaNetwork so it doesn't call cordova 
     * related stuff in simulated environment.
     */
    function networkConnectionService($cordovaNetwork) {
        //#region Utils
        var simulator = isRippleEmulator();
        //#endregion

        //#region Public methods
        function isOnline() {
            if (simulator) {
                return navigator.onLine;
            }
            return $cordovaNetwork.isOnline();
        }

        function isOffline() {
            if (simulator) {
                if (sessionStorage.mockoffline === "true") {
                    return true;
                }

                return !navigator.onLine;
            }
            return $cordovaNetwork.isOffline();
        }
        //#endregion

        //#region Service instance
        const service = {
            isOnline,
            isOffline
        };
        return service;
        //#endregion
    }

    //#region Service registration
    mobileServices.factory("networkConnectionService", ["$cordovaNetwork", networkConnectionService]);
    //#endregion

})(mobileServices);