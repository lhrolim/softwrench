
(function (angular, _) {
    "use strict";

    const day = 24 * 60 * 60 * 1000;

    //#region WhereClauses
    // textindex04 = location of the wo
    const locationsOfAssignedWos = "select textindex04 from DataEntry where application = 'workorder' and textindex04 is not null";

    // textindex01 = location of locancestor
    // textindex02 = ancestor of locancestor
    const childLocationOfAssignedWos = `select textindex01 from associationdata where application = 'locancestor' and textindex02 in (${locationsOfAssignedWos})`;

    // textindex01 = location of location
    const preferredLocations = `textindex01 in (${locationsOfAssignedWos}) or textindex01 in (${childLocationOfAssignedWos})`;


    // textindex01 = location of locancestor
    // textindex02 = ancestor of locancestor
    const childLocationsOfGivenLocation = "select textindex01 from associationdata where application = 'locancestor' and textindex02 = @location";

    // textindex01 = location of asset
    const assetsWithLocationEqualOrDescendant = `textindex01 = @location or textindex01 in (${childLocationsOfGivenLocation})`;

    class fsWorkorderOfflineService {

        constructor($q, crudContextService, swdbDAO, $timeout, securityService, offlineSchemaService, $log) {
            this.$q = $q;
            this.crudContextService = crudContextService;
            this.dao = swdbDAO;
            this.$timeout = $timeout;
            this.securityService = securityService;
            this.offlineSchemaService = offlineSchemaService;
            this.$log = $log;
        }


        //afterchange
        afterFailureChanged() {
            const dm = this.crudContextService.currentDetailItemDataMap();
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


        //afterchange
        afterProblemChanged() {
            const dm = this.crudContextService.currentDetailItemDataMap();
            if (dm["fr1code"] != null) {
                dm["fr1code"] = "null$ignorewatch";
            }
            if (dm["fr2code"] != null) {
                dm["fr2code"] = "null$ignorewatch";
            }
        }

        //afterchange
        afterCauseChanged() {
            const dm = this.crudContextService.currentDetailItemDataMap();
            if (dm["fr2code"] != null) {
                dm["fr2code"] = "null$ignorewatch";
            }
        }
        //afterchange
        afterRemedyChanged() {

        }

        //afterchange --> asset selected
        updateLocation(event) {
            const datamap = event.datamap;

            const asset = datamap["assetnum"];
            if (!asset || (angular.isArray(asset) && asset.length === 0)) {
                return this.$q.when();
            }

            const location = datamap["offlineasset_.location"];
            const failurecode = datamap["offlineasset_.failurecode"];
            datamap["location"] = `${location}$ignorewatch`;
            datamap["failurecode"] = failurecode;
            if (!!failurecode) {
                return this.dao.findSingleByQuery("AssociationData", `textindex01='${failurecode}' and application = 'failurelistonly'`).then(result => {
                    datamap["failurelistonly_.failurelist"] = result.datamap.failurelist;
                });
            }


        }
        //afterchange
        clearAsset(event) {
            const datamap = event.datamap;
            datamap["assetnum"] = "null$ignorewatch";
        }

        onDetailLoad() {
            const log = this.$log.get("fsWorkorderOfflineService#onDetailLoad", ["crud", "detail"]);
            const item = this.crudContextService.currentDetailItem();
            const dm = item.datamap;

            const problemData = dm["problemlist"];
            const causeData = dm["fr1list"];
            const failurelist = dm["failurelist"];

            if (!!problemData) {
                dm["offlineproblemlist_.failurelist"] = problemData;
            }

            if (!!failurelist) {
                dm["failurelistonly_.failurelist"] = failurelist;
            }

            if (!!causeData) {
                dm["offlinecauselist_.failurelist"] = causeData;
            }
            log.debug("setting failurelist ids");

        }

        onNewDetailLoad(scope, schema, datamap) {
            var offlineSchemaService = this.offlineSchemaService;
            var $timeout = this.$timeout;

            // defaults origination to 'Field Analysis'
            this.dao.findSingleByQuery("AssociationData", `application='classstructure' and datamap like '%"description":"Field Analysis"%'`)
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
                    const input = document.querySelector("textarea[ion-autocomplete][item-value-key='datamap.classstructureid']");
                    const originationField = offlineSchemaService.getFieldByAttribute(schema, "classstructureid");
                    const showId = !_.contains([false, "false", "False", 0, "0"], originationField.rendererParameters["showCode"]);

                    if (!input) {
                        return;
                    }
                    $timeout(() => {
                        const $ngModel = angular.element(input).controller("ngModel");
                        $ngModel.$viewValue = showId ? `${id} - ${description}` : description;
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
        assignWorkOrder(schema, datamap) {
            var crudContextService = this.crudContextService;
            var $timeout = this.$timeout;
            // const user = securityService.currentFullUser();
            const item = crudContextService.currentDetailItem();
            // datamap["owner"] = user["PersonId"];
            item["application"] = "workorder";
            return this.crudContextService.saveChanges()
                .then(saved =>
                    // TODO: set list model (in the crud context) and list view (in the history stack) manually so screen transition is not so agravating
                    crudContextService.loadApplicationGrid("workorder", "list")
                        // $timeout required so list controller has time to get and dispose it's viewmodel
                        .then(() => $timeout(() => crudContextService.loadDetail(saved), 0, false))
                );
        }


        getLocationsWhereClause() {
            return preferredLocations;
        }

      


        getAssetWhereClause() {
            return assetsWithLocationEqualOrDescendant;
        }

        getFacilityFilterWhereClause(option) {
            const facilities = option.split(",");
            const terms = [];
            angular.forEach(facilities, facility => {
                const trimmed = facility.trim();
                if (!trimmed) {
                    return;
                }

                // textindex04 = location of the wo
                terms.push(`root.textindex04 like '${trimmed}%'`);
            });

            return `(${terms.join(" or ")})`;
        }

        //#region Filter providers
        getFacilityFilterOptions() {
            const options = [];

            const user = this.securityService.currentFullUser();
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

        //#region Menu whereclauses
        // `assignment_`.dateindex02 = scheduled date
        getTodayWosWhereClause() {
            const now = new Date();
            const todayTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime();
            const tomorrowTime = todayTime + day;
            return `assignment_.dateindex02 >= ${todayTime} and assignment_.dateindex02 < ${tomorrowTime}`;
        }

        // `assignment_`.dateindex02 = scheduled date
        getPastWosWhereClause() {
            const now = new Date();
            const todayTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime();
            return `assignment_.dateindex02 < ${todayTime}`;
        }

        // `assignment_`.dateindex02 = scheduled date
        getFutureWosWhereClause() {
            const now = new Date();
            const tomorrowTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime() + day;
            return `assignment_.dateindex02 >= ${tomorrowTime}`;
        }

        getCreatedWosWhereClause() {
            return "`root`.remoteId is null";
        }
        //#endregion
    }


    fsWorkorderOfflineService["$inject"] = ["$q", "crudContextService", "swdbDAO", "$timeout", "securityService", "offlineSchemaService", "$log"];

    angular.module("maximo_offlineapplications").service("fsWorkorderOfflineService", fsWorkorderOfflineService);

})(angular, _);
