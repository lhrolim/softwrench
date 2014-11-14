var app = angular.module('sw_layout');

app.factory('poreceivingService', function($http, redirectService) {

    return {
        poreceivelistclick: function(datamap, schema) {
            var param = {};
            //param.id = datamap['matrectrans_.matrectransid'];
            var application = 'matrectrans';
            var detail = 'newdetail';
            var mode = 'input';
            redirectService.goToApplicationView(application, detail, mode, null, param, null);

        },

        submitorderedItems: function (schema, datamap) {
            var param = {};
            param.id = datamap[0]['ponum'];
            var application = 'po';
            var detail = 'editdetail';
            var mode = 'input';
            redirectService.goToApplicationView(application, detail, mode, null, param, null);
        }
    };
});
