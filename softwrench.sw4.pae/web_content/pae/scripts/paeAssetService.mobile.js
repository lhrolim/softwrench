
(function (angular) {
    "use strict";

    //#region Service registration

    angular.module("maximo_offlineapplications").factory("paeAssetService", [
        "scanningCommonsService", "$log", "crudContextService", "offlineAuditService","$ionicPopup",
        paeAssetService]);

    //#endregion

    function paeAssetService(scanningCommonsService, $log, crudContextService, offlineAuditService, $ionicPopup) {

        //#region Service Instance

        var service = {
            initAssetDetailListener: initAssetDetailListener,
            initAssetGridListener: initAssetGridListener
        };

        return service;

        //#endregion

        //#region Utils


        //#endregion

        //#region Public methods


        function initAssetDetailListener(scope, schema, datamap, parameters) {
            scanningCommonsService.registerScanCallBackOnSchema(parameters, function(data) {
                crudContextService.loadDetailByMaximoUid("asset",schema, data);
                offlineAuditService.registerEvent("scan", "asset", data);
            });
        };

        function initAssetGridListener(scope, schema, datamap, parameters) {
            scanningCommonsService.registerScanCallBackOnSchema(parameters, function(data) {
                crudContextService.loadDetailByMaximoUid("asset", schema,data).then(function(item) {
                    offlineAuditService.registerEvent("scan", "asset", data);
                }).catch(function() {
                    $ionicPopup.alert({
                        title: "Asset {0} not found".format(data),
                    });
                });
                
            });
        };

        //#endregion


    }



})(angular);
