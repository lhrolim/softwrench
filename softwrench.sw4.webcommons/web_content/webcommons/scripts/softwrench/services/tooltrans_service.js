var app = angular.module('sw_layout');

app.factory('tooltranService', function ($http, contextService, redirectService, modalService, restService, searchService, alertService, validationService) {
    var doUpdateToolRateCost = function (parameters, unitCostFieldName) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            itemsetid: parameters['fields']['itemsetid'],
            orgid: parameters['fields']['orgid']
        };
        searchService.searchWithData("itemorginfo", searchData).success(function (data) {
            var resultObject = data.resultObject;
            if (resultObject != null) {
                parameters.fields[unitCostFieldName] = resultObject[0].fields["toolrate"];
            }
        });
    };
    
    return {
        aftertoolchange: function (event) {
            event.fields['itemsetid'] = event.fields["tool_.itemsetid"];

            var user = contextService.getUserData();
            event.fields['orgid'] = user.orgId;
            
            doUpdateToolRateCost(event, "toolrate");
        }
    }
});