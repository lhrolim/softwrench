var app = angular.module('sw_layout');

app.factory('relatedrecordService', function (redirectService) {

    return {

        open: function (datamap, columnmetadata) {
            var app = datamap['relatedrecclass'];
            var id = datamap['relatedreckey'];
            redirectService.goToApplication(app, "detail", {id:id});
        },

        saveBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },



    };

});