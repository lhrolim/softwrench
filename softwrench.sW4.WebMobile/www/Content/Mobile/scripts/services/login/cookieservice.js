(function (angular) {
    "use strict";

    function cookieService($log, $q, dao, settingsService, entities) {
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
                this.cookieMaster.setCookieValue(url, name, value, () => deferred.resolve(value), error => deferred.reject(error));
                return deferred.promise;
            }
            clearCookies() {
                const deferred = this.$q.defer();
                this.cookieMaster.clear(() => deferred.resolve(), error => deferred.reject(error));
                return deferred.promise;
            }
        }
        //#endregion

        const $cookie = new CordovaCookieMasterAdapter(window.cookieMaster, $q);

        /**
         * Fetches the value of a cookie that is persisted locally.
         * 
         * @param {String} name 
         * @returns {String} 
         */
        function getCookie(name) {
            return dao.findSingleByQuery("Cookie", `name='${name}'`).then(c => !c ? null : c.value);
        }

        /**
         * Fetches the value of a cookie available in the webview by name.
         * 
         * @param {String} name 
         * @returns Promise<String> 
         */
        function getWebCookie(name) {
            return settingsService.getServerUrl().then(url => $cookie.getCookieValue(url, name));
        }

        /**
         * Sets the value of a cookie in the webview.
         * 
         * @param {String} name 
         * @param {String} value 
         * @returns Promise<String> resolved with the value 
         */
        function setWebCookie(name, value) {
            return settingsService.getServerUrl().then(url => $cookie.setCookieValue(url, name, value));
        }

        function rippleProxy(method) {
            return function() {
                return isRippleEmulator() ? $q.when() : method.apply(null, arguments);
            }
        }

        //#endregion

        //#region Public methods

        /**
         * Clears all cookies from the webview and persisted locally.
         * 
         * @returns {Promise<Void>} 
         */
        function clearCookies() {
            return $cookie.clearCookies().then(() => dao.executeStatement(entities.Cookie.deleteAllStatement));
        }

        /**
         * Fetches cookie from webview and persists it locally.
         * 
         * @param {string} name 
         * @returns {Promise<String>} resolved with the cookie value 
         */
        function persistCookie(name) {
            return $q.all([
                dao.countByQuery("Cookie", `name = '${name}'`),
                getWebCookie(name)
            ]).spread((count, value) => {
                const promise = count <= 0
                    ? dao.executeStatement(entities.Cookie.insertStatement, [persistence.createUUID(), name, value])
                    : dao.executeStatement(entities.Cookie.updateByNameStatement, [value, name]);
                return promise.then(() => {
                    $log.get("cookieService#persistCookie", ["cookie"]).debug(`persisted cookie {name:${name}, value: ${value}}`);
                    return value;
                });
            });
        }

        /**
         * Fetches the value of a cookie persisted locally and sets it back into the webview.
         * 
         * @param {String} name 
         * @returns {Promise<String>} cookie value 
         */
        function restoreCookie(name) {
            const logger = $log.get("cookieService#restoreCookie", ["cookie"]);

            const promise = getCookie(name).then(value => !value ? null : setWebCookie(name, value));

            return logger.isLevelEnabled("debug")
                ? promise.then(value => {
                    logger.debug(`restore cookie {name:${name}, value: ${value}}`);
                    return value;
                })
                : promise;
        }

        //#endregion

        //#region Service Instance
        const service = {
            clearCookies: rippleProxy(clearCookies),
            persistCookie: rippleProxy(persistCookie),
            restoreCookie: rippleProxy(restoreCookie)
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services").factory("cookieService", ["$log", "$q", "swdbDAO", "settingsService", "offlineEntities", cookieService]);

    //#endregion

})(angular);