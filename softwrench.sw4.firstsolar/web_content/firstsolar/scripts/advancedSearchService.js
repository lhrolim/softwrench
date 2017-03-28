
(function (angular) {
    "use strict";

    function advancedSearchService($rootScope, $log, $q, $timeout, restService, crudContextHolderService, searchService, redirectService, alertService, modalService, schemaCacheService) {
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
            const promise = restService.getPromise("FirstSolarAdvancedSearch", controllerMethod, parameters);
            promise.then(function (response) {
                const options = response.data.map(function (dbData) {
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
            const pcs = $(pcsFilterId).val();
            if (!pcs) {
                return true;
            }

            if (!optionValue) {
                return false;
            }
            const tokens = optionValue.split("-");
            return tokens[tokens.length - 1].contains(pcs);
        }

        // filters pcs location by block
        var filterByBlock = function (optionValue) {
            const block = $(blockFilterId).val();
            if (!block) {
                return true;
            }

            if (!optionValue) {
                return false;
            }
            const dashNumber = (optionValue.match(/-/g) || []).length;
            if (dashNumber === 4 || dashNumber === 3) {
                return optionValue.split("-")[2].contains(block);
            }

            return true;
        }

        // searchs for the siteid from location option value
        var findSiteId = function (associationKey, location) {
            const options = crudContextHolderService.fetchEagerAssociationOptions(associationKey, null, searchPanelId);
            if (!options) {
                return null;
            }
            const locationOption = options.find(function (option) {
                return option["value"] === location;
            });
            return locationOption && locationOption.extrafields ? locationOption.extrafields["siteid"] : null;
        }
        //#endregion

        //#region Public methods

        // called when a facility is selected
        // updates the locations of interest and switchgear location options
        //afterchange
        function facilitySelected(event) {
            const facility = event.fields ? event.fields[facilityId] : null;
            const facilities = facility ? facility.split(",") : [];
            const parameters = { facilities: facilities };
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
                filterClearBtn: '<span class="input-group-addon"><i class="fa fa-eraser"></i></span>',

                // copied from the defaults from bootstrap-multiselect line 335
                // it was not merging the templates properly
                button: '<button type="button" class="multiselect dropdown-toggle" data-toggle="dropdown"><span class="multiselect-selected-text"></span> <b class="caret"></b></button>',
                ul: '<ul class="multiselect-container dropdown-menu"></ul>',
                li: '<li><a tabindex="0"><label></label></a></li>',
                divider: '<li class="multiselect-item divider"></li>',
                liGroup: '<li class="multiselect-item multiselect-group"><label></label></li>'
            }

            dropdownOptions["filterFunction"] = function (optionValue, optionText, filterInput) {
                log.debug("filterAvailablePcsLocations - Filtering: {0}".format(optionValue));
                const pcsOk = filterByPcs(optionValue);
                const blockOk = filterByBlock(optionValue);
                const isOk = pcsOk && blockOk;
                log.debug("filterAvailablePcsLocations - {0} filter result is {1}".format(optionValue, isOk));
                return isOk;
            }

            // recalcs scroll pane height - considering the added or removed values
            dropdownOptions["onChange"] = function () {
                if (fieldMetadata.rendererParameters["showvaluesbelow"] === "true") {
                    $timeout(() => {
                        // adds the tooltips on below values
                        $(".multiselect-div [rel=tooltip]").tooltip();

                        $(window).trigger("resize");
                    });
                }
            }

            // calcs the margin top to open the dropdown upward
            dropdownOptions["onDropdownShow"] = function () {
                const multiselectContainer = selectElement.closest(".multiselect-div").find("ul.multiselect-container");
                const containerHeight = multiselectContainer.height();
                multiselectContainer.css({ 'margin-top': "-" + (containerHeight + 34) + "px" });
            }
        }

        function woNoResultsPreAction() {
            const searchDatamap = crudContextHolderService.rootDataMap(searchPanelId);
            const facility = searchDatamap[facilityId];
            const includeSubLocations = searchDatamap["fsincludesubloc"];
            const map = buildLocationAndSiteIdMap();
            if (map.length === 0 && facility) {
                //if none selected, let´s stick with the facility itself
                //TODO: fetch correctsiteid
                map.push({location:facility + "-00", siteid :null});
            }
            if (map.length === 0) {
                //this would only happen if no search at all has been performed, and, hence, there´s no way to fill the initial wo data.
                return $q.when();
            }
            const parameters = {
                includeSubLocations: includeSubLocations,
                locations: map,
            };
            return restService.getPromise("FirstSolarAdvancedSearch", "FindAssetsBySelectedLocations", parameters).then(function (response) {
                var appData = response.data;
                const schema = schemaCacheService.getSchemaFromResult(appData);
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
                        const buffer = crudContextHolderService.getSelectionModel("#modal").selectionBuffer;
                        const keys = Object.keys(buffer);
                        if (keys.length === 1) {
                            selectedAsset = buffer[keys[0]].fields;
                        }
                        const workorderInitialDataMap = {
                            siteid: selectedAsset.siteid,
                            assetnum: selectedAsset.assetnum,
                            location: selectedAsset.location
                        };
                        deferred.resolve(workorderInitialDataMap);
                        modalService.hide();
                    }
                });

                return deferred.promise;
            });

  
        }

        function buildLocationAndSiteIdMap() {
            const searchDatamap = crudContextHolderService.rootDataMap(searchPanelId);


            // tries to get a single location from selected  locations of interest,
            // switchgear locations or pcs locations, if more than one location is
            // found just returns null
            const locsOfInterest = searchDatamap[locOfInterestId] || [];
            const switchgearLocations = searchDatamap[switchgearLocationsId] || [];
            const pcsLocations = searchDatamap[pcsLocationsId] || [];
            var toSearch = {
                "_FsLocationsOfInterest": locsOfInterest,
                "_FsSwitchgearLocations": switchgearLocations,
                "_FsPcsLocations": pcsLocations
            };

            var results = [];

            //concatenating everything into a single array
            Object.keys(toSearch).forEach(function (associationKey) {
                const items = toSearch[associationKey].map(function (location) {
                    const siteId = findSiteId(associationKey, location);
                    return { location: location, siteid: siteId };
                });
                results = results.concat(items);
            });

            return results;

        }

        function newWorkOrder() {
            return woNoResultsPreAction().then(function (datamap) {
                modalService.hide();
                const msg = "Are you sure you want to leave the page?";
                if (crudContextHolderService.getDirty()) {
                    alertService.confirmCancel(msg).then(function () {
                        redirectService.goToApplication("workorder", "newdetail", null, datamap);
                        crudContextHolderService.clearDirty();
                        crudContextHolderService.clearDetailDataResolved();
                    }, function () { return; });
                }
                else {
                    redirectService.goToApplication("workorder", "newdetail", null, datamap);
                }
            });
        }
        //#endregion

        //#region Service Instance
        const service = {
            facilitySelected,
            customizeComboDropdown,
            buildLocationAndSiteIdMap,
            woNoResultsPreAction,
            newWorkOrder
        };
        return service;
        //#endregion
    }

    //#region Service registration


    angular
      .module('firstsolar')
      .clientfactory('advancedSearchService', ["$rootScope", "$log", "$q", "$timeout", "restService", "crudContextHolderService", "searchService", "redirectService", "alertService", "modalService", "schemaCacheService", advancedSearchService]);

    //#endregion

})(angular);
