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
         * @param {} matchingparameters dictionary in the format:
         *              {
         *                  applicationName: String, // the name of the application to register the scan,
         *                  schemaId: String, //the schemaId to register the scan
         *                  tabId: String // the id of the tab to register the scan (composition or tab)
         *              }
         * @param function callback receives the scanned data as it's single argument 
         */
        function registerScanCallBackOnSchema(matchingparameters, callback) {

            var registerApplication = matchingparameters.applicationName;
            var registerSchemaId = matchingparameters.schemaId;
            var registerTabId = matchingparameters.tabid || "";

            scanCallbackMap["{0}.{1}.{2}".format(registerApplication, registerSchemaId, registerTabId)] = callback;

            $(document).scannerDetection({
                avgTimeByChar: timeBetweenCharacters,


                onComplete: function (data) {
                    var tabId = crudContextHolderService.getActiveTab() || "";
                    var schema = crudContextHolderService.currentSchema();
                    var applicationName = crudContextHolderService.currentApplicationName();
                    if (applicationName == null || schema == null) {
                        //we´re not on a crud screen, let´s take the chance to unregister the scanner detector
                        //whenever it reaches the proper screen it can then register it self again
                        $(document).scannerDetection(null);
                        return;
                    }

                    if (!Array.isArray(schema)) {
                        //sometimes we could have multiple schemas at the same time on screen, such as a master-detail for compositions 
                        var callbackFn = scanCallbackMap["{0}.{1}.{2}".format(applicationName, schema.schemaId, tabId)];
                        if (callbackFn) {
                            callbackFn(data);
                        } else {
                            //no call back defined, let´s take the chance to unregister the scanner detector
                            //whenever it reaches the proper screen it can then register it self again
                            $(document).scannerDetection(null);
                        }
                        return;
                    }

                    //if we have multiple schemas on screen, invoke both functions unless they are the same
                    var callbackFn1 = scanCallbackMap["{0}.{1}.{2}".format(applicationName, schema[0].schemaId, tabId)];
                    var callbackFn2 = scanCallbackMap["{0}.{1}.{2}".format(applicationName, schema[1].schemaId, tabId)];
                    if (callbackFn1 == null && callbackFn2 == null) {
                        //no call back defined, let´s take the chance to unregister the scanner detector
                        //whenever it reaches the proper screen it can then register it self again
                        $(document).scannerDetection(null);
                        return;
                    }

                    if (callbackFn1 == callbackFn2) {
                        callbackFn1(data);
                        return;
                    }
                    if (callbackFn1) {
                        callbackFn1(data);
                    }
                    if (callbackFn2) {
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

        var service = {
            registerScanCallBackOnSchema: registerScanCallBackOnSchema,
            getTimeBetweenChars: getTimeBetweenChars
        };

        return service;

        //#endregion
    }



})(angular);
