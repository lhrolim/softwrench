
(function () {
    'use strict';

    angular.module('sw_layout').factory('invtransferService', ["$log",'searchService','inventoryServiceCommons', "alertService","redirectService", invtransferService]);

    function invtransferService($log, searchService, inventoryServiceCommons, alertService,redirectService) {

        var service = {
            afterChangeSite: afterChangeSite,
            afterChangeItem: afterChangeItem,
            afterChangeBin: afterChangeFromBin,
            afterChangeFromStoreLoc: afterChangeFromStoreLoc,
            afterChangeTransferQuantity: afterChangeTransferQuantity,
            cancelTransfer: cancelTransfer
        };

        return service;

        

        function afterChangeFromStoreLoc(parameters) {
            parameters['fields']['invuseline_.fromstoreloc'] = parameters['fields']['fromstoreloc'];
        };

        function afterChangeSite(parameters) {
            var fields = parameters['fields'];
            if (fields['invuseline_.siteid'] == null || fields['invuseline_.siteid'].trim() === "") {
                fields['itemnum'] = null;
                fields['fromstoreloc'] = null;
                fields['invuseline_.frombin'] = null;
                fields['invuseline_.tostoreloc'] = null;
                fields['invuseline_.tobin'] = null;
            }

        };

        function afterChangeTransferQuantity(event) {
            var fields = event.fields;
            if (fields['invuseline_.quantity'] > fields['#curbal']) {
                alertService.alert("The quantity being transferred cannot be greater than the current balance of the From Bin.");
                event.scope.datamap['invuseline_.quantity'] = fields['#curbal'];
            }
        };


        function afterChangeFromBin (parameters) {
            var fields = parameters['fields'];
            fields['invuseline_.fromlot'] = fields['frominvbalance_.lotnum'];
            fields['invuseline_.tolot'] = fields['frominvbalance_.lotnum'];
            fields['#curbal'] = fields['frominvbalance_.curbal'];
            //$rootScope.$digest();
        };

        function afterChangeItem(parameters) {
            var itemnum = parameters['fields']['itemnum'];
            parameters['fields']['invuseline_.itemnum'] = itemnum;
            parameters['fields']['binnum'] = null;
            parameters['fields']['invuseline_.binnum'] = null;
            parameters['fields']['lotnum'] = null;
            parameters['fields']['invuseline_.lotnum'] = null;
            parameters['fields']['#curbal'] = null;
            parameters['fields']['frominvbalance_.curbal'] = null;
            if (nullOrEmpty(itemnum)) {
                parameters['fields']['invuseline_.itemnum'] = null;
                parameters['fields']['unitcost'] = null;
                parameters['fields']['invuseline_.issueunit'] = null;
                parameters['fields']['invuseline_.itemtype'] = null;
                return;
            }

            var searchData = {
                itemnum: itemnum,
                location: parameters['fields']['fromstoreloc'],
                siteid: parameters['fields']['siteid'],
                orgid: parameters['fields']['orgid'],
                itemsetid: parameters['fields']['itemsetid']
            };

            searchService.searchWithData("inventory", searchData)
                .success(function (data) {
                    var resultObject = data.resultObject;
                    var fields = resultObject[0].fields;
                    var costtype = fields['costtype'];
                    parameters['fields']['inventory_.costtype'] = costtype;
                    var locationFieldName = "";
                    if (parameters['fields'].fromstoreloc != undefined) {
                        locationFieldName = "fromstoreloc";
                    }
                    inventoryServiceCommons.doUpdateUnitCostFromInventoryCost(parameters, "invuseline_.unitcost", locationFieldName);
                });

            // Check if there is a single invbalance record for the item in the
            // given location. If there is, use it as the from bin, from lot 
            // and curbal.
            var searchOperators = {
                itemnum: searchService.getSearchOperator("="),
                location: searchService.getSearchOperator("="),
                siteid: searchService.getSearchOperator("=")
            }
            searchService.searchWithData("invbalances", searchData,
                "binLookupList", { searchOperators: searchOperators }).success(function (data) {
                    var resultObject = data.resultObject;
                    if (resultObject.length === 1) {
                        parameters["fields"]["binnum"] =
                            resultObject[0].fields.binnum;
                        parameters["fields"]["invuseline_.frombin"] =
                            resultObject[0].fields.binnum;
                        parameters["fields"]["lotnum"] =
                            resultObject[0].fields.lotnum;
                        parameters["fields"]["invuseline_.lotnum"] =
                            resultObject[0].fields.lotnum;
                        parameters["fields"]["#curbal"] =
                            resultObject[0].fields.curbal;
                        parameters["fields"]["frominvbalance_.curbal"] =
                            resultObject[0].fields.curbal;
                    }
                });
        };

        function cancelTransfer() {
            redirectService.goToApplication("matrectransTransfers", "matrectransTransfersList");
        };
    }
})();

