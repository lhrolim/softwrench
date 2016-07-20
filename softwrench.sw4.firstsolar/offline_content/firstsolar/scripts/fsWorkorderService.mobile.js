(function (angular) {
    "use strict";

    function fsWorkorderOfflineService(crudContextService, dao, $timeout, securityService) {
        //#region Utils

        //#endregion

        //#region Public methods

        function afterFailureChanged() {
            const dm = crudContextService.currentDetailItemDataMap();
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
            const dm = crudContextService.currentDetailItemDataMap();
            if (dm["fr1code"] != null) {
                dm["fr1code"] = "null$ignorewatch";
            }
            if (dm["fr2code"] != null) {
                dm["fr2code"] = "null$ignorewatch";
            }
        }
        function afterCauseChanged() {
            const dm = crudContextService.currentDetailItemDataMap();
            if (dm["fr2code"] != null) {
                dm["fr2code"] = "null$ignorewatch";
            }
        }
        function afterRemedyChanged() {

        }

        function updateLocation(event) {
            const datamap = event.datamap;

            const asset = datamap["assetnum"];
            if (!asset || (angular.isArray(asset) && asset.length === 0)) {
                return;
            }

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
                    // TODO: load association descriptions correctly, similar to online mode
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

        /**
         * Moves current item to 'workorder' application. 
         * 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         * @returns {Promise<entities.DataEntry>} 
         */
        function assignWorkOrder(schema, datamap) {
            // const user = securityService.currentFullUser();
            const item = crudContextService.currentDetailItem();
            // datamap["owner"] = user["PersonId"];
            item["application"] = "workorder";
            return crudContextService.saveChanges()
                .then(saved =>
                    // TODO: set list model (in the crud context) and list view (in the history stack) manually so screen transition is not so agravating
                    crudContextService.loadApplicationGrid("workorder", "WO - Assigned", "list")
                        // $timeout required so list controller has time to get and dispose it's viewmodel
                        .then(() => $timeout(() => crudContextService.loadDetail(saved), 0, false)) 
                );
        }

        //#region WhereClauses
        // textindex04 = location of the wo
        const locationsOfAssignedWos = "select textindex04 from DataEntry where application = 'workorder' and textindex04 is not null";

        // textindex01 = location of locancestor
        // textindex02 = ancestor of locancestor
        const childLocationOfAssignedWos = `select textindex01 from associationdata where application = 'locancestor' and textindex02 in (${locationsOfAssignedWos})`;
        
        // textindex01 = location of location
        const preferredLocations = `textindex01 in (${locationsOfAssignedWos}) or textindex01 in (${childLocationOfAssignedWos})`;

        function getLocationsWhereClause() {
            return preferredLocations;
        }

        // textindex01 = location of locancestor
        // textindex02 = ancestor of locancestor
        const childLocationsOfGivenLocation = "select textindex01 from associationdata where application = 'locancestor' and textindex02 = @location";

        // textindex01 = location of asset
        const assetsWithLocationEqualOrDescendant = `textindex01 = @location or textindex01 in (${childLocationsOfGivenLocation})`;

        function getAssetWhereClause() {
            return assetsWithLocationEqualOrDescendant;
        }

        function getFacilityFilterWhereClause(option) {
            const facilities = option.split(",");
            const terms = [];
            angular.forEach(facilities, facility => {
                const trimmed = facility.trim();
                if (!trimmed) {
                    return;
                }

                // textindex04 = location of the wo
                terms.push(`textindex04 like '${trimmed}%'`);
            });

            return `(${terms.join(" or ")})`;
        }

        //#endregion

        //#region Filter providers
        function getFacilityFilterOptions() {
            const options = [];

            const user = securityService.currentFullUser();
            if (!user) {
                return options;
            }

            const props = user.properties;
            if (!props) {
                return options;
            }

            const facilities = props["sync.facilities"];
            if (!facilities) {
                return options;
            }

            angular.forEach(facilities.sort(), facility => {
                const option = { value: facility };
                option.label = option.value;
                option.text = option.value;
                options.push(option);
            });

            return options;
        }
        //#endregion

        //#endregion

        //#region Service Instance
        const service = {
            // change event handlers
            afterFailureChanged,
            afterProblemChanged,
            afterCauseChanged,
            afterRemedyChanged,
            updateLocation,
            clearAsset,
            onNewDetailLoad,
            assignWorkOrder,
            // whereclauses
            getLocationsWhereClause,
            getAssetWhereClause,
            getFacilityFilterWhereClause,
            // filter providers
            getFacilityFilterOptions
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("maximo_offlineapplications")
        .factory("fsWorkorderOfflineService", ["crudContextService", "swdbDAO", "$timeout", "securityService", fsWorkorderOfflineService]);

    //#endregion

})(angular);