(function (angular) {
    "use strict";

    function fsWorkorderOfflineService(crudContextHolderService, dao, $timeout) {
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

        function onNewDetailLoad(scope, schmema, datamap) {
            // defaults origination to 'Field Analysis'
            dao.findSingleByQuery("AssociationData", `application='classstructure' and datamap like '%"description":"Field Analysis"%'`)
                .then(a => {
                    if (!a || !a.datamap || !a.datamap.classstructureid) {
                        return;
                    }
                    const id = a.datamap.classstructureid;
                    const description = a.datamap.description;
                    datamap["classstructureid"] = id;
                    // setting just the viewValue to show description
                    // TODO: make $formatters do their job correctly
                    const input = document.querySelector("input[ion-autocomplete][item-value-key='datamap.classstructureid']");
                    if (!input) {
                        return;
                    }
                    $timeout(() => {
                        const $ngModel = angular.element(input).controller("ngModel");
                        $ngModel.$viewValue = `${id} - ${description}`;
                        $ngModel.$render();
                    });
                });
        }

        //#endregion

        //#region Service Instance
        const service = {
            afterFailureChanged,
            afterProblemChanged,
            afterCauseChanged,
            afterRemedyChanged,
            updateLocation,
            clearAsset,
            onNewDetailLoad
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("maximo_offlineapplications")
        .factory("fsWorkorderOfflineService", ["crudContextHolderService", "swdbDAO", "$timeout", fsWorkorderOfflineService]);

    //#endregion

})(angular);