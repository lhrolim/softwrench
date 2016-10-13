(function (angular) {
    'use strict';

    angular.module('sw_layout').factory('inventoryServiceCommons', ['searchService', 'alertService', '$q', inventoryServiceCommons]);

    //TODO: remove fields entirely

    function inventoryServiceCommons(searchService, alertService, $q) {
        const service = {
            updateInventoryCosttype,
            doUpdateUnitCostFromInventoryCost,
            returnTransformation,
            returnConfirmation
        };
        return service;

        function doUpdateUnitCostFromInventoryCost(parameters, unitCostFieldName, locationFieldName) {
            var fields = parameters['fields'];
            const searchData = {
                itemnum: fields['itemnum'],
                location: fields[locationFieldName],
                siteid: fields['siteid']
            };
            return searchService.searchWithData("invcost", searchData).then(function (response) {
                const resultObject = response.data.resultObject;
                const resultFields = resultObject[0];
                const costtype = fields['inventory_.costtype'];
                if (costtype === 'STANDARD') {
                    fields[unitCostFieldName] = resultFields.stdcost;
                } else if (costtype === 'AVERAGE') {
                    fields[unitCostFieldName] = resultFields.avgcost;
                }
                return parameters;
            });
        };

        function updateInventoryCosttype(parameters, storelocation) {
            const fields = parameters['fields'];

            const searchData = {
                itemnum: fields['itemnum'],
                location: fields[storelocation],
                siteid: fields['siteid'],
                orgid: fields['orgid'],
                itemsetid: fields['itemsetid']
            };
            var that = this;

            //TODO: rebuild this method
            return searchService.searchWithData("inventory", searchData).then(function (response) {
                const resultObject = response.data.resultObject[0];
                const costtype = resultObject['costtype'];
                if (costtype.equalsAny("fifo","lifo")) {
                    // TODO: Add support for FIFO / LIFO cost types
                    alertService.error("FIFO and LIFO cost types are not supported at this time");
                    return $q.when(parameters);
                }
                fields['inventory_.costtype'] = costtype;
                return that.doUpdateUnitCostFromInventoryCost(parameters, "unitcost", storelocation);
            });
        };

        function returnTransformation(event, datamap) {
            datamap['issueid'] = datamap['matusetransid'];
            datamap['matusetransid'] = null;
            datamap['rowstamp'] = null;
            datamap['quantity'] = datamap['#quantityadj'];
            datamap['issuetype'] = 'RETURN';
            datamap['qtyreturned'] = null;
            datamap['qtyrequested'] = datamap['#quantityadj'];
        };


        function returnConfirmation (event, datamap, parameters) {
            const returnQty = datamap['#quantityadj'];
            const item = datamap['itemnum'];
            const storeloc = datamap['storeloc'];
            const binnum = datamap['binnum'];
            var message = "Return (" + returnQty + ") " + item + " to " + storeloc + "?";
            if (binnum != null) {
                message = message + " (Bin: " + binnum + ")";
            }
            return alertService.confirm(message).then(function () {
                parameters.continue();
            });
        };

    }
})(angular);
