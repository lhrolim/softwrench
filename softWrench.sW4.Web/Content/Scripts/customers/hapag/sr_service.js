(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('srservice', ["$http", "alertService", "fieldService", function ($http, alertService, fieldService) {

    return {
        //This service is to add the new field ACTION in Service Request details for resolved tickets
        submitaction: function (datamap, schema) {

            if (datamap.customAction == "0") {
                alertService.alert("please select either agree or disagree");

            } else {
                var parameters = {
                    ticketid: fieldService.getId(datamap, schema),
                    status: datamap.customAction,
                    crud: datamap
                }
                var urlToUse = url("api/data/operation/servicerequest/SubmitAction?platform=web&id=" + parameters.ticketid);
                parameters = addCurrentSchemaDataToJson(parameters, schema);
                var json = angular.toJson(parameters);
                $http.post(urlToUse, json).then(function () {
                    datamap.status = datamap.customAction;
                    alertService.alert("Status changed successfully");
                });
            }
        },

        //afterchange
        afterITCAssetChange: function (event) {
            // Clean User-Personal Asset
            if (event['assetnum'] != null) {
                event['assetnum'] = '$null$ignorewatch';
            }
            //event.fields['assetnum'] = null;

        },

        //afterchange
        afterUserAssetChange: function (event) {
            // Clean ITC-Responsible Asset
            if (event['itcassetnum'] != null) {
                event['itcassetnum'] = '$null$ignorewatch';
            }
            //event.fields['itcassetnum'] = null;
        }
    }; 
}]);

})(angular);