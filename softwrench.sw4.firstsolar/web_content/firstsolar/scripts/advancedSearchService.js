
(function (angular) {
    "use strict";

    function advancedSearchService(restService, crudContextHolderService, searchService, $log) {
        var log = $log.getInstance("sw4.advancedSearchService");

        //#region Utils
        var searchPanelId = "search";
        var facilityId = "fsfacility";
        var pcsFilterId = "#fspcs";
        var blockFilterId = "#fsblock";

        var updateOptions = function(controllerMethod, parameters, associationKey) {
            var promise = restService.getPromise("FirstSolarAdvancedSearch", controllerMethod, parameters);
            promise.then(function (response) {
                var options = response.data.map(function(dbData) {
                    return {
                        "type": "AssociationOption",
                        "value": dbData["location"],
                        "label": dbData["description"] ? dbData["description"] : dbData["location"]
                }
                });
                crudContextHolderService.updateEagerAssociationOptions(associationKey, options, null, searchPanelId);
            });
        }

        // filters pcs location by pcs
        var filterByPcs = function (optionValue) {
            var pcs = $(pcsFilterId).val();
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
        var filterByBlock = function (optionValue) {
            var block = $(blockFilterId).val();
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
            updateOptions("GetAvailablePcsLocations", parameters, "_FsPcsLocations");
        }

        function customizeComboDropdown(fieldMetadata, selectElement, dropdownOptions) {
            dropdownOptions["templates"] = {
                filter : '<li class="multiselect-item filter">' +
                '<div class="input-group"><span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>' +
                '<input id="fsblock" class="form-control multiselect-search" placeholder="Block" type="text">' +
                '<input id="fspcs" class="form-control multiselect-search" placeholder="PCS" type="text"></div></li>'
            }

            dropdownOptions["filterFunction"] = function(optionValue, optionText, filterInput) {
                log.debug("filterAvailablePcsLocations - Filtering: {0}".format(optionValue));
                var pcsOk = filterByPcs(optionValue);
                var blockOk = filterByBlock(optionValue);
                var isOk = pcsOk && blockOk;
                log.debug("filterAvailablePcsLocations - {0} filter result is {1}".format(optionValue, isOk));
                return isOk;
            }

            // recalcs scroll pane height - considering the added or removed values
            dropdownOptions["onChange"] = function () {
                if (fieldMetadata.rendererParameters["showvaluesbelow"] === "true") {
                    // adds the tooltips on below values
                    $(".multiselect-div [rel=tooltip]").tooltip();
                }
                $(window).trigger("resize");
            }

            // calcs the margin top to open the dropdown upward
            dropdownOptions["onDropdownShow"] = function () {
                var multiselectContainer = selectElement.closest(".multiselect-div").find("ul.multiselect-container");
                var containerHeight = multiselectContainer.height();
                multiselectContainer.css({ 'margin-top': "-" + (containerHeight + 34) + "px" });
            }
        }
        //#endregion

        //#region Service Instance
        var service = {
            facilitySelected: facilitySelected,
            customizeComboDropdown: customizeComboDropdown
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("advancedSearchService", ["restService", "crudContextHolderService", "searchService", "$log", advancedSearchService]);

    //#endregion

})(angular);