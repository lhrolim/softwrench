(function (angular) {
    "use strict";

    function fsWorkorderOfflineService(crudContextHolderService) {
        //#region Utils

        //#endregion

        //#region Public methods

        function afterFailureChanged() {
            const dm = crudContextHolderService.currentDetailItemDataMap();
            if (dm["problemcode"] != null) {
                dm["problemcode"] = "null$ignorewatch";
            }
            if (dm["fr1code"] != null) {
                dm["fr1code"] = "null$ignorewatch";
            }
            if (dm["fr2code"] != null) {
                dm["fr2code"] = "null$ignorewatch";
            }
        }
        function afterProblemChanged() {
            const dm = crudContextHolderService.currentDetailItemDataMap();
            if (dm["fr1code"] != null) {
                dm["fr1code"] = "null$ignorewatch";
            }
            if (dm["fr2code"] != null) {
                dm["fr2code"] = "null$ignorewatch";
            }
        }
        function afterCauseChanged() {
            const dm = crudContextHolderService.currentDetailItemDataMap();
            if (dm["fr2code"] != null) {
                dm["fr2code"] = "null$ignorewatch";
            }
        }
        function afterRemedyChanged() {

        }

        function updateLocation(event) {
            const datamap = event.datamap;
            const location = datamap["offlineasset_.location"];
            datamap["location"] = `${location}$ignorewatch`;
        }

        function clearAsset(event) {
            const datamap = event.datamap;
            datamap["assetnum"] = "null$ignorewatch";
        }

        //#endregion

        //#region Service Instance
        const service = {
            afterFailureChanged,
            afterProblemChanged,
            afterCauseChanged,
            afterRemedyChanged,
            updateLocation,
            clearAsset
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("maximo_offlineapplications").factory("fsWorkorderOfflineService", ["crudContextHolderService", fsWorkorderOfflineService]);

    //#endregion

})(angular);