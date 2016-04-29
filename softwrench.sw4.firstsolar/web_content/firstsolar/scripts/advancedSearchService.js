
(function (angular) {
    "use strict";

    function advancedSearchService($rootScope, $log, $q, restService, crudContextHolderService, searchService, redirectService, alertService, modalService, schemaCacheService) {
        var log = $log.getInstance("sw4.advancedSearchService");

        //#region Utils
        var searchPanelId = "search";
        var facilityId = "fsfacility";
        var locOfInterestId = "fslocint";
        var switchgearLocationsId = "fsswitchgear";
        var pcsLocationsId = "fspcslocations";
        var pcsFilterId = "#fspcs";
        var blockFilterId = "#fsblock";

        var updateOptions = function (controllerMethod, parameters, associationKey) {
            var promise = restService.getPromise("FirstSolarAdvancedSearch", controllerMethod, parameters);
            promise.then(function (response) {
                var options = response.data.map(function (dbData) {
                    return {
                        "type": "MultiValueAssociationOption",
                        "value": dbData["location"],
                        "label": dbData["description"] ? dbData["description"] : dbData["location"],
                        "extrafields": {
                            "siteid": dbData["siteid"]
                        }
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

        // searchs for the siteid from location option value
        var findSiteId = function (associationKey, location) {
            var options = crudContextHolderService.fetchEagerAssociationOptions(associationKey, null, searchPanelId);
            if (!options) {
                return null;
            }
            var locationOption = options.find(function (option) {
                return option["value"] === location;
            });
            return locationOption && locationOption.extrafields ? locationOption.extrafields["siteid"] : null;
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
                filter: '<li class="multiselect-item filter">' +
                '<div class="input-group"><span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>' +
                '<input id="fsblock" class="form-control multiselect-search" placeholder="Block" type="text">' +
                '<input id="fspcs" class="form-control multiselect-search" placeholder="PCS" type="text" style="margin-top: -1px"></div></li>',
                filterClearBtn: '<span class="input-group-addon"><i class="fa fa-eraser"></i></span>'
            }

            dropdownOptions["filterFunction"] = function (optionValue, optionText, filterInput) {
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

        function woNoResultsPreAction() {
            var searchDatamap = crudContextHolderService.rootDataMap(searchPanelId);
            var facility = searchDatamap[facilityId];
            var includeSubLocations = searchDatamap["fsincludesubloc"];
            var map = buildLocationAndSiteIdMap();
            if (map.length === 0 && facility) {
                //if none selected, let´s stick with the facility itself
                //TODO: fetch correctsiteid
                map.push({location:facility + "-00", siteid :null});
            }
            if (map.length === 0) {
                //this would only happen if no search at all has been performed, and, hence, there´s no way to fill the initial wo data.
                return $q.when();
            }


            var parameters = {
                includeSubLocations: includeSubLocations,
                locations: map,
            }


            return restService.getPromise("FirstSolarAdvancedSearch", "FindAssetsBySelectedLocations", parameters).then(function (response) {
                var appData = response.data;
                var schema = schemaCacheService.getSchemaFromResult(appData);


                if (appData.resultObject.length === 0) {
                    alertService.alert("No asset could be found at the selected locations. Cannot create a workorder");
                    return $q.reject();
                }

                var deferred = $q.defer();

                modalService.show(schema, appData, {
                    title: "Select an Asset",
                    onloadfn: function () {
                        crudContextHolderService.setFixedWhereClause("#modal", appData.filterFixedWhereClause);
                    },
                    savefn: function () {

                        var selectedAsset = {};

                        var buffer = crudContextHolderService.getSelectionModel("#modal").selectionBuffer;
                        var keys = Object.keys(buffer);
                        if (keys.length === 1) {
                            selectedAsset = buffer[keys[0]].fields;
                        }

                        var workorderInitialDataMap = {
                            siteid: selectedAsset.siteid,
                            assetnum: selectedAsset.assetnum,
                            location: selectedAsset.location
                        }

                        deferred.resolve(workorderInitialDataMap);
                        modalService.hide();
                    }
                });

                return deferred.promise;
            });

  
        }

        function buildLocationAndSiteIdMap() {
            var location = null;
            var associationKey = null;
            var searchDatamap = crudContextHolderService.rootDataMap(searchPanelId);


            // tries to get a single location from selected  locations of interest,
            // switchgear locations or pcs locations, if more than one location is
            // found just returns null
            var locsOfInterest = searchDatamap[locOfInterestId] || [];
            var switchgearLocations = searchDatamap[switchgearLocationsId] || [];
            var pcsLocations = searchDatamap[pcsLocationsId] || [];

            var toSearch = {
                "_FsLocationsOfInterest": locsOfInterest,
                "_FsSwitchgearLocations": switchgearLocations,
                "_FsPcsLocations": pcsLocations
            };

            var results = [];

            //concatenating everything into a single array
            Object.keys(toSearch).forEach(function (associationKey) {
                var items = toSearch[associationKey].map(function (location) {
                    var siteId = findSiteId(associationKey, location);
                    return { location: location, siteid: siteId };
                });
                results = results.concat(items);
            });

            return results;

        }

        function newWorkOrder() {
            return woNoResultsPreAction().then(function (datamap) {
                modalService.hide();
                var msg = "Are you sure you want to leave the page?";
                if (crudContextHolderService.getDirty()) {
                    alertService.confirmCancel(null, null, function () {
                        redirectService.goToApplication("workorder", "newdetail", null, datamap);
                        crudContextHolderService.clearDirty();
                        crudContextHolderService.clearDetailDataResolved();
                    }, msg, function () { return; });
                }
                else {
                    redirectService.goToApplication("workorder", "newdetail", null, datamap);
                }
            });
        }
        //#endregion

        //#region Service Instance
        var service = {
            facilitySelected: facilitySelected,
            customizeComboDropdown: customizeComboDropdown,
            buildLocationAndSiteIdMap: buildLocationAndSiteIdMap,
            woNoResultsPreAction: woNoResultsPreAction,
            newWorkOrder: newWorkOrder
        };
        return service;
        //#endregion
    }

    //#region Service registration


    angular
      .module('firstsolar')
      .clientfactory('advancedSearchService', ["$rootScope", "$log", "$q", "restService", "crudContextHolderService", "searchService", "redirectService", "alertService", "modalService", "schemaCacheService", advancedSearchService]);

    //#endregion

})(angular);
