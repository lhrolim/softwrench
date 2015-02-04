var app = angular.module('sw_layout');

app.factory('matusetranService', function ($http, contextService, redirectService, modalService, restService, searchService, alertService, validationService) {
    var doUpdateItemCosttype = function (parameters) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid']
        };

        searchService.searchWithData('invlookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject[0] != undefined && resultObject.length == 1) {
                parameters.fields['costtype'] = resultObject[0].fields['costtype'];
                doUpdateItemCost(parameters); 
            }
        });
    };

    var doUpdateItemCost = function(parameters) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid']
        };

        searchService.searchWithData('invcostlookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject[0] != undefined && resultObject.length == 1) {
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

    var doUpdateItemBalance = function (parameters) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid'],
            binnum: parameters['fields']['binnum'],
            lotnum: parameters['fields']['lotnum']
        };

        searchService.searchData('invballookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject[0] != undefined && resultObject.length == 1) {
                parameters.fields['curbal'] = resultObject[0].fields['curbal'];
                doUpdateItemCost(parameters);
            }
        })
    }

    return {
        afterlinetypechange: function (event) {
            event.fields["itemnum"] = null;
            event.fields["description"] = "";

            event.fields["storeloc"] = null;
            event.fields["binnum"] = null;
            event.fields["lotnum"] = null;
            event.fields["itemsetid"] = null;
            event.fields["location"] = null;

            event.fields["linecost"] = 0.00;
            event.fields["unitcost"] = 0.00;
            event.fields["curbal"] = 0.00; 
            event.fields["quantity"] = 1;
        },

        afterstorelocchange: function (event) {
            event.fields["unitcost"] = 0.00;
            doUpdateItemCosttype(event);
        },

        afterchange: function (event) {
            doUpdateItemBalance(event);
        }
    }
});