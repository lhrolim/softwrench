var app = angular.module('sw_layout');

app.factory('srnetworkservice', function ($http, alertService, fieldService, $rootScope,contextService) {

    "ngInject";

    return {
        //This service is to add the new field ACTION in Service Request details for resolved tickets
     

        onload: function (datamap,schema) {
            var comp = $("[data-id='#crosstestresult']");
            comp.val("try same user at different PC \ntry other user at same PC");

        },

      
    }; 
});