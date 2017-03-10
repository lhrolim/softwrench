(function (mobileServices) {
    "use strict";

    class scriptsSynchronizationService {

        constructor(restService, $q,$log, localStorageService, dynamicScriptsCacheService, $cordovaAppVersion) {
            this.restService = restService;
            this.$q = $q;
            this.$log = $log;
            this.localStorageService = localStorageService;
            this.dynamicScriptsCacheService = dynamicScriptsCacheService;
            this.$cordovaAppVersion = $cordovaAppVersion;
        }

        syncData() {

            const clientState = this.dynamicScriptsCacheService.getClientState();

            const clientVersionPromise = isRippleEmulator() ? this.$q.when("ripple") : this.$cordovaAppVersion.getVersionNumber();
            const offlineDevice = ionic.Platform.isAndroid() ? "android" : "ios";

            return clientVersionPromise.then(offlineVersion => {
                const data = {
                    clientState,
                    offlineDevice,
                    offlineVersion
                };
                return this.restService.post("Mobile", "BuildSyncMap", {}, data);
            }).then(scriptsResult => {
                return this.dynamicScriptsCacheService.syncWithServerSideScripts(scriptsResult.data);
            }).catch (err => {
                this.$log.warn("[dynamic-scripts]incompatible server side version");
                return this.$q.when();
            });
        }

    }

    scriptsSynchronizationService.$inject = ["offlineRestService", "$q","$log", "localStorageService", "dynamicScriptsCacheService", "$cordovaAppVersion"];


    mobileServices.service("scriptsSynchronizationService", scriptsSynchronizationService);


})(mobileServices);