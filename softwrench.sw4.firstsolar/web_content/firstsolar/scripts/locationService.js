(function (angular) {
    'use strict';


    function firstSolarLocationService($rootScope, redirectService, crudContextHolderService, alertService, restService, contextService, modalService) {

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
            restService.postPromise("FirstSolarWorkorderBatch", "SubmitLocationBatch", null, JSON.stringify(submissionData));
        }

        function proceedToBatchSelection(httpResponse) {
            var applicationResponse = httpResponse.data;
            if (applicationResponse.extraParameters && true === applicationResponse.extraParameters["allworkorders"]) {
                return alertService.confirm2("All the selected locations already have corresponding workorders.Do you want to proceed anyway?")
                    .then(function () {
                        //storing untouched first line to serve as shared data later
                        contextService.set("batchshareddata", applicationResponse.resultObject[0].fields);
                        return $rootScope.$broadcast("sw_redirectapplicationsuccess", applicationResponse, "input", "workorder");
                    }).catch(function () {
                        //catching exception in order to close the modal on the outer promise handler
                        return;
                    });
            }
            contextService.set("batchshareddata", applicationResponse.resultObject[0].fields, true);
            return $rootScope.$broadcast("sw_redirectapplicationsuccess", applicationResponse, "input", "workorder");
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
            loadRelatedWorkorders: loadRelatedWorkorders,
            dispatchWO: dispatchWO,
            submitBatch: submitBatch
        };

        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('locationService', ['$rootScope', 'redirectService', 'crudContextHolderService', 'alertService', 'restService', 'contextService', 'modalService', firstSolarLocationService]);


})(angular);