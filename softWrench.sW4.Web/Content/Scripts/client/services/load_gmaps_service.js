(function (angular) {
    "use strict";

    // Lazy loading of Google Map API
    function loadGoogleMapApi($window, $q, $rootScope, $log) {
        var deferred = $q.defer();
        var log = $log.get("loadGoogleMapApi");

        // Load Google map API script
        function loadScript() {
            log.debug("Loading gmaps api.");
            $rootScope.$broadcast(JavascriptEventConstants.AjaxInit);
            lockCommandBars();
            lockTabs();

            // Use global document since Angular"s $document is weak
            var script = document.createElement("script");
            script.src = "//maps.googleapis.com/maps/api/js?sensor=false&language=en&callback=initMap";

            document.body.appendChild(script);
        }

        function end(error) {
            if (!error) {
                log.debug("Gmaps api loaded.");
                $rootScope.$broadcast(JavascriptEventConstants.AjaxFinished);
            } else {
                log.warn("Fail to load gmaps api.");
                $rootScope.$broadcast(JavascriptEventConstants.ErrorAjax);
            }
            unLockCommandBars();
            unLockTabs();
        }

        // Script loaded callback, send resolve
        $window.initMap = function () {
            deferred.resolve();
        }

        loadScript();

        return deferred.promise.then(function () {
            end(false);
        }, function () {
            end(true);
        });
    }

    //#region Service registration

    angular.module("sw_layout").factory("loadGoogleMapApi", ["$window", "$q", "$rootScope", "$log", loadGoogleMapApi]);

    //#endregion

})(angular);