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

    var doUpdateItemCost = function (parameters) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid']
        };

        searchService.searchWithData('invcostlookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject.length == 1) {
                var fields = resultObject[0].fields;
                var costtype = parameters.fields['costtype'];

                if (costtype === 'STANDARD') {
                    parameters.fields['unitcost'] = fields.stdcost;
                } else if (costtype === 'AVERAGE') {
                    parameters.fields['unitcost'] = fields.avgcost;
                } else if (costtype === 'FIFO') {
                    parameters.fields['unitcost'] = fields.lastcost;
                }

                parameters.fields['conditioncode'] = fields.conditioncode;
                doUpdateItemBalance(parameters);
            }
            else {
                for (index = 0; index < resultObject.length; index++) {
                    var fields = resultObject[index].fields;

                    if (fields.condrate == 100) {
                        var costtype = parameters.fields['costtype'];

                        if (costtype === 'STANDARD') {
                            parameters.fields['unitcost'] = fields.stdcost;
                        } else if (costtype === 'AVERAGE') {
                            parameters.fields['unitcost'] = fields.avgcost;
                        } else if (costtype === 'FIFO') {
                            parameters.fields['unitcost'] = fields.lastcost;
                        }

                        parameters.fields['conditioncode'] = fields.conditioncode;
                        doUpdateItemBalance(parameters);
                    }
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
            lotnum: parameters['fields']['lotnum'],
            conditioncode: parameters['fields']['conditioncode']
        };

        searchService.searchWithData('invbalookup', searchData).success(function (data) {
            var resultObject = data.resultObject;

            if (resultObject[0] != undefined) {
                parameters.fields['curbal'] = resultObject[0].fields['curbal'];
                parameters.fields['binnum'] = resultObject[0].fields['binnum'];
                parameters.fields['lotnum'] = resultObject[0].fields['lotnum'];
            }
            else {
                parameters.fields['curbal'] = 0.00;
            }
        });
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

        afteritemchange: function (event) {
            event.fields["#description"] = event.fields["item_.description"];

            event.fields["linecost"] = 0.00;
            event.fields["unitcost"] = 0.00;
            event.fields["curbal"] = 0.00;
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