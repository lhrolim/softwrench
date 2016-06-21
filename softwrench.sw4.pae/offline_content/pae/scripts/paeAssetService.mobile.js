
(function (angular) {
    "use strict";

    //#region Service registration

    angular.module("maximo_offlineapplications").factory("paeAssetService", [
        "scanningCommonsService", "$log", "crudContextService", "offlineAuditService", "$ionicPopup", "maximoDataService","swdbDAO","offlineSaveService",
        paeAssetService]);

    //#endregion

    function paeAssetService(scanningCommonsService, $log, crudContextService, offlineAuditService, $ionicPopup, maximoDataService,swdbDAO,offlineSaveService) {

  

        //#region Utils

        function initScanEventListener(schema, parameters) {
            scanningCommonsService.registerScanCallBackOnSchema(parameters, function (data) {
                var loadedAsset = null;
                maximoDataService.loadItemByMaximoUid("asset", schema, data)
                    .then(function (asset) {
                        asset.isDirty = true;
                        
                        return offlineSaveService.saveItem("asset", asset,false);
                    })
                    .then(function (asset) {
                        loadedAsset = asset;
                        return offlineAuditService.registerEvent("scan", "asset", asset.id, asset.remoteId, data);
                    })
                    .then(function (auditentry) {
                        return crudContextService.loadDetail(loadedAsset);
                    })
                    .catch(function (error) {
                        $ionicPopup.alert({ title: error });
                    });
            });
        }

        //#endregion

        //#region Public methods

        function preSync(datamap,originaldatamap) {
            datamap["#originallocation"] = datamap["location"];
        }

        function initAssetDetailListener(scope, schema, datamap, parameters) {
            initScanEventListener(schema, parameters);
        };

        function initAssetGridListener(scope, schema, datamap, parameters) {
            initScanEventListener(schema, parameters);
        };

        //#endregion

        //#region Service Instance

        var service = {
            initAssetDetailListener: initAssetDetailListener,
            initAssetGridListener: initAssetGridListener,
            preSync: preSync
        };

        return service;

        //#endregion

        
    }



})(angular);
