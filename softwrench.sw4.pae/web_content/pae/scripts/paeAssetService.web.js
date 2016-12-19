(function (angular, $) {
    "use strict";

    //#region Service registration

    angular.module("maximo_applications").factory("paeAssetService", [
        "scanningCommonsService", "validationService", "$http", "submitService",
        "contextService", "searchService", "redirectService", "alertService",
        "crudContextHolderService", "formatService", "schemaService",
        paeAssetService]);

    //#endregion

    function paeAssetService(scanningCommonsService, validationService, $http, submitService, contextService, searchService, redirectService, alertService, crudContextHolderService, formatService, schemaService) {
        //#region Utils

        function navigateToAsset(data) {
            const user = contextService.getUserData();
            const searchData = {
                siteid: user.siteId,
                orgid: user.orgId,
                assetnum: data
            };
            const equalityoperator = searchService.getSearchOperationBySymbol("=");
            const extraparameters = {
                searchOperators: {
                    siteid: equalityoperator,
                    orgid: equalityoperator,
                    assetnum: equalityoperator
                }
            };
            searchService.searchWithData("asset", searchData, null, extraparameters).then(response => {
                const resultObject = response.data.resultObject;

                if (resultObject.length <= 0) {
                    alertService.alert("Asset record not found. Please contact your System Administrator.");
                    return;
                }

                if (resultObject.length > 1) {
                    alertService.alert("More than one asset found. Please contact your System Administrator.");
                    return;
                }

                const assetId = resultObject[0]["assetid"];
                const param = {
                    id: assetId,
                    scanmode: true
                };

                redirectService.goToApplicationView("asset", "detail", "input", null, param, null);
            });
        }

        //#endregion

        //#region Public methods

        function initAssetDetailListener(scope, schema, datamap, parameters) {

            datamap["#originallocation"] = datamap["location"];

            // Set the avgTimeByChar to the correct value depending on if using mobile or desktop
            $(document).scannerDetection({
                avgTimeByChar: scanningCommonsService.getTimeBetweenChars(),
                onComplete: function (data) {
                    if (!crudContextHolderService.getDirty()) {
                        navigateToAsset(data);
                        return;
                    }

                    const searchParameters = {
                        'continue': function () {
                            const jsonString = angular.toJson(datamap);
                            const httpParameters = {
                                application: "asset",
                                currentSchemaKey: "detail.input.web",
                                platform: "web",
                                scanmode: true
                            };
                            const urlToUse = url(`/api/data/asset/${datamap["assetid"]}?${$.param(httpParameters)}`);

                            $http.put(urlToUse, jsonString)
                                .then(() => {
                                    // navigate to the asset which had been scanned
                                    navigateToAsset(data);
                                })
                                .catch(error => {
                                    // Failed to update the asset
                                });
                        }
                    };
                    submitService.submitConfirmation();
                }
            });
        };

        function initAssetGridListener (scope, schema, datamap, parameters) {

            // Set the avgTimeByChar to the correct value depending on if using mobile or desktop
            $(document).scannerDetection({
                avgTimeByChar: scanningCommonsService.getTimeBetweenChars(),
                onComplete: function (data) {
                    navigateToAsset(data);
                }
            });
        };

        //#endregion

        //#region Service Instance
        const service = {
            initAssetDetailListener,
            initAssetGridListener,
            transportationAssetChanged
        };
        return service;
        //#endregion
    }

    

})(angular, jQuery);
