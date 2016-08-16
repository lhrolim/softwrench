(function (angular) {
    'use strict';


    function firstSolarLocationService($rootScope, redirectService, crudContextHolderService, alertService, restService, contextService, modalService, batchworkorderService, workorderservice) {

        //        $rootScope.$on("sw.crud.applicationchanged", function() {
        //            //playing safe here
        //            contextService.deleteFromContext("batchshareddata");
        //        });



        function proceedToBatchSelection(httpResponse) {
            var confirmMessage = "All the selected locations already have corresponding workorders. Do you want to proceed anyway?";
            return batchworkorderService.proceedToBatchSelection(httpResponse, confirmMessage);
        }

        function initBatchWorkorder(schema, datamap) {
            var selectionBuffer = crudContextHolderService.getSelectionModel().selectionBuffer;
            if (Object.keys(selectionBuffer).length === 0) {
                alertService.alert("Please select at least one location to proceed");
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


        function dispatchWo(schema, datamap) {

            var queryParams = {
                location: datamap["location"],
                classification: datamap["classstructureid"]
            };

            return restService.getPromise("FirstSolarWorkorderBatch", "GetListOfRelatedWorkorders", queryParams).then(function (httpResponse) {
                var appResponse = httpResponse.data;
                if (appResponse.type === "BlankApplicationResponse") {
                    return workorderservice.openNewDetailModal();
                }

                contextService.insertIntoContext("grid_refreshdata", { data: appResponse, panelid: "#modal" }, true);
                crudContextHolderService.setFixedWhereClause("#modal", appResponse.pageResultDto.filterFixedWhereClause);
                return modalService.show(appResponse.schema, appResponse.resultObject, { title: "There are already related workorders. Proceed?" }, function () {
                    workorderservice.openNewDetailModal();
                });

            });
        }

        var service = {
            initBatchWorkorder: initBatchWorkorder,
            dispatchWO: dispatchWo
        };

        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('locationService', ['$rootScope', 'redirectService', 'crudContextHolderService', 'alertService', 'restService', 'contextService', 'modalService', 'firstsolar.batchWorkorderService', 'workorderService', firstSolarLocationService]);


})(angular);