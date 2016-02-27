
(function (angular) {
    "use strict";

    function advancedSearchService(restService, crudContextHolderService) {
        //#region Utils
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

        function hasLocationsOfInterest(datamap) {
            return datamap["fs_locint"] && datamap["fs_locint"].length > 0;
        }

        function hasSwitchGear(datamap) {
            return datamap["fs_switchgear"] && datamap["fs_switchgear"].length > 0;
        }
        //#endregion

        //#region Public methods

        // called when a facility is selected
        // updates the locations of interest and switchgear location options
        function facilitySelected(event) {
            var facility = event.fields ? event.fields["fs_facility"] : null;
            var parameters = { facility: facility }
            updateOptions("GetLocationsOfInterest", parameters, "_FsLocationsOfInterest");
            updateOptions("GetSwitchgearLocations", parameters, "_FsSwitchgearLocations");
        }

        function isLocationsOfInterestRequired(datamap) {
            return !hasSwitchGear(datamap) && !datamap["fs_pcs"] && !datamap["fs_block"];
        }

        function isSwitchgearLocationsRequired(datamap) {
            return !hasLocationsOfInterest(datamap) && !datamap["fs_pcs"] && !datamap["fs_block"];
        }

        function isBlockRequired(datamap) {
            return (!hasSwitchGear(datamap) && !hasLocationsOfInterest(datamap)) || datamap["fs_pcs"];
        }

        function isPcsRequired(datamap) {
            return (!hasSwitchGear(datamap) && !hasLocationsOfInterest(datamap)) || datamap["fs_block"];
        }
        //#endregion

        //#region Service Instance
        var service = {
            facilitySelected: facilitySelected,
            isLocationsOfInterestRequired: isLocationsOfInterestRequired,
            isSwitchgearLocationsRequired: isSwitchgearLocationsRequired,
            isBlockRequired: isBlockRequired,
            isPcsRequired: isPcsRequired
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("firstsolar").clientfactory("advancedSearchService", ["restService", "crudContextHolderService", advancedSearchService]);

    //#endregion

})(angular);