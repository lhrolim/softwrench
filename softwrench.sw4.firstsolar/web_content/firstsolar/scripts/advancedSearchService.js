
(function (angular) {
    "use strict";

    function advancedSearchService(restService, crudContextHolderService, $rootScope, searchService) {
        //#region Utils
        var locOfIntId = "fslocint";
        var switchgearId = "fsswitchgear";
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

        function hasLocationsOfInterest(datamap) {
            return datamap[locOfIntId] && datamap[locOfIntId].length > 0;
        }

        function hasSwitchGear(datamap) {
            return datamap[switchgearId] && datamap[switchgearId].length > 0;
        }

        function hasPcs(datamap) {
            return hasPcsOrBlock(datamap, pcsId);
        }

        function hasBlock(datamap) {
            return hasPcsOrBlock(datamap, blockId);
        }

        function hasPcsOrBlock(datamap, id) {
            var pcsDatamap = datamap["#pcs_"];
            var hasDatamap = pcsDatamap && pcsDatamap.length > 0;
            var parameters = datamap[id];
            var hasParameter = parameters && parameters.length > 0;
            if (!hasDatamap && !hasParameter) {
                return false;
            }

            var firstParameter;
            if (hasDatamap) {
                firstParameter = pcsDatamap.find(function(parameterRow) {
                    return parameterRow[id] && true;
                });
            } else {
                if (typeof parameters === "string") {
                    return true;
                }
                firstParameter = parameters.find(function (parameter) {
                    return parameter && true;
                });
            }
            return firstParameter !== undefined;
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

        function isLocationsOfInterestRequired(datamap) {
            return !hasSwitchGear(datamap) && !hasPcs(datamap) && !hasBlock(datamap);
        }

        function isSwitchgearLocationsRequired(datamap) {
            return !hasLocationsOfInterest(datamap) && !hasPcs(datamap) && !hasBlock(datamap);
        }

        function isBlockRequired(datamap) {
            return (!hasSwitchGear(datamap) && !hasLocationsOfInterest(datamap)) || hasPcs(datamap);
        }

        function isPcsRequired(datamap) {
            return (!hasSwitchGear(datamap) && !hasLocationsOfInterest(datamap)) || hasBlock(datamap);
        }

        function search(schema, datamap) {
            var pcsDataMap = datamap["#pcs_"];
            if (!pcsDataMap) {
                $rootScope.$broadcast("sw.crud.search", [schema, datamap]);
                return;
            }

            var blocks = [];
            var pcs = [];
            pcsDataMap.forEach(function(pcsDatamap) {
                blocks.push(pcsDatamap[blockId]);
                pcs.push(pcsDatamap[pcsId]);
            });
            var searchOperator = {}
            var eqOperator = searchService.getSearchOperationById("EQ");
            searchOperator[blockId] = eqOperator;
            searchOperator[pcsId] = eqOperator;
            datamap[blockId] = blocks;
            datamap[pcsId] = pcs;


            $rootScope.$broadcast("sw.crud.search", [schema, datamap, searchOperator]);
        }
        //#endregion

        //#region Service Instance
        var service = {
            facilitySelected: facilitySelected,
            isLocationsOfInterestRequired: isLocationsOfInterestRequired,
            isSwitchgearLocationsRequired: isSwitchgearLocationsRequired,
            isBlockRequired: isBlockRequired,
            isPcsRequired: isPcsRequired,
            search: search
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("firstsolar").clientfactory("advancedSearchService", ["restService", "crudContextHolderService", "$rootScope", "searchService", advancedSearchService]);

    //#endregion

})(angular);