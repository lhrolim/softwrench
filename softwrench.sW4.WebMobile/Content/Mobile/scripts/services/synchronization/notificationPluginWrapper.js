(function (mobileServices) {
    "use strict";

    function notificationPluginWrapper($window, $rootScope, $timeout) {

        /**
         * Register's ngCordova's notification event listeners.
         * Has to be called within "$ionicPlatform.ready" callback.
         * Hack: the plugin isn't ready at the time the listeners are originally bound by ngCordova and, 
         * since ngCordova doesn't expose their binding to be called externally, it is required to be manually called.
         */
        var init = function() {
            // ----- "Scheduling" events

            // A local notification was scheduled
            $window.cordova.plugins.notification.local.on("schedule", function (notification, state) {
                $timeout(function () {
                    $rootScope.$broadcast("$cordovaLocalNotification:schedule", notification, state);
                });
            });

            // A local notification was triggered
            $window.cordova.plugins.notification.local.on("trigger", function (notification, state) {
                $timeout(function () {
                    $rootScope.$broadcast("$cordovaLocalNotification:trigger", notification, state);
                });
            });

            // ----- "Update" events

            // A local notification was updated
            $window.cordova.plugins.notification.local.on("update", function (notification, state) {
                $timeout(function () {
                    $rootScope.$broadcast("$cordovaLocalNotification:update", notification, state);
                });
            });

            // ----- "Clear" events

            // A local notification was cleared from the notification center
            $window.cordova.plugins.notification.local.on("clear", function (notification, state) {
                $timeout(function () {
                    $rootScope.$broadcast("$cordovaLocalNotification:clear", notification, state);
                });
            });

            // All local notifications were cleared from the notification center
            $window.cordova.plugins.notification.local.on("clearall", function (state) {
                $timeout(function () {
                    $rootScope.$broadcast("$cordovaLocalNotification:clearall", state);
                });
            });

            // ----- "Cancel" events

            // A local notification was cancelled
            $window.cordova.plugins.notification.local.on("cancel", function (notification, state) {
                $timeout(function () {
                    $rootScope.$broadcast("$cordovaLocalNotification:cancel", notification, state);
                });
            });

            // All local notifications were cancelled
            $window.cordova.plugins.notification.local.on("cancelall", function (state) {
                $timeout(function () {
                    $rootScope.$broadcast("$cordovaLocalNotification:cancelall", state);
                });
            });

            // ----- Other events

            // A local notification was clicked
            $window.cordova.plugins.notification.local.on("click", function (notification, state) {
                $timeout(function () {
                    $rootScope.$broadcast("$cordovaLocalNotification:click", notification, state);
                });
            });
        }

        var service = {
            init: init
        };

        return service;
    }

    mobileServices.factory("notificationPluginWrapper", ["$window", "$rootScope", "$timeout", notificationPluginWrapper]);

})(mobileServices);
