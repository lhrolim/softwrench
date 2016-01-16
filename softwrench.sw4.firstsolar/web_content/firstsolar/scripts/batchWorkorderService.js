
(function () {
    'use strict';

    angular
      .module('sw_layout')
      .factory('firstsolar.batchWorkorderService', ['$rootScope', 'crudContextHolderService', 'modalService', 'associationService', 'validationService', 'restService', 'alertService', 'contextService', 'redirectService', batchWorkorderService]);

    function batchWorkorderService($rootScope, crudContextHolderService, modalService, associationService, validationService, restService, alertService, contextService, redirectService) {
        var service = {
            woInlineEditSave: woInlineEditSave,
            woBatchSharedSave: woBatchSharedSave,
            proceedToBatchSelection: proceedToBatchSelection,
            loadRelatedWorkorders: loadRelatedWorkorders
        };

        // save method of inline wo edit modal
        function woInlineEditSave(saveDataMap, schema) {
            var validationErrors = validationService.validate(schema, schema.displayables, saveDataMap, {});
            if (validationErrors.length > 0) {
                return;
            }

            associationService.insertAssocationLabelsIfNeeded(schema, saveDataMap);
            var gridDatamap = crudContextHolderService.rootDataMap();
            gridDatamap.forEach(function (row) {
                if (row.fields[schema.idFieldName] === saveDataMap[schema.idFieldName]) {
                    row.fields.summary = saveDataMap.summary;
                    row.fields.details = saveDataMap.details;
                    row.fields.siteid = saveDataMap.siteid;
                    row.fields.classification = saveDataMap["#classificationid_label"] || "";
                    row.fields.classificationid = saveDataMap.classificationid;
                }
            });
            modalService.hide();
        }

        // save method of pre wo batch creation
        function woBatchSharedSave(nonSharedPropTargetField, nonSharedPropSrcValueField, nonSharedPropSrcDescriptionField, selectionBuffer, controllerBatchInitMethod, proceedToBatchSelectionFunction) {
            var confirmBatch = function (modalData, schema) {
                associationService.insertAssocationLabelsIfNeeded(schema, modalData);

                var batchData = {
                    summary: modalData["summary"],
                    details: modalData["details"],
                    siteid: modalData["siteid"],
                    classification: {
                        value: modalData["classificationid"],
                        label: modalData["#classificationid_label"]
                    }
                }
                batchData[nonSharedPropTargetField] = Object.keys(selectionBuffer).map(function(key) {
                    var value = selectionBuffer[key];
                    return {
                        value: value.fields[nonSharedPropSrcValueField],
                        label: value.fields[nonSharedPropSrcDescriptionField]
                    };
                });

                return restService.postPromise("FirstSolarWorkorderBatch", controllerBatchInitMethod, null, batchData).then(proceedToBatchSelectionFunction);
            };
            return confirmBatch;
        }

        function proceedToBatchSelection(httpResponse, confirmMessage) {
            var applicationResponse = httpResponse.data;
            if (applicationResponse.extraParameters && true === applicationResponse.extraParameters["allworkorders"]) {
                return alertService.confirm2(confirmMessage)
                    .then(function () {
                        //storing untouched first line to serve as shared data later
                        contextService.set("batchshareddata", applicationResponse.resultObject[0].fields, true);
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

        return service;
    }
})();
