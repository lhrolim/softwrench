(function (angular, _) {
    "use strict";

    function paeAssetService(scanningCommonsService, $log, crudContextService, audit, $ionicPopup, maximoDataService, dao, offlineSaveService, securityService, $q) {
        //#region Utils
        const config = {
            /** holds previously scanned VIN code when scanning assets by ASSSETNUM */
            vin: null,
            /** flag indicates whether or not to find the scanned assets by VIN (the other otpion is by ASSSETNUM) */
            vinMode: true,

            codes: {
                ignore: "CODE_IGNORE_ERROR"
            }
        };

        function scanBySerialnum(serialnum) {

            return maximoDataService.loadSingleItemByField("transportation", "serialnum", serialnum)
                .catch(error => {
                    const confirm = $ionicPopup.confirm({
                        title: "Transportation Asset Scan",
                        template: `VIN/SN ${serialnum} not found.<br>Would you like to scan using the asset?`
                    });
                    return confirm.then(res => {
                        if (!res) {
                            return $q.reject({ code: config.codes.ignore });
                        }
                        // prepare 'scan by assetnum' mode
                        config.vin = serialnum;
                        config.vinMode = false;
                        return $q.reject({ code: config.codes.ignore });
                    });
                });
        }

        function scanByAssetnum(assetnum, schema) {
            return maximoDataService.loadItemByMaximoUid("transportation", schema, assetnum)
                .then(asset => {
                    asset.datamap["serialnum"] = config.vin;
                    return asset;
                })
                .finally(() => {
                    // restore original mode
                    config.vin = null;
                    config.vinMode = true;
                });
        }

        function auditAndOpenDetail(promise, application, scanData) {
            return promise
                // register scan audit event
                .then(asset =>
                    audit.registerEvent("scan", application, asset.id, asset.remoteId, scanData).then(entry => asset)
                )
                // refresh grid (reflect detail changes) and redirect to asset detail
                .then(asset => crudContextService.refreshGrid().then(() => asset))
                .then(asset => crudContextService.loadDetail(asset))
                // alert error
                .catch(error => {
                    // user action interrupted operation
                    if (error && error.code === config.codes.ignore) return;

                    $log.get("paeAssetService#initScanEventListener", ["scan"]).error(error);
                    $ionicPopup.alert({ title: error.message });
                });
        }

        function initScanEventListener(schema, parameters) {
            scanningCommonsService.registerScanCallBackOnSchema(parameters, function (data) {

                const promise = maximoDataService.loadItemByMaximoUid("asset", schema, data).then(asset => {
                    asset.isDirty = true;
                    return offlineSaveService.saveItem("asset", asset, false);
                });

                auditAndOpenDetail(promise, "asset", data);
            });
        }

        function initTransportationScanEventListener(schema, parameters) {
            scanningCommonsService.registerScanCallBackOnSchema(parameters, function (data) {

                // scan according to mode
                const scanPromise = config.vinMode ? scanBySerialnum(data) : scanByAssetnum(data, schema);

                // common scan chain
                const promise = scanPromise.then(asset => {
                    // update asset
                    const personId = securityService.currentFullUser()["PersonId"];
                    const now = new Date();
                    asset.isDirty = true;
                    asset.datamap["invposttype"] = "AUTOMATIC";
                    asset.datamap["invpostdateby"] = personId;
                    asset.datamap["changeby"] = personId;
                    asset.datamap["invpostdate"] = now;
                    asset.datamap["changedate"] = now;
                    asset.datamap["scanremarks"] = "No Scan Remarks Entered";

                    return offlineSaveService.saveItem("transportation", asset, false);
                });

                auditAndOpenDetail(promise, "transportation", data);
            });
        }

        //#endregion

        //#region Public methods

        function preSync(datamap, originaldatamap) {
            datamap["#originallocation"] = datamap["location"];
        }

        function initAssetDetailListener(scope, schema, datamap, parameters) {
            initScanEventListener(schema, parameters);
        }

        function initAssetGridListener(scope, schema, datamap, parameters) {
            initScanEventListener(schema, parameters);
        }

        function initTransportationDetailListener(scope, schema, datamap, parameters) {
            initTransportationScanEventListener(schema, parameters);
        }

        function initTransportationGridListener(scope, schema, datamap, parameters) {
            initTransportationScanEventListener(schema, parameters);
        }

        function transportationAssetChanged(schema, datamap) {
            const user = securityService.currentFullUser();
            datamap["changeby"] = user["PersonId"];
            datamap["changedate"] = new Date();
        }

        //#endregion

        //#region Service Instance
        const service = {
            initAssetDetailListener,
            initAssetGridListener,
            initTransportationDetailListener,
            initTransportationGridListener,
            transportationAssetChanged,
            preSync
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("maximo_offlineapplications")
        .factory("paeAssetService", [
            "scanningCommonsService", "$log", "crudContextService", "offlineAuditService", "$ionicPopup", "maximoDataService", "swdbDAO", "offlineSaveService", "securityService", "$q", paeAssetService
        ]);

    //#endregion



})(angular, _);
