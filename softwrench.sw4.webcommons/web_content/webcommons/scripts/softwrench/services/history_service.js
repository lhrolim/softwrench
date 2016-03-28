
(function (angular) {
    "use strict";

    function historyService($rootScope, $location) {
        //#region Utils
        //#endregion

        //#region Public methods

        function addToHistory(url) {
            var hash = "state=" + Base64.encode(url);
            if ($location.hash() === "") {
                $location.replace();
            }
            $location.hash(hash);
        }

        function getLocationUrl() {
            if (!$location.hash() || !$location.hash().startsWith("state=")) {
                return null;
            }
            var base64 = $location.hash().substring(6);
            return Base64.decode(base64);
        }

        //#endregion

        //#region Service Instance
        var service = {
            addToHistory: addToHistory,
            getLocationUrl: getLocationUrl
        };
        return service;
        //#endregion
    }

    //#region Service registration

    modules.webcommons.factory("historyService", ["$rootScope", "$location", historyService]);

    //#endregion

})(angular);