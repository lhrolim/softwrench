
(function (angular) {
    "use strict";

    //#region Service registration

    angular.module("maximo_applications").factory("paeAssetService", [
        "scanningCommonsService", "validationService", "$http", "submitService",
        "contextService", "searchService", "redirectService", "alertService",
        paeAssetService]);

    //#endregion

    function paeAssetService(scanningCommonsService, validationService, $http, submitService, contextService, searchService, redirectService, alertService, crudContextHolderService) {

        //#region Service Instance

        var service = {
            initAssetDetailListener: initAssetDetailListener,
            initAssetGridListener: initAssetGridListener
        };

        return service;

        //#endregion

        //#region Utils

        function navigateToAsset(data) {
            var user = contextService.getUserData();
            var searchData = {
                siteid: user.siteId,
                orgid: user.orgId,
                assetnum: data
            };

            var equalityoperator = searchService.getSearchOperationBySymbol("=");
            var extraparameters = {
                searchOperators: {
                    siteid: equalityoperator,
                    orgid: equalityoperator,
                    assetnum: equalityoperator
                }
            }

            searchService.searchWithData("asset", searchData, null, extraparameters).success(function (resultData) {

                var resultObject = resultData.resultObject;

                if (resultObject.length == 0) {
                    alertService.alert("Asset record not found. Please contact your System Administrator.");
                    return;
                }

                if (resultObject.length > 1) {
                    alertService.alert("More than one asset found. Please contact your System Administrator.");
                    return;
                }

                var assetId = resultObject[0]['fields']['assetid'];
                var param = {};
                param.id = assetId;
                var application = 'asset';
                var detail = 'detail';
                var mode = 'input';
                param.scanmode = true;

                redirectService.goToApplicationView(application, detail, mode, null, param, null);
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

                    var searchParameters = {
                        continue: function () {
                            var jsonString = angular.toJson(datamap);
                            var httpParameters = {
                                application: "asset",
                                currentSchemaKey: "detail.input.web",
                                platform: "web",
                                scanmode: true
                            };
                            var urlToUse = url("/api/data/asset/" + datamap["assetid"] + "?" + $.param(httpParameters));
                            $http.put(urlToUse, jsonString).success(function () {
                                // navigate to the asset which had been scanned
                                navigateToAsset(data);
                            }).error(function () {
                                // Failed to update the asset
                            });
                        }
                    };
                    submitService.submitConfirmation(null, datamap, searchParameters);
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

     
    }

    

})(angular);
