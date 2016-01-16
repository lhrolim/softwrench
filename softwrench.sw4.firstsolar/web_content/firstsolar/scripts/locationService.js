(function (angular) {
    'use strict';


    function firstSolarLocationService($rootScope, redirectService, crudContextHolderService, alertService, restService, contextService, modalService, batchworkorderService) {

        //        $rootScope.$on("sw.crud.applicationchanged", function() {
        //            //playing safe here
        //            contextService.deleteFromContext("batchshareddata");
        //        });


        function submitBatch(datamap) {

            var sharedData = contextService.get("batchshareddata", false, true);
            var specificData = {};

            var submissionData = {
                sharedData: sharedData,
                specificData: specificData
            };

            datamap.forEach(function (datamap) {
                var fields = datamap.fields;

                var customizedValues = Object.keys(fields).filter(function (prop) {
                    return prop !== "location" && fields[prop] !== sharedData[prop];
                });

                if (customizedValues.length !== 0) {
                    specificData[fields.location] = {};
                    customizedValues.forEach(function (prop) {
                        specificData[fields.location][prop] = fields[prop];
                    });
                } else {
                    specificData[fields.location] = null;
                }

            });
            restService.postPromise("FirstSolarWorkorderBatch", "SubmitLocationBatch", null, JSON.stringify(submissionData)).then(function(httpResponse) {
                var appResponse = httpResponse.data;
                return $rootScope.$broadcast("sw_redirectapplicationsuccess", appResponse, "input", "workorder");
            });

        }

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
            };

            redirectService.goToApplication("workorder", "batchshared", params);
        };


        function dispatchWO(schema, datamap) {

            var queryParams = {
                location: datamap.fields["location"],
                classification: datamap.fields["classstructureid"]
            };

            return restService.getPromise("FirstSolarWorkorderBatch", "GetListOfRelatedWorkorders", queryParams).then(function (httpResponse) {
                var appResponse = httpResponse.data;
                if (appResponse.type === "BlankApplicationResponse") {
                    return redirectService.goToApplication("workorder", "newdetail", null, { "location": datamap.fields["location"] });
                }

                contextService.insertIntoContext("grid_refreshdata", { data: appResponse, panelid: "#modal" }, true);
                crudContextHolderService.setFixedWhereClause("#modal", appResponse.pageResultDto.filterFixedWhereClause);
                return modalService.show(appResponse.schema, appResponse.resultObject, { title: "There are already related workorders. Proceed?" }, function () {
                    return redirectService.goToApplication("workorder", "newdetail", null, { "location": datamap.fields["location"] });
                });

            });
        }

        var service = {
            initBatchWorkorder: initBatchWorkorder,
            dispatchWO: dispatchWO,
            submitBatch: submitBatch
        };

        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('locationService', ['$rootScope', 'redirectService', 'crudContextHolderService', 'alertService', 'restService', 'contextService', 'modalService', 'firstsolar.batchWorkorderService', firstSolarLocationService]);


})(angular);