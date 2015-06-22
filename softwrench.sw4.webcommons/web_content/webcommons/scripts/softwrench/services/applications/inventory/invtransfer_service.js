﻿
(function () {
    'use strict';

    angular.module('sw_layout').factory('invtransferService', ["$log", 'searchService', 'inventoryServiceCommons', "alertService", "redirectService", invtransferService]);

    function invtransferService($log, searchService, inventoryServiceCommons, alertService, redirectService) {

        var service = {
            afterChangeSite: afterChangeSite,
            afterChangeItem: afterChangeItem,
            afterChangeFromBin: afterChangeFromBin,
            afterChangeFromStoreLoc: afterChangeFromStoreLoc,
            afterChangeTransferQuantity: afterChangeTransferQuantity,
            cancelTransfer: cancelTransfer
        };

        return service;



        function afterChangeFromStoreLoc(parameters) {
            var fields = parameters['fields'];
            fields['invuseline_.fromstoreloc'] = fields['fromstoreloc'];
            if (fields['#datamaptype'] == "compositionitem") {
                //let´s put a negative id so that it gets ignored on maximo side
                fields['invuseid'] = -1 * (fields["#datamapidx"] - 1);
            }
        };

        function afterChangeSite(parameters) {
            var fields = parameters['fields'];
            if (fields['invuseline_.siteid'] == null || fields['invuseline_.siteid'].trim() === "") {
                nullifyProperties(fields, ['itemnum', 'fromstoreloc', 'invuseline_.frombin', 'invuseline_.tostoreloc', 'invuseline_.tobin']);
            }
        }

        function afterChangeTransferQuantity(event) {
            var fields = event.fields;
            if (fields['invuseline_.quantity'] > fields['#curbal']) {
                alertService.alert("The quantity being transferred cannot be greater than the current balance of the From Bin.");
                event.scope.datamap['invuseline_.quantity'] = fields['#curbal'];
            }
        }


        function afterChangeFromBin(parameters) {
            var fields = parameters['fields'];
            fields['invuseline_.fromlot'] = fields['frominvbalance_.lotnum'];
            fields['invuseline_.tolot'] = fields['frominvbalance_.lotnum'];
            fields['#curbal'] = fields['frominvbalance_.curbal'];
            fields['curbal'] = fields['#curbal'];
            //$rootScope.$digest();
        }

        function afterChangeItem(parameters) {

            var fields = parameters['fields'];
            var itemnum = fields['itemnum'];

            fields['invuseline_.itemnum'] = itemnum;


            nullifyProperties(fields, ['binnum', 'invuseline_.binnum', 'lotnum', 'invuseline_.lotnum', '#curbal', 'curbal', 'frominvbalance_.curbal']);

            if (nullOrEmpty(itemnum)) {
                nullifyProperties(fields, ['invuseline_.itemnum', 'unitcost', 'invuseline_.issueunit', 'invuseline_.itemtype']);
                return;
            }

            var searchData = {
                itemnum: itemnum,
                location: fields['fromstoreloc'],
                siteid: fields['siteid'],
                orgid: fields['orgid'],
                itemsetid: fields['itemsetid']
            };

            searchService.searchWithData("inventory", searchData)
                .success(function (data) {
                    var resultObject = data.resultObject;
                    var resultFields = resultObject[0].fields;
                    var costtype = resultFields['costtype'];
                    fields['inventory_.costtype'] = costtype;
                    var locationFieldName = "";
                    if (fields.fromstoreloc != undefined) {
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
                        var resultFields = resultObject[0].fields;
                        fields["binnum"] = resultFields.binnum;
                        fields["invuseline_.frombin"] = resultFields.binnum;
                        fields["lotnum"] = resultFields.lotnum;
                        fields["invuseline_.lotnum"] = resultFields.lotnum;
                        fields["#curbal"] = resultFields.curbal;
                        fields["curbal"] = resultFields.curbal;
                        fields["frominvbalance_.curbal"] = resultFields.curbal;
                    }
                });
        }

        function cancelTransfer() {
            redirectService.goToApplication("matrectransTransfers", "matrectransTransfersList");
        }
    }
})();

