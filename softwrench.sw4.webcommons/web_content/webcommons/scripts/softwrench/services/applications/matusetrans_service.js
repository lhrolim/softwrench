(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('matusetranService', [
        "$http", "contextService", "redirectService", "modalService", "restService", "searchService", "alertService", 
        function ($http, contextService, redirectService, modalService, restService, searchService, alertService) {

    var doCommodityGroupAssociation = function (parameters) {
        const searchData = {
            itemnum: parameters['fields']['itemnum'],
            itemsetid: parameters['fields']['itemsetid']
        };
        searchService.searchWithData('itemlookup', searchData).then(function (response) {
            const data = response.data;
            const resultObject = data.resultObject;
            if (resultObject.length > 0) {
                parameters.fields['commoditygroup'] = resultObject[0]['commoditygroup'];
            }
        });
    }

    var doItemBalanceAssociation = function (parameters) {
        const searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid'],
            orgid: parameters['fields']['orgid'],
            binnum: parameters['fields']['binnum'],
            lotnum: parameters['fields']['lotnum'],
            conditioncode: parameters['fields']['conditioncode']
        };
        parameters['fields']['curbal'] = 0.00;
        parameters['fields']['physcnt'] = 0.00;

        searchService.searchWithData('invbalookup', searchData).then(function (response) {
            const data = response.data;
            const resultObject = data.resultObject;
            if (resultObject.length === 1) {
                parameters.fields['binnum'] = resultObject[0]['binnum'];
                parameters.fields['lotnum'] = resultObject[0]['lotnum'];
                
                parameters.fields['curbal'] = resultObject[0]['curbal'];
                parameters.fields['physcnt'] = resultObject[0]['physcnt'];
            } else if (resultObject.length <= 0) {
                // no inventory item found, display error message
                alertService.alert('This material is not available for use; please make another selection.');
            }
        });
    }

    var doItemCostAssociation = function (parameters) {
        const searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            conditioncode: parameters['fields']['conditioncode'],
            siteid: parameters['fields']['siteid'],
            orgid: parameters['fields']['orgid']
        };
        parameters['fields']['unitcost'] = 0.00;

        return searchService.searchWithData('invcostlookup', searchData).then(function (response) {
            const data = response.data;
            const resultObject = data.resultObject;
            if (resultObject.length === 1) {
                const fields = resultObject[0];
                const costtype = parameters.fields['costtype'];
                if (costtype === 'STANDARD') {
                    parameters.fields['unitcost'] = fields.stdcost;
                } else if (costtype === 'AVERAGE') {
                    parameters.fields['unitcost'] = fields.avgcost;
                } else if (costtype === 'FIFO') {
                    parameters.fields['unitcost'] = fields.lastcost;
                }
            }
        });
    };

    var doItemAssociation = function(parameters) {
        doItemCostAssociation(parameters);
        doItemBalanceAssociation(parameters);
    }

    var doItemLookup = function (parameters) {
        const searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid']
        };
        return searchService.searchWithData('invlookup', searchData).then(function (response) {
            const data = response.data;
            const resultObject = data.resultObject;
            if (resultObject.length > 0) {
                const resultMap = resultObject[0];
                parameters.fields['costtype'] = resultMap['costtype'];
                parameters.fields['itemsetid'] = resultMap['itemsetid'];
                doCommodityGroupAssociation(parameters);
                doItemAssociation(parameters);
            } else {
                // no inventory item found, display error message
                alertService.alert('This material is not available for use; please make another selection.');
            }
        });
    };

    return {
        afterlinetypechange: function (event) {
            event.fields['itemnum'] = null;
            event.fields['description'] = '';

            event.fields['storeloc'] = null;
            event.fields['binnum'] = null;
            event.fields['lotnum'] = null;
            event.fields['itemsetid'] = null;
            event.fields['location'] = null;
            event.fields['conditioncode'] = null;

            event.fields['unitcost'] = 0.00;
            event.fields['curbal'] = 0.00; 
            event.fields['quantity'] = 1;
        },

        //afterchange
        afteritemchange: function (event) {
            const dm = event.fields;
            if (dm['linetype'].equalIc("sparepart")) {
                dm['#description'] = dm['sparepart_.description'];
            } else {
                dm['#description'] = dm['item_.description'];
            }

            doItemLookup(event);
            if (!!dm['storeloc']) {
                //store loc already selected
                doItemAssociation(event);
            }

        },

        //afterchange
        afterchange: function (event) {
            doItemAssociation(event);
        }
    };
}]);

})(angular);