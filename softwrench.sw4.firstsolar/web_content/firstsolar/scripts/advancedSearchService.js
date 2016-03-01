
(function (angular) {
    "use strict";

    function advancedSearchService(restService, crudContextHolderService, $rootScope, searchService, alertService) {
        //#region Utils
        var blockId = "fsblock";
        var pcsId = "fspcs";
        var facilityId = "fsfacility";

        var toOption = function(dbData, optionsArray) {
            var option = {
                "type": "AssociationOption",
                "value": dbData["location"],
                "label": dbData["description"] ? dbData["description"] : dbData["location"] + " - No Description"
            }
            optionsArray.push(option);
        }

        var updateOptions = function(controllerMethod, parameters, associationKey) {
            var promise = restService.getPromise("FirstSolarAdvancedSearch", controllerMethod, parameters);
            promise.then(function (response) {
                var options = [];
                response.data.forEach(function(dbData) {
                    toOption(dbData, options);
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
            updateOptions("GetLocationsOfInterest", parameters, "_FsLocationsOfInterest");
            updateOptions("GetSwitchgearLocations", parameters, "_FsSwitchgearLocations");
        }

        function search(schema, datamap) {
            var pcsDataMap = datamap["#pcs_"];
            if (!pcsDataMap) {
                $rootScope.$broadcast("sw.crud.search", [schema, datamap]);
                return;
            }

            var blocks = [];
            var pcss = [];
            var alertMsg = "";
            pcsDataMap.forEach(function (pcsDatamap, index) {
                console.log(index);
                var block = pcsDatamap[blockId];
                var pcs = pcsDatamap[pcsId];
                if (!block && !pcs) {
                    return;
                }

                if (block && pcs) {
                    blocks.push(pcsDatamap[pcs]);
                    pcss.push(pcsDatamap[blockId]);
                }

                if (!block) {
                    alertMsg = appendAlert(alertMsg, "Block", index);
                }
                if (!pcs) {
                    alertMsg = appendAlert(alertMsg, "PCS", index);
                }
            });
            if (alertMsg) {
                alertService.notifymessage("error", alertMsg);
                return;
            }

            var searchOperator = {}
            var eqOperator = searchService.getSearchOperationById("EQ");
            searchOperator[blockId] = eqOperator;
            searchOperator[pcsId] = eqOperator;
            datamap[blockId] = blocks;
            datamap[pcsId] = pcss;


            $rootScope.$broadcast("sw.crud.search", [schema, datamap, searchOperator]);
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

    angular.module("firstsolar").clientfactory("advancedSearchService", ["restService", "crudContextHolderService", "$rootScope", "searchService", "alertService", advancedSearchService]);

    //#endregion

})(angular);