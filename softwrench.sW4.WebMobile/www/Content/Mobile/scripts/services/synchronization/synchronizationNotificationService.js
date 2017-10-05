(function (mobileServices, angular) {
    "use strict";

    var synchronizationNotificationService = function ($rootScope, menuModelService, routeService, $cordovaLocalNotification, $ionicPopup, notificationPluginWrapper) {

        /* DEFAULT CONFIGURATION */

        var notificationConfig = {
            title: "Synchronization Result",
            message: "A synchronization result has been received. Would you like to check it?",
            idseed: 0
        };

        /* UTILS */

        var isSimulator = !!isRippleEmulator();

        var buildNotification = function(operation) {
            return {
                id: notificationConfig.idseed++, // id has to be an Integer otherwise it is defaulted to 0
                title: notificationConfig.title,
                text: notificationConfig.message,
                data: {
                    operationId: operation.id
                }
            };
        };

        /* ACTIONS */

        /**
         * Registers a local notification on the device notification center.
         * 
         * @param SyncOperation operation 
         * @returns Promise 
         */
        var deviceNotifySyncReceived = function (operation) {
            var notification = buildNotification(operation);
            return $cordovaLocalNotification.schedule(notification);
        };

        /**
         * Prompts the user with a popup emulating a notification (clickable and dismissable, but a popup).
         * 
         * @param SyncOperation operation 
         * @returns Promise 
         */
        var simulatorNotifySyncReceived = function(operation) {
            return  $ionicPopup.confirm({
                title: notificationConfig.title,
                template: notificationConfig.message
            }).then(function (res) {
                if (res) {
                    // broadcast notification click event
                    var notification = buildNotification(operation);
                    $rootScope.$broadcast("$cordovaLocalNotification:click", notification);
                }
            });
        };

        /**
         * Registers a notification depending on the runtime the application is running in:
         * - device/device emulators: notification center
         * - Ripple emulator: popup
         * 
         * @param SyncOperation operation 
         * @returns Promise 
         */
        var notifySynchronizationReceived = function(operation) {
            if (isSimulator) {
                return simulatorNotifySyncReceived(operation);
            }
            return deviceNotifySyncReceived(operation);
        };

        /**
         * Check for permissions to show local notifications - iOS 8 NEEDS permission to run.
         * If permission is not granted prompt the user asking for permission.
         */
        var checkNotificationPermission = function() {
            if (isSimulator) {
                return;
            }
            $cordovaLocalNotification.hasPermission().then(function (granted) {
                $cordovaLocalNotification.cancelAll();
                if (!granted) {
                    $cordovaLocalNotification.promptForPermission();
                };
            });
        };

        /**
         * Prepares the app to use notification feature: registers plugin listeners and checks permission.
         */
        var prepareNotificationFeature = function() {
            if (!isSimulator) {
                notificationPluginWrapper.init();
                checkNotificationPermission();
            }
        };

        /* EVENT LISTENERS */

        $rootScope.$on("$cordovaLocalNotification:click", function ($event, notification, state) {
            var data = notification.data;
            if (angular.isString(notification.data)) {
                data = JSON.parse(data);
            }
            routeService.go("main.syncoperationdetail", { id: data.operationId });
            menuModelService.updateAppsCount();
        });

        /* SERVICE INSTANCE */

        var api = {
            notifySynchronizationReceived: notifySynchronizationReceived,
            checkNotificationPermission: checkNotificationPermission,
            prepareNotificationFeature: prepareNotificationFeature
        };
        
        return api;
    }

    mobileServices.factory("synchronizationNotificationService",
        ["$rootScope","menuModelService", "routeService", "$cordovaLocalNotification", "$ionicPopup", "notificationPluginWrapper", synchronizationNotificationService]);

})(mobileServices, angular);
