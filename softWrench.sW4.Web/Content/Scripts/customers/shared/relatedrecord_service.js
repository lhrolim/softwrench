var app = angular.module('sw_layout');

app.factory('relatedrecordService', function (redirectService, searchService) {
    var getWorkOrderId = function (app, wonum, siteid) {
        var searchData = {
            wonum: wonum,
            siteid: siteid
        };
        searchService.searchWithData("wonumlookup", searchData).success(function (data) {
            var resultObject = data.resultObject;
            var workorderid = resultObject[0]['fields']['workorderid'];
            redirectService.goToApplicationView(app, "editdetail", "input", null, { id: workorderid });
        });
    };

    return {

        open: function (datamap, columnmetadata) {
            var app = datamap['relatedrecclass'];
            var id = datamap['relatedreckey'];
            
            if (app == 'WORKORDER') {
                getWorkOrderId(app, id, datamap['siteid']);
            }
            else if (app == 'SR') {
                redirectService.goToApplicationView("servicerequest", "editdetail", "input", null, { id: id });
            }
            else {
                redirectService.goToApplicationView(app, "detail", "input", null, { id: id });
            }
        },

        saveBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },



    };

});