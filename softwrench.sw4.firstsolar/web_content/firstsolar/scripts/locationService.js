(function (angular) {
    'use strict';


    function firstSolarLocationService(redirectService, crudContextHolderService, alertService, restService, $rootScope) {

        function submitBatch() {
            
        }

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

                var commaSeparattedQuotedIds =
                    wonums.split(',')
                    .map(function (item) {
                        return "'" + item + "'";
                    }).join(",");

                var fixedWhereClause = "wonum in ({0})".format(commaSeparattedQuotedIds);

                var params = {
                    searchDTO: {
                        filterFixedWhereClause: fixedWhereClause
                    }
                }

                redirectService.openAsModal("workorder", "readonlyfixedlist", params).then(function () {
                    crudContextHolderService.setFixedWhereClause("#modal", fixedWhereClause);
                });
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
                    classification: modalData["classstructureid"],
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
            loadRelatedWorkorders: loadRelatedWorkorders,
            submitBatch:submitBatch
        };

        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('locationService', ['redirectService', 'crudContextHolderService', 'alertService', 'restService', '$rootScope', firstSolarLocationService]);


})(angular);