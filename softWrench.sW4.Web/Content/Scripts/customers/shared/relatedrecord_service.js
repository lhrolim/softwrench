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

    return {

        open: function (datamap, columnmetadata) {
            var app = datamap['relatedrecclass'];
            var userid = datamap['relatedreckey'];
            var siteid = datamap['siteid'];

            var swApp = app.equalsIc("sr") ? "servicerequest" : app;
            
            if (app.equalsAny('SR','WORKORDER')) {
                redirectService.goToApplicationView(swApp, "editdetail", "input", null, { userid: userid, siteid: siteid });
            }
            else {
                redirectService.goToApplicationView(app, "detail", "input", null, { userid: userid, siteid: siteid });
            }
        },

        saveBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },

    };

}]);

})(angular);