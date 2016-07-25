(function (angular) {
    "use strict";

    function cookieService($q, dao, settingsService) {
        //#region Utils

        //#region cordova-cookie-master plugin adapter promisefying methods
        class CordovaCookieMasterAdapter {
            constructor(cookieMaster, q) {
                this.cookieMaster = cookieMaster;
                this.$q = q;
            }
            getCookieValue(url, name) {
                const deferred = this.$q.defer();
                this.cookieMaster.getCookieValue(url, name, cookie => deferred.resolve(cookie.cookieValue), error => deferred.reject(error));
                return deferred.promise;
            }
            setCookieValue(url, name, value) {
                const deferred = this.$q.defer();
                this.cookieMaster.setCookieValue(url, name, value, () => deferred.resolve(), error => deferred.reject(error));
                return deferred.promise;
            }
            clearCookies() {
                const deferred = this.$q.defer();
                this.cookieMaster.clearCookies(() => deferred.resolve(), error => deferred.reject(error));
                return deferred.promise;
            }
        }
        //#endregion

        const $cookie = new CordovaCookieMasterAdapter(window.cookieMaster, $q);
        const config = {
            cookieName: "swcookie"
        }

        //#endregion

        //#region Public methods

        function getWebCookie() {
            return settingsService.getServerUrl(url => $cookie.getCookieValue(url, config.cookieName));
        }

        function setWebCookie(value) {
            return settingsService.getServerUrl(url => $cookie.setCookieValue(url, config.cookieName, value));
        }

        function clearCookies() {
            return $cookie.clearCookies();
        }

        //#endregion

        //#region Service Instance
        const service = {
            getWebCookie,
            setWebCookie,
            clearCookies
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services").factory("cookieService", ["$q", "swdbDAO", "settingsService", cookieService]);

    //#endregion

})(angular);