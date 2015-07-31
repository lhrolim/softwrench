
(function (angular) {
    "use strict";

    //TODO: crudContextHolderService should be somehow on a dependant module...
    angular.module('sw_scan', []);

    //#region Service registration

    angular.module("sw_scan").factory("scanningCommonsService", ["crudContextHolderService", scanningCommonsService]);

    //#endregion

    function scanningCommonsService(crudContextHolderService) {

        //#region Utils

        var timeBetweenCharacters = isMobile() ? 35 : 14; // Used by the jQuery scanner detection plug in to differentiate scanned data and data input from the keyboard
        if ("true" == sessionStorage.debugscanner) {
            timeBetweenCharacters = 30000;
        }

        var scanCallbackMap = {

        };
        //#endregion

        //#region Public methods


        function registerScanCallBackOnSchema(matchingparameters, callback) {
            /// <summary>
            /// 
            ///  Register the given callback function to be executed upon a successful scanning on a context that matches the matchingparameters 
            /// 
            /// </summary>
            /// <param name="matchingparameters">
            ///  applicationName --> the name of the application to register the scan
            ///  schemaId --> the schemaId to register the scan
            ///  tabId --> the id of the tab to register the scan (composition or tab)
            /// </param>
            /// <param name="callback"></param>

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
                    
                    if (!Array.isArray(schema)) {
                        //sometimes we could have multiple schemas at the same time on screen, such as a master-detail for compositions 
                        var callbackFn = scanCallbackMap["{0}.{1}.{2}".format(applicationName, schema.schemaId, tabId)];
                        if (callbackFn) {
                            callbackFn(data);
                        }
                        return;
                    }

                    //if we have multiple schemas on screen, invoke both functions unless they are the same
                    var callbackFn1 = scanCallbackMap["{0}.{1}.{2}".format(applicationName, schema[0].schemaId, tabId)];
                    var callbackFn2 = scanCallbackMap["{0}.{1}.{2}".format(applicationName, schema[1].schemaId, tabId)];
                    if (callbackFn1 == callbackFn2) {
                        callbackFn1(data);
                        return;
                    }
                    callbackFn1(data);
                    callbackFn2(data);
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
