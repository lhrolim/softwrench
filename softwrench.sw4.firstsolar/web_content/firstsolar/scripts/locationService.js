(function (angular) {
    'use strict';


    function firstSolarLocationService(redirectService, crudContextHolderService, alertService, restService, $rootScope) {


        function proceedToBatchSelection(httpResponse) {
            var resultObject = httpResponse.data;
            if (resultObject.extraParameters && true === resultObject.extraParameters["allworkorders"]) {
                return alertService.confirm2("All the selected locations already have corresponding workorders.Do you want to proceed anyway?")
                    .then(function () {
                        return $rootScope.$broadcast("sw_redirectapplicationsuccess", resultObject, "input", "workorder");
                    }).catch(function () {
                        //catching exception in order to close the modal on the outer promise handler
                        return;
                    });
            }

            return $rootScope.$broadcast("sw_redirectapplicationsuccess", resultObject, "input", "workorder");
        }

        function loadRelatedWorkorders(rowDm, column) {
            if (column.attribute === "#warning") {
                var wonums = rowDm["#wonums"];

                var params ={
                    searchDTO: {
                        searchParams : "wonum",
                        searchValues : wonums
                    }
                }
                redirectService.openAsModal("workorder", "readonlyfixedlist", params);
                return false;
            }
            return true;
        }


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
                    locations: Object.keys(selectionBuffer).map(function (key) {
                        var value = selectionBuffer[key];
                        return { value: value.fields.location, label: value.fields.description };
                    })
                }

                return restService.postPromise("FirstSolarWorkorderBatch", "InitLocationBatch", null, batchData).then(proceedToBatchSelection);

            };

            var params = {
                popupmode: "modal",
                savefn: confirmBatch
            };



            redirectService.goToApplication("workorder", "batchshared", params);
        };

        var service = {
            initBatchWorkorder: initBatchWorkorder,
            loadRelatedWorkorders: loadRelatedWorkorders
        };

        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('locationService', ['redirectService', 'crudContextHolderService', 'alertService', 'restService', '$rootScope', firstSolarLocationService]);


})(angular);