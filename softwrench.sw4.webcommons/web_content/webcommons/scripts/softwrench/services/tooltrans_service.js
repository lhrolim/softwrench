(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('tooltranService', function (contextService, searchService) {
    "ngInject";

    var doUpdateToolRateCost = function (parameters, unitCostFieldName) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            itemsetid: parameters['fields']['itemsetid'],
            orgid: parameters['fields']['orgid']
        };
        searchService.searchWithData("itemorginfo", searchData).then(function (response) {
            const data = response.data;
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
    };
});

})(angular);