(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('relatedrecordService', ["$rootScope", "redirectService", "searchService", "crudContextHolderService", function ($rootScope, redirectService, searchService, crudContextHolderService) {
        const getWorkOrderId = function (app, wonum, siteid) {
            const searchData = {
                wonum: wonum,
                siteid: siteid
            };
            searchService.searchWithData("workorder", searchData, "wonumlookup").then(function (response) {
                const data = response.data;
                const resultObject = data.resultObject;
                const workorderid = resultObject[0]['workorderid'];
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
            const app = datamap["relatedrecclass"];
            const key = datamap["relatedreckey"];
            const siteid = datamap["siteid"];
            const swApp = appName(app);
            const schemaId = app.equalsAny("SR", "WORKORDER", "INCIDENT") ? "editdetail" : "detail";
            const params = { userid: key, siteid: siteid };
            params.saveHistoryReturn = true;

            return redirectService.goToApplicationView(swApp, schemaId, "input", null, params);
        },

        saveBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },

        //afterchange
        clearSelectedRelatedEntityId: function (event) {
            const fields = event.fields;
            fields["#transient_relatedreckey"] = null;
            fields["relatedreckey"] = null;
            fields["relatedservicerequest_"] = null;
            fields["relatedworkorder_"] = null;

            $rootScope.$broadcast("sw_cleartypeaheadtext", null);
        },

        //afterchange
        onAfterRelatedEntitySelected: function(event) {
            var datamap = event.fields;
            var selectedEntityName = datamap["relatedrecclass"];
            const selectedEntity = (function() {
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