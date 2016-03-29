
(function (angular) {
    "use strict";

    function historyService($rootScope, $location, $log) {
        //#region Utils

        // workaround - a way to know if the location was changed by adding a url to the history (causes change in hash)
        // or by browser back and forward
        var locationUpdatedByService = false;

        function getHashBase64() {
            // firefox workaround - there  is some cases that firefox turns the hash into a path
            if (BrowserDetect.browser.toLocaleLowerCase() === "firefox") {
                var path = $location.path();
                if (path && path.startsWith("/state=")) {
                    return path.substring(7);
                }
            }

            var hash = $location.hash();
            if (hash && hash.startsWith("state=")) {
                return hash.substring(6);
            }

            return null;
        }
        //#endregion

        //#region Public methods

        function addToHistory(url) {
            var log = $log.getInstance("historyService#addToHistory");
            log.debug("The url ({0}) is added to history.".format(url));
            var hash = "state=" + Base64.encode(url);
            locationUpdatedByService = true;
            if ($location.hash() === "") {
                $location.replace();
            }
            $location.hash(hash);
        }

        function getLocationUrl() {
            var log = $log.getInstance("historyService#getLocationUrl");

            var base64 = getHashBase64();
            if (!base64) {
                log.debug("No history url on current location.");
                return null;
            }

            var locationUrl = Base64.decode(base64);
            log.debug("The history url ({0}) was recovered from current location.".format(locationUrl));
            return locationUrl;
        }

        function wasLocationUpdatedByService() {
            return locationUpdatedByService;
        }

        function resetLocationUpdatedByService() {
            locationUpdatedByService = false;
        }

        //#endregion

        //#region Service Instance
        var service = {
            addToHistory: addToHistory,
            getLocationUrl: getLocationUrl,
            wasLocationUpdatedByService: wasLocationUpdatedByService,
            resetLocationUpdatedByService: resetLocationUpdatedByService
        };
        return service;
        //#endregion
    }

    //#region Service registration

    modules.webcommons.factory("historyService", ["$rootScope", "$location", "$log", historyService]);

    //#endregion

})(angular);