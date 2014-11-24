var app = angular.module('sw_layout');

app.factory('poreceivingService', function($http, redirectService, restService) {

    return {
        poreceivelistclick: function(datamap, schema) {
            var param = {};
            //param.id = datamap['matrectrans_.matrectransid'];
            var application = 'matrectrans';
            var detail = 'newdetail';
            var mode = 'input';
            redirectService.goToApplicationView(application, detail, mode, null, param, null);

        },

        submitorderedItems: function(schema, datamap) {
            var param = {};
            param.id = datamap[0]['ponum'];
            var application = 'po';
            var detail = 'editdetail';
            var mode = 'input';
            redirectService.goToApplicationView(application, detail, mode, null, param, null);
        },

        submitMatrecTrans: function(schema, datamap) {
            // create a datamap for matrectrans
            var matdatamap = {};
            matdatamap['ponum'] = datamap['ponum'];
            matdatamap['polinenum'] = datamap['polinenum'];
            matdatamap['quantity'] = datamap['#qtytoreceive'];

            var jsonString = angular.toJson(matdatamap);
            // submit a post of matrectrans to hit the right crudconnector
            var httpParameters = {
                application: "materialrecords",
                platform: "web",
                currentSchemaKey: "newdetail.input.web"
            };
            restService.invokePost("data", "post", httpParameters, jsonString, function() {
                var restParameters = {
                    key: {
                        schemaId: "list",
                        mode: "none",
                        platform: "web"
                    },
                    SearchDTO: null
                };
                var urlToUse = url("/api/Data/materialrecords?" + $.param(restParameters));
                $http.get(urlToUse).success(function(data) {
                    redirectService.goToApplication("materialrecords", "list", null, data);
                });
            });
        },
    };
});