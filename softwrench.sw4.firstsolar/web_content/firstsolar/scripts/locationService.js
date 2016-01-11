(function (angular) {
    'use strict';


    function firstSolarLocationService(redirectService, crudContextHolderService, alertService, restService) {


        function initBatchWorkorder(schema, datamap) {

            var selectionBuffer = crudContextHolderService.getSelectionModel().selectionBuffer;
            if (Object.keys(selectionBuffer).length === 0) {
                alertService.alert("Please select at least one location to proceed");
                return;
            }

            function confirmBatch(modalData) {

                var batchData = {
                    summary: modalData["summary"],
                    details: modalData["details"],
                    siteid: modalData["siteid"],
                    locations: Object.keys(selectionBuffer)
                }

                return restService.postPromise("FirstSolarWorkorderBatch", "InitLocationBatch", null, batchData);
            };

            var params = {
                popupmode: "modal",
                savefn: confirmBatch
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
    .clientfactory('locationService', ['redirectService', 'crudContextHolderService', 'alertService', 'restService', firstSolarLocationService]);


})(angular);