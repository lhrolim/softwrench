(function () {
    'use strict';

    angular.module('sw_layout').factory('inventoryServiceCommons', ['searchService', 'alertService', inventoryServiceCommons]);

    function inventoryServiceCommons(searchService, alertService) {

        var service = {
            updateInventoryCosttype: updateInventoryCosttype,
            doUpdateUnitCostFromInventoryCost: doUpdateUnitCostFromInventoryCost,
            returnTransformation: returnTransformation,
            returnConfirmation: returnConfirmation
        };

        return service;

        function doUpdateUnitCostFromInventoryCost(parameters, unitCostFieldName, locationFieldName) {
            var fields = parameters['fields'];
            var searchData = {
                itemnum: fields['itemnum'],
                location: fields[locationFieldName],
                siteid: fields['siteid']
            };
            searchService.searchWithData("invcost", searchData).success(function (data) {
                var resultObject = data.resultObject;
                var resultFields = resultObject[0].fields;
                var costtype = fields['inventory_.costtype'];
                if (costtype === 'STANDARD') {
                    parameters.fields[unitCostFieldName] = resultFields.stdcost;
                } else if (costtype === 'AVERAGE') {
                    parameters.fields[unitCostFieldName] = resultFields.avgcost;
                }
            });
        };

        function updateInventoryCosttype(parameters, storelocation) {
            var searchData = {
                itemnum: parameters['fields']['itemnum'],
                location: parameters['fields'][storelocation],
                siteid: parameters['fields']['siteid'],
                orgid: parameters['fields']['orgid'],
                itemsetid: parameters['fields']['itemsetid']
            };
            var that = this;
            searchService.searchWithData("inventory", searchData).success(function (data) {
                var resultObject = data.resultObject;
                var fields = resultObject[0].fields;
                var costtype = fields['costtype'];
                if (costtype.equalIc("fifo") || costtype.equalIc("lifo")) {
                    // TODO: Add support for FIFO / LIFO cost types
                    alertService.error("FIFO and LIFO cost types are not supported at this time");
                    return;
                }
                parameters['fields']['inventory_.costtype'] = costtype;
                that.doUpdateUnitCostFromInventoryCost(parameters, "unitcost", storelocation);
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
            var returnQty = datamap['#quantityadj'];
            var item = datamap['itemnum'];
            var storeloc = datamap['storeloc'];
            var binnum = datamap['binnum'];
            var message = "Return (" + returnQty + ") " + item + " to " + storeloc + "?";
            if (binnum != null) {
                message = message + " (Bin: " + binnum + ")";
            }
            return alertService.confirm(message).then(function () {
                parameters.continue();
            });
        };

    }
})();
