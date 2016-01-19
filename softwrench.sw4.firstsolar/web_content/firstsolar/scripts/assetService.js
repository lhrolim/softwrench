(function (angular) {
    'use strict';


    function firstSolarAssetService(redirectService, crudContextHolderService, alertService, restService, $rootScope, batchworkorderService) {


        function proceedToBatchSelection(httpResponse) {
            var confirmMessage = "All the selected assets already have corresponding workorders. Do you want to proceed anyway?";
            return batchworkorderService.proceedToBatchSelection(httpResponse, confirmMessage);
        }

        function initBatchWorkorder(schema, datamap) {
            var selectionBuffer = crudContextHolderService.getSelectionModel().selectionBuffer;
            if (Object.keys(selectionBuffer).length === 0) {
                alertService.alert("Please select at least one asset to proceed");
                return;
            }

            var params = {
                popupmode: "modal",
                savefn: function (modaData, modalSchema) {
                    return batchworkorderService.woBatchSharedSave(schema, modaData, modalSchema).then(proceedToBatchSelection);
                }
            };

            redirectService.goToApplication("workorder", "batchshared", params);
        };

        var service = {
            initBatchWorkorder: initBatchWorkorder
        };

        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('assetService', ['redirectService', 'crudContextHolderService', 'alertService', 'restService', '$rootScope', 'firstsolar.batchWorkorderService', firstSolarAssetService]);


})(angular);