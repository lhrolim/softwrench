
(function () {
    'use strict';

    angular.module('sw_layout').service('invtransferService', ["$log", 'searchService', 'inventoryServiceCommons', "alertService", "redirectService", invtransferService]);

    function invtransferService($log, searchService, inventoryServiceCommons, alertService, redirectService) {
        const service = {
            afterChangeSite: afterChangeSite,
            afterChangeItem: afterChangeItem,
            afterChangeFromBin: afterChangeFromBin,
            afterChangeFromStoreLoc: afterChangeFromStoreLoc,
            afterChangeTransferQuantity: afterChangeTransferQuantity,
            afterChangeToStoreLoc: afterChangeToStoreLoc,
            cancel: cancel
        };
        return service;



        function afterChangeFromStoreLoc(parameters) {
            const fields = parameters['fields'];
            fields['invuseline_.fromstoreloc'] = fields['fromstoreloc'];
        };

        function afterChangeToStoreLoc(parameters) {
            const fields = parameters['fields'];
            fields['invuseline_.tostoreloc'] = fields['tostoreloc'];
        };

        function afterChangeSite(parameters) {
            const fields = parameters['fields'];
            if (fields['invuseline_.siteid'] == null || fields['invuseline_.siteid'].trim() === "") {
                nullifyProperties(fields, ['itemnum', 'fromstoreloc', 'invuseline_.frombin', 'invuseline_.tostoreloc', 'invuseline_.tobin']);
            }
        }

        function afterChangeTransferQuantity(event) {
            const fields = event.fields;
            if (fields['invuseline_.quantity'] > fields['#curbal']) {
                alertService.alert("The quantity being transferred cannot be greater than the current balance of the From Bin.");
                event.scope.datamap['invuseline_.quantity'] = fields['#curbal'];
            }
        }


        function afterChangeFromBin(parameters) {
            const fields = parameters['fields'];
            fields['invuseline_.fromlot'] = fields['frominvbalance_.lotnum'];
            fields['invuseline_.tolot'] = fields['frominvbalance_.lotnum'];
            fields['#curbal'] = fields['frominvbalance_.curbal'];
            fields['curbal'] = fields['#curbal'];
            //$rootScope.$digest();
        }

        function afterChangeItem(parameters) {

            var fields = parameters['fields'];
            const itemnum = fields['itemnum'];
            fields['invuseline_.itemnum'] = itemnum;


            nullifyProperties(fields, ['binnum', 'invuseline_.binnum', 'lotnum', 'invuseline_.lotnum', '#curbal', 'curbal', 'frominvbalance_.curbal']);

            if (nullOrEmpty(itemnum)) {
                nullifyProperties(fields, ['invuseline_.itemnum', 'unitcost', 'invuseline_.issueunit', 'invuseline_.itemtype']);
                return;
            }
            const searchData = {
                itemnum: itemnum,
                location: fields['fromstoreloc'],
                siteid: fields['siteid'],
                orgid: fields['orgid'],
                itemsetid: fields['itemsetid']
            };
            searchService.searchWithData("inventory", searchData)
                .then(function (response) {
                    const data = response.data;
                    const resultObject = data.resultObject;
                    const resultFields = resultObject[0];
                    const costtype = resultFields['costtype'];
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
            const searchOperators = {
                itemnum: searchService.getSearchOperator("="),
                location: searchService.getSearchOperator("="),
                siteid: searchService.getSearchOperator("=")
            };
            searchService.searchWithData("invbalances", searchData,
                "binLookupList", { searchOperators: searchOperators }).then(function (response) {
                    const data = response.data;
                    const resultObject = data.resultObject;
                    if (resultObject.length === 1) {
                        const resultFields = resultObject[0];
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

        function cancel() {
            redirectService.goToApplication("matrectransTransfers", "matrectransTransfersList");
        }
    }
})();

