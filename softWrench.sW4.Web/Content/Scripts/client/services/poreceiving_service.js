﻿var app = angular.module('sw_layout');

app.factory('poreceivingService', function ($http, redirectService, restService, searchService, alertService) {

    var getReceiptData = function(ponum) {
        var searchData = {
            ponum: ponum
        };
        searchService.searchWithData("po", searchData).success(function (data) {
            var resultObject = data.resultObject;
            return resultObject[0]['fields']['receipts'];
        });
    };

    return {

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
            matdatamap['gldebitacct'] = datamap['gldebitacct'];
            matdatamap['glcreditacct'] = datamap['glcreditacct'];
            matdatamap['itemnum'] = datamap['itemnum'];
            matdatamap['itemsetid'] = datamap['itemsetid'];
            matdatamap['siteid'] = datamap['siteid'];
            matdatamap['orgid'] = datamap['orgid'];
            matdatamap['linetype'] = datamap['linetype'];

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
                    redirectService.goToApplication("receiving", "list", null, data);
                });
            });
        },

        completeReceipts: function(schema, datamap) {
            // Check if the receipts is already complete for the application
            var ponum = datamap[0]['ponum'];
            var searchData = {
                ponum: ponum
            };
            searchService.searchWithData("po", searchData).success(function (data) {
                var resultObject = data.resultObject;
                var receipts = resultObject[0]['fields']['receipts'];
                if (receipts.equalIc('complete')) {
                    alertService.alert("RECEIPTS ALREADY COMPLETED");
                    return;
                }
                // if not check if the quantity due on all the polines is 0


                // if quantity due is 0, complete receipts
            });

        },
    };
});