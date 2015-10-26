var app = angular.module('sw_layout');

app.factory('matusetranService', function ($http, contextService, redirectService, modalService, restService, searchService, alertService, validationService) {

    var doCommodityGroupAssociation = function (parameters) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            itemsetid: parameters['fields']['itemsetid']
        };

        searchService.searchWithData('itemlookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject.length > 0) {
                parameters.fields['commoditygroup'] = resultObject[0].fields['commoditygroup'];
            }
        });
    }

    var doItemBalanceAssociation = function (parameters) {
        var searchData = {
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

        searchService.searchWithData('invbalookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject.length === 1) {
                parameters.fields['binnum'] = resultObject[0].fields['binnum'];
                parameters.fields['lotnum'] = resultObject[0].fields['lotnum'];
                
                parameters.fields['curbal'] = resultObject[0].fields['curbal'];
                parameters.fields['physcnt'] = resultObject[0].fields['physcnt'];
            } else if (resultObject.length <= 0) {
                // no inventory item found, display error message
                alertService.alert('This material is not available for use; please make another selection.');
            }
        });
    }

    var doItemCostAssociation = function (parameters) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            conditioncode: parameters['fields']['conditioncode'],
            siteid: parameters['fields']['siteid'],
            orgid: parameters['fields']['orgid']
        };

        parameters['fields']['unitcost'] = 0.00;

        searchService.searchWithData('invcostlookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject.length === 1) {
                var fields = resultObject[0].fields;
                var costtype = parameters.fields['costtype'];

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
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid']
        };

        searchService.searchWithData('invlookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject.length > 0) {
                var resultMap = resultObject[0].fields;
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

        afteritemchange: function (event) {
            if (event.fields['linetype'].equalIc("sparepart")) {
                event.fields['#description'] = event.fields['sparepart_.description'];
            } else {
                event.fields['#description'] = event.fields['item_.description'];
            }

            doItemLookup(event);
        },

        afterchange: function (event) {
            doItemAssociation(event);
        }
    }
});