var app = angular.module('sw_layout');

app.factory('matusetranService', function ($http, contextService, redirectService, modalService, restService, searchService, alertService, validationService) {
    var doUpdateUnitCostFromInventoryCost = function (parameters, unitCostFieldName) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid']
        };
        searchService.searchWithData("invcost", searchData).success(function (data) {
            var resultObject = data.resultObject;
            if (resultObject[0] != undefined) {
                var fields = resultObject[0].fields;
                var costtype = parameters['fields']['inventory_.costtype'];
                if (costtype === 'STANDARD') {
                    parameters.fields[unitCostFieldName] = fields.stdcost;
                } else if (costtype === 'AVERAGE') {
                    parameters.fields[unitCostFieldName] = fields.avgcost;
                } else if (costtype === 'FIFO') {
                    parameters.fields[unitCostFieldName] = fields.lastcost;
                }
            }
            // Output notification that there's no associated cost with this item
            if (parameters.fields[unitCostFieldName] == 0) {
                alertService.alert("This item has no cost associated with it.");
            }
        });
    };

    return {
        afterlinetypechange: function (event) {
            if (event.fields["linetype"] == 'ITEM') {
                event.fields["itemnum"] = null;
                event.fields["inventory_"] = null; 
            }

            event.fields["description"] = "";
            event.fields["storeloc"] = null;
            event.fields["linecost"] = 0.00;
            event.fields["unitcost"] = 0.00;
            event.fields["quantity"] = 1;
        },
        afteritemnumchange: function (event) {
            event.fields["storeloc"] = event.fields["inventory_.location"];
            event.fields["binnum"] = event.fields["inventory_.binnum"];
            event.fields["description"] = event.fields["inventory_.item_.description"];
            event.fields["itemsetid"] = event.fields["inventory_.itemsetid"];
            event.fields["siteid"] = event.fields["inventory_.siteid"];
            event.fields["curbal"] = event.fields["inventory_.invbalances_.curbal"];
            event.fields["physcnt"] = event.fields["inventory_.invbalances_.physcnt"];
            event.fields["unitcost"] = 0.00;

            doUpdateUnitCostFromInventoryCost(event, "unitcost");
        }
    }
});