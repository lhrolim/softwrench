(function (angular) {
    "use strict";

    //TODO: crudContextHolderService should be somehow on a dependant module...
    angular.module('sw_scan', []);

    //#region Service registration

    angular.module("sw_scan").factory("scanningCommonsService", ["$rootScope", "crudContextHolderService", scanningCommonsService]);

    //#endregion

    function scanningCommonsService($rootScope, crudContextHolderService) {

        $rootScope.$on("sw.crud.applicationchanged", function() {
            $(document).scannerDetection(null);
        });

        //#region Utils

        var timeBetweenCharacters = isMobile() ? 300 : 14; // Used by the jQuery scanner detection plug in to differentiate scanned data and data input from the keyboard
        if ("true" === sessionStorage.debugscanner) {
            timeBetweenCharacters = 30000;
        }

        var scanCallbackMap = {

        };
        //#endregion

        //#region Public methods

        /**
         * Register the given callback function to be executed upon a successful scanning on a context that matches the matchingparameters.
         * To simulate a scan event in a debugging evironment just call <code>$(document).scannerDetection("<data_being_scanned>")</code>. 
         * Remember that in order to trigger your callbacks they need to have been previously registered and the event needs to be fired
         * in the correct context (defined by the matchingparamenters argument).
         * 
         * @param {Object} matchingparameters dictionary in the format:
         *              {
         *                  applicationName: String, // the name of the application to register the scan,
         *                  schemaId: String, //the schemaId to register the scan
         *                  tabId: String // the id of the tab to register the scan (composition or tab)
         *              }
         * @param {Function<?, String>} callback receives the scanned data as it's single argument 
         */
        function registerScanCallBackOnSchema(matchingparameters, callback) {
            const registerApplication = matchingparameters.applicationName;
            const registerSchemaId = matchingparameters.schemaId;
            const registerTabId = matchingparameters.tabid || "";

            scanCallbackMap[`${registerApplication}.${registerSchemaId}.${registerTabId}`] = callback;

            $(document).scannerDetection({
                avgTimeByChar: timeBetweenCharacters,

                onComplete: function (data) {
                    const tabId = crudContextHolderService.getActiveTab() || "";
                    const schema = crudContextHolderService.currentSchema();
                    const applicationName = crudContextHolderService.currentApplicationName();
                    if (!applicationName || !schema) {
                        //we´re not on a crud screen, let´s take the chance to unregister the scanner detector
                        //whenever it reaches the proper screen it can then register it self again
                        $(document).scannerDetection(null);
                        return;
                    }

                    if (!Array.isArray(schema)) {
                        //sometimes we could have multiple schemas at the same time on screen, such as a master-detail for compositions 
                        const callbackFn = scanCallbackMap[`${applicationName}.${schema.schemaId}.${tabId}`];
                        if (angular.isFunction(callbackFn)) {
                            callbackFn(data);
                        } else {
                            //no call back defined, let´s take the chance to unregister the scanner detector
                            //whenever it reaches the proper screen it can then register it self again
                            $(document).scannerDetection(null);
                        }
                        return;
                    }

                    //if we have multiple schemas on screen, invoke both functions unless they are the same
                    const callbackFn1 = scanCallbackMap[`${applicationName}.${schema[0].schemaId}.${tabId}`];
                    const callbackFn2 = scanCallbackMap[`${applicationName}.${schema[1].schemaId}.${tabId}`];
                    if (!angular.isFunction(callbackFn1) && !angular.isFunction(callbackFn2)) {
                        //no call back defined, let´s take the chance to unregister the scanner detector
                        //whenever it reaches the proper screen it can then register it self again
                        $(document).scannerDetection(null);
                        return;
                    }

                    if (callbackFn1 === callbackFn2) {
                        callbackFn1(data);
                        return;
                    }
                    if (angular.isFunction(callbackFn1)) {
                        callbackFn1(data);
                    }
                    if (angular.isFunction(callbackFn2)) {
                        callbackFn2(data);
                    }

                }
            });
        };

        function getTimeBetweenChars() {
            return timeBetweenCharacters;
        }

        //#endregion

        //#region Service Instance
        const service = {
            registerScanCallBackOnSchema,
            getTimeBetweenChars
        };
        return service;
        //#endregion
    }

})(angular);
