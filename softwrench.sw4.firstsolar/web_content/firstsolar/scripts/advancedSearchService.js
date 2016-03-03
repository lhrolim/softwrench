
(function (angular) {
    "use strict";

    function advancedSearchService(restService, crudContextHolderService, crudSearchService, searchService, alertService, $log) {
        var log = $log.getInstance("sw4.advancedSearchService");

        //#region Utils
        var blockId = "fsblock";
        var pcsId = "fspcs";
        var facilityId = "fsfacility";

        var updateOptions = function(controllerMethod, parameters, associationKey) {
            var promise = restService.getPromise("FirstSolarAdvancedSearch", controllerMethod, parameters);
            promise.then(function (response) {
                var options = response.data.map(function(dbData) {
                    return {
                        "type": "AssociationOption",
                        "value": dbData["location"],
                        "label": dbData["description"] ? dbData["description"] : dbData["location"] + " - No Description"
                    }
                });
                crudContextHolderService.updateEagerAssociationOptions(associationKey, options, null, "search");
            });
        }

        var appendAlert = function (alert, fieldLabel, index) {
            var innerAlert = alert;
            if (innerAlert) {
                innerAlert += ", ";
            }
            innerAlert += "<b>" + fieldLabel + " (" + (index + 1) + ")</b> is required";
            return innerAlert;
        }
        //#endregion

        //#region Public methods

        // called when a facility is selected
        // updates the locations of interest and switchgear location options
        function facilitySelected(event) {
            var facility = event.fields ? event.fields[facilityId] : null;
            var facilities = facility ? facility.split(",") : [];
            var parameters = { facilities: facilities }
            log.debug("Updating locations of interest for facilities: {0}".format(facilities));
            updateOptions("GetLocationsOfInterest", parameters, "_FsLocationsOfInterest");
            log.debug("Updating switchgears for facilities: {0}".format(facilities));
            updateOptions("GetSwitchgearLocations", parameters, "_FsSwitchgearLocations");
        }

        function search(schema, datamap) {
            var pcsDataMap = datamap["#pcs_"];
            if (!pcsDataMap) {
                crudSearchService.search(schema, datamap);
                return;
            }

            // gets the blocks and pcs from each row of composition
            // and build an array of blocks and an array of pcs
            // because the server is not ready to receive compositions as searchdata
            // so thore arrays are added as two new fields on datamp
            var blocks = [];
            var pcss = [];
            var alertMsg = "";
            pcsDataMap.forEach(function (pcsDatamap, index) {
                var block = pcsDatamap[blockId];
                var pcs = pcsDatamap[pcsId];
                if (!block && !pcs) {
                    // if the full row is empty returns
                    return;
                }

                if (block && pcs) {
                    // if the full row is filled the data is added to the arrays
                    blocks.push(pcsDatamap[blockId]);
                    pcss.push(pcsDatamap[pcsId]);
                }

                // otherwise a required allert is added
                if (!block) {
                    alertMsg = appendAlert(alertMsg, "Block", index);
                }
                if (!pcs) {
                    alertMsg = appendAlert(alertMsg, "PCS", index);
                }
            });

            // if alert message is not empty some pcs or blocks are missing - no search is done
            if (alertMsg) {
                alertService.notifymessage("error", alertMsg);
                return;
            }

            // add the arrays to  the datamap and do the search
            var searchOperator = {}
            var eqOperator = searchService.getSearchOperationById("EQ");
            searchOperator[blockId] = eqOperator;
            searchOperator[pcsId] = eqOperator;
            datamap[blockId] = blocks;
            datamap[pcsId] = pcss;

            crudSearchService.search(schema, datamap, searchOperator);
        }
        //#endregion

        //#region Service Instance
        var service = {
            facilitySelected: facilitySelected,
            search: search
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("firstsolar").clientfactory("advancedSearchService", ["restService", "crudContextHolderService", "crudSearchService", "searchService", "alertService", "$log", advancedSearchService]);

    //#endregion

})(angular);