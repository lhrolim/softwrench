(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('relatedrecordService', ["redirectService", "searchService", "crudContextHolderService", function (redirectService, searchService, crudContextHolderService) {
    var getWorkOrderId = function (app, wonum, siteid) {
        var searchData = {
            wonum: wonum,
            siteid: siteid
        };
        searchService.searchWithData("workorder", searchData, "wonumlookup").success(function (data) {
            var resultObject = data.resultObject;
            var workorderid = resultObject[0]['workorderid'];
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
            var params = { userid: key, siteid: siteid };
            params.saveHistoryReturn = true;

            return redirectService.goToApplicationView(swApp, schemaId, "input", null, params);
        },

        saveBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },

        clearSelectedRelatedEntityId: function (event) {
            event.fields["#transient_relatedreckey"] = null;
            event.fields["relatedreckey"] = null;
            event.fields["relatedservicerequest_"] = null;
            event.fields["relatedworkorder_"] = null;
        },

        onAfterRelatedEntitySelected: function(event) {
            var datamap = event.fields;
            var selectedEntityName = datamap["relatedrecclass"];
            var selectedEntity = (function() {
                switch (selectedEntityName) {
                    case "SR":
                        return datamap["relatedservicerequest_"];
                    case "WORKORDER":
                        return datamap["relatedworkorder_"];
                    default:
                        return { siteid: null, orgid: null }
                }
            })();

            datamap["relatedrecsiteid"] = selectedEntity["siteid"];
            datamap["relatedrecorgid"] = selectedEntity["orgid"];
        }

    };

}]);

})(angular);