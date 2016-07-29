(function (mobileServices, angular) {
    "use strict";

    function swAlertPopup($ionicPopup, $timeout) {

        //#region Utils

        var control = {
            queue: [], // queue of popup configs scheduled to be shown
            showing: false, // flag indicating if there's an active (showing) popup
            showinterval: 100 // empirically determined: 100 milliseconds interval in-between closing current and showing next one 
        }

        function showNext(previous) {
            // close previous (currently showing) popup before showing the next
            if (!!previous) {
                control.showing = false;
                previous.close();
            }

            // no popups scheduled to be shown
            if (control.queue.length <= 0) return;

            // schedule next popup to be shown
            $timeout(function () {
                const config = control.queue.shift();
                var popup = $ionicPopup.alert(config);
                control.showing = true;
                // schedule preconfigured (by the caller) alert popup close
                var timeid = null;
                const timeout = config.timeout;
                if (angular.isNumber(timeout) && timeout > 0) {
                    timeid = $timeout(function () {
                        showNext(popup);
                    }, timeout);
                }
                // popup was closed: open next on queue
                popup.then(function () {
                    // if closed by the user, cancel the timeout to automatically close it
                    if (timeid) $timeout.cancel(timeid);
                    showNext(popup);
                });

            }, control.showinterval); 

        }

        function hidrateConfig(config, timeout) {
            config.timeout = timeout;
            return config;
        }

        //#endregion

        //#region Public methods

        /**
         * Shows alert popup (same as $ionicPopup#showAlert)
         * 
         * @param {} config same as $ionicPopup#showAlert
         * @param Long timeout interval in milliseconds to automatically close the popup.
         *             If =< 0 or NaN, will be ignored.
         */
        function show(config, timeout) {
            config = hidrateConfig(config, timeout);
            control.queue.push(config);
            // if first on queue and no current active popup: trigger show mechanism 
            if (control.queue.length <= 1 && !control.showing) {
                showNext();
            }
        }

        //#endregion

        //#region Service Instance
        const service = {
            show
        };
        return service;
        //#endregion
    }
    //#region Service registration
    mobileServices.factory("swAlertPopup", ["$ionicPopup", "$timeout", swAlertPopup]);
    //#endregion

})(mobileServices, angular);