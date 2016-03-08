(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('relatedrecordService', ["redirectService", "searchService", function (redirectService, searchService) {
    var getWorkOrderId = function (app, wonum, siteid) {
        var searchData = {
            wonum: wonum,
            siteid: siteid
        };
        searchService.searchWithData("workorder", searchData, "wonumlookup").success(function (data) {
            var resultObject = data.resultObject;
            var workorderid = resultObject[0]['fields']['workorderid'];
            redirectService.goToApplicationView(app, "editdetail", "input", null, { id: workorderid });
        });
    };

    function appName(app) {
        if (app.equalsIc("sr")) {
            return "servicerequest";
        } else if (app.equalsIc("incident")) {
            return "incident";
        }
        return app;
    }

    return {

        open: function (datamap, columnmetadata) {
            var app = datamap["relatedrecclass"];
            var key = datamap["relatedreckey"];
            var siteid = datamap["siteid"];

            var swApp = appName(app);
            var schemaId = app.equalsAny("SR", "WORKORDER", "INCIDENT") ? "editdetail" : "detail";
            var params = swApp.equalsIc("incident") ? { id: key, siteid: siteid } : { userid: key, siteid: siteid };

            return redirectService.goToApplicationView(swApp, schemaId, "input", null, params);
        },

        saveBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },

    };

}]);

})(angular);