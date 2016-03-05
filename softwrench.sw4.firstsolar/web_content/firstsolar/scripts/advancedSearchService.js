
(function (angular) {
    "use strict";

    function advancedSearchService(restService, crudContextHolderService, crudSearchService, searchService, $log) {
        var log = $log.getInstance("sw4.advancedSearchService");

        //#region Utils
        var searchPanelId = "search";
        var facilityId = "fsfacility";
        var availablePcsId = "fspcsavailable";
        var selectedPcsId = "fspcsselected";
        var pcsFilterId = "fspcs";
        var blockFilterId = "fsblock";

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
                crudContextHolderService.updateEagerAssociationOptions(associationKey, options, null, searchPanelId);
            });
        }

        // filters pcs location by pcs
        var filterByPcs = function (datamap, optionValue) {
            var pcs = datamap[pcsFilterId];
            if (!pcs) {
                return true;
            }

            if (!optionValue) {
                return false;
            }

            var tokens = optionValue.split("-");
            return tokens[tokens.length - 1].contains(pcs);
        }

        // filters pcs location by block
        var filterByBlock = function (datamap, optionValue) {
            var block = datamap[blockFilterId];
            if (!block) {
                return true;
            }

            if (!optionValue) {
                return false;
            }

            var dashNumber = (optionValue.match(/-/g) || []).length;
            if (dashNumber === 4 || dashNumber === 3) {
                return optionValue.split("-")[2].contains(block);
            }

            return true;
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
            log.debug("Updating available pcs locations for facilities: {0}".format(facilities));
            updateOptions("GetAvailablePcsLocations", parameters, "_FsAvailablePcsLocations");
        }

        function search(schema, datamap) {
            log.debug("search - Searching ...");
            var selectedOptions = crudContextHolderService.fetchEagerAssociationOptions("_FsSelectedPcsLocations", null, searchPanelId) || [];
            if (selectedOptions.length === 0) {
                log.debug("search - No pcs options selected.");
                crudSearchService.search(schema, datamap);
                return;
            }

            // get all selected pcs options and adds to the datamap as a array of values
            var selectedValues = [];
            var eqOperator = searchService.getSearchOperationById("EQ");

            selectedOptions.forEach(function(selectedOption) {
                selectedValues.push(selectedOption.value);
            });

            var searchOperator = {}
            searchOperator[selectedPcsId] = eqOperator;
            datamap[selectedPcsId] = selectedValues;
            crudSearchService.search(schema, datamap, searchOperator);
        }

        // adds pcs location options from available list to the selected one
        function addPcsLocations(datamap) {
            log.debug("addPcsLocations - Adding pcs location options from available list to the selected one.");
            var availablePcsSelected = datamap[availablePcsId];
            if (!availablePcsSelected || availablePcsSelected.length === 0) {
                log.debug("addPcsLocations - No pcs locations selected.");
                return;
            }
            var availableOptions = crudContextHolderService.fetchEagerAssociationOptions("_FsAvailablePcsLocations", null, searchPanelId);
            var selectedOptions = crudContextHolderService.fetchEagerAssociationOptions("_FsSelectedPcsLocations", null, searchPanelId) || [];
            
            availablePcsSelected.forEach(function (selectedLocationValue) {
                // if option already selected just continues.
                var alreadySelectedOption = selectedOptions.find(function (option) { return option.value === selectedLocationValue });
                if (alreadySelectedOption) {
                    return;
                }

                // gets the available option and adds to the selected 
                var selectedOption = availableOptions.find(function (option) { return option.value === selectedLocationValue });
                selectedOptions.push(selectedOption);
            });
            crudContextHolderService.updateEagerAssociationOptions("_FsSelectedPcsLocations", selectedOptions, null, searchPanelId);
        }

        // removes pcs locations from the selected list
        function removePcsLocations(datamap) {
            log.debug("removePcsLocations - Removing pcs location options from selected list.");
            var selectedPcsSelected = datamap[selectedPcsId];
            if (!selectedPcsSelected || selectedPcsSelected.length === 0) {
                log.debug("removePcsLocations - No pcs locations selected.");
                return;
            }
            var selectedOptions = crudContextHolderService.fetchEagerAssociationOptions("_FsSelectedPcsLocations", null, searchPanelId) || [];

            selectedPcsSelected.forEach(function (selectedPcsValue) {
                selectedOptions.forEach(function(selectedPcsOption, index, array) {
                    if (selectedPcsOption.value === selectedPcsValue) {
                        array.splice(index, 1);
                    }
                });
            });
            crudContextHolderService.updateEagerAssociationOptions("_FsSelectedPcsLocations", selectedOptions, null, searchPanelId);
        }

        // clears the pcs locations options selected list
        function clearPcsLocations() {
            log.debug("clearPcsLocations - Clearing selected pcs location options from selected list.");
            crudContextHolderService.updateEagerAssociationOptions("_FsSelectedPcsLocations", [], null, searchPanelId);
        }

        function filterAvailablePcsLocations(option) {
            log.debug("filterAvailablePcsLocations - Filtering: {0}".format(option.value));
            var datamap = crudContextHolderService.rootDataMap(searchPanelId);
            var pcsOk = filterByPcs(datamap, option.value);
            var blockOk = filterByBlock(datamap, option.value);
            var isOk = pcsOk && blockOk;
            log.debug("filterAvailablePcsLocations - {0} filter result is {1}".format(option.value, isOk));
            return isOk;
        }
        //#endregion

        //#region Service Instance
        var service = {
            facilitySelected: facilitySelected,
            search: search,
            addPcsLocations: addPcsLocations,
            removePcsLocations: removePcsLocations,
            clearPcsLocations: clearPcsLocations,
            filterAvailablePcsLocations: filterAvailablePcsLocations
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("advancedSearchService", ["restService", "crudContextHolderService", "crudSearchService", "searchService", "$log", advancedSearchService]);

    //#endregion

})(angular);