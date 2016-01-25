
(function (angular) {
    'use strict';



    function batchWorkorderService($rootScope, $q, $log, crudContextHolderService, modalService, associationService, validationService, restService, alertService, contextService, redirectService) {


        // save method of inline wo edit modal
        function woInlineEditSave(saveDataMap, schema) {
            var validationErrors = validationService.validate(schema, schema.displayables, saveDataMap, {});
            if (validationErrors.length > 0) {
                return;
            }

            var params = {
                batchType: saveDataMap.userIdFieldName === "location" ? "location" : "asset",
                specificValue: saveDataMap[saveDataMap.userIdFieldName],
                classificationId: saveDataMap["classificationid"]
            }

            var resultObject;

            restService.postPromise("FirstSolarWorkorderBatch", "ValidateExistingWorkorders", params).then(function (httpResponse) {
                resultObject = httpResponse.data.resultObject;
                associationService.insertAssocationLabelsIfNeeded(schema, saveDataMap);
                var gridDatamap = crudContextHolderService.rootDataMap();
                gridDatamap.forEach(function (row) {
                    if (row.fields[schema.idFieldName] === saveDataMap[schema.idFieldName]) {
                        row.fields.summary = saveDataMap.summary;
                        row.fields.details = saveDataMap.details;
                        row.fields.siteid = saveDataMap.siteid;
                        row.fields.classification = saveDataMap["#classificationid_label"] || "";
                        row.fields.classificationid = saveDataMap.classificationid;
                        row.fields["#warning"] = resultObject["#warning"];
                        row.fields["#wonums"] = resultObject["#wonums"];
                    }
                });
                modalService.hide();
            });
        }

        // save method of pre wo batch creation
        function woBatchSharedSave(schema, modalData, modalSchema) {


            associationService.insertAssocationLabelsIfNeeded(modalSchema, modalData);
            var selectionBuffer = crudContextHolderService.getSelectionModel().selectionBuffer;

            var batchData = {
                summary: modalData["summary"],
                details: modalData["details"],
                siteid: modalData["siteid"],
                classification: {
                    value: modalData["classificationid"],
                    label: modalData["#classificationid_label"]
                }
            }


            batchData["items"] = Object.keys(selectionBuffer).map(function (key) {
                var value = selectionBuffer[key];
                var associationOption = {
                    value: value.fields[schema.userIdFieldName],
                    label: value.fields["description"]
                };

                associationOption.extraFields = {
                    "siteid": value.fields["siteid"]
                }

                if (schema.applicationName === "asset") {
                    //passing location of the asset as an extra projection field
                    associationOption.extraFields["location"] = value.fields["location"];
                }
                return associationOption;
            });

            var params = {
                batchType: schema.applicationName === "asset" ? "asset" : "location"
            }

            return restService.postPromise("FirstSolarWorkorderBatch", "InitBatch", params, batchData);
        }

        function proceedToBatchSelection(httpResponse, confirmMessage) {
            var applicationResponse = httpResponse.data;
            //            if (applicationResponse.extraParameters && true === applicationResponse.extraParameters["allworkorders"]) {
            //                return alertService.confirm2(confirmMessage)
            //                    .then(function () {
            //                        //storing untouched first line to serve as shared data later
            //                        contextService.set("batchshareddata", applicationResponse.resultObject[0].fields, true);
            //                        return redirectService.redirectFromServerResponse(applicationResponse);
            //
            //                    }).catch(function () {
            //                        //catching exception in order to close the modal on the outer promise handler
            //                        return;
            //                    });
            //            }
            contextService.set("batchshareddata", applicationResponse.resultObject[0].fields, true);
            return redirectService.redirectFromServerResponse(applicationResponse);
        }

        function spreadSheetLineClick(rowDm, column, schema) {
            var wonums = rowDm["#wonums"];
            if (column.attribute === "#warning" && wonums) {
                loadRelatedWorkorders(rowDm, wonums);
                return false;
            }

            var nextSchemaId = schema.properties["list.click.schema"];
            redirectService.openAsModal("workorder", nextSchemaId, null, rowDm);
            return false;
        }

        function loadRelatedWorkorders(rowDm, wonums) {
            var commaSeparattedQuotedIds = wonums.split(',').map(function (item) {
                return "'" + item + "'";
            }).join(",");

            var fixedWhereClause = "wonum in ({0})".format(commaSeparattedQuotedIds);

            var params = {
                searchDTO: {
                    filterFixedWhereClause: fixedWhereClause
                },
                title: "Work Orders of " + rowDm["specificLabel"]
            }

            redirectService.openAsModal("workorder", "readonlyfixedlist", params).then(function () {
                crudContextHolderService.setFixedWhereClause("#modal", fixedWhereClause);
            });
        }

        function submitBatch(batchType) {

            var log = $log.get("batchWorkorderService#submitBatch", ["workorder"]);

            log.debug("init batch submission process for {0}".format(batchType));

            var keyName = batchType === "asset" ? "assetnum" : "location";

            var itemsToSubmit = crudContextHolderService.getSelectionModel().selectionBuffer;
            var itemsToSubmitKeys = Object.keys(itemsToSubmit);

            var sharedData = contextService.get("batchshareddata", false, true);
            var specificData = {};

            var submissionData = {
                sharedData: sharedData,
                specificData: specificData
            };

            if (itemsToSubmitKeys.length === 0) {
                alertService.alert("Please, select at least one entry to confirm the batch");
                return $q.reject();
            }

            itemsToSubmitKeys.forEach(function (bufferKey) {
                var datamap = itemsToSubmit[bufferKey];
                var fields = datamap.fields;
                var customizedValues = Object.keys(fields).filter(function (prop) {
                    return prop !== keyName && fields[prop] !== sharedData[prop];
                });

                var key = fields[keyName];

                if (customizedValues.length !== 0) {
                    specificData[key] = {};
                    customizedValues.forEach(function (prop) {
                        specificData[key][prop] = fields[prop];
                    });
                } else {
                    specificData[key] = null;
                }

            });
            var params = { batchType: batchType };


            return restService.postPromise("FirstSolarWorkorderBatch", "SubmitBatch", params, JSON.stringify(submissionData))
                .then(function (httpResponse) {
                    var appResponse = httpResponse.data;
                    return redirectService.redirectFromServerResponse(appResponse, "workorder");
                });

        }


        var service = {
            woInlineEditSave: woInlineEditSave,
            woBatchSharedSave: woBatchSharedSave,
            proceedToBatchSelection: proceedToBatchSelection,
            spreadSheetLineClick: spreadSheetLineClick,
            submitBatch: submitBatch
        };

        return service;
    }

    angular
      .module('firstsolar')
      .clientfactory('batchWorkorderService', ['$rootScope', "$q", "$log", 'crudContextHolderService', 'modalService', 'associationService', 'validationService', 'restService', 'alertService', 'contextService', 'redirectService', batchWorkorderService]);
})(angular);