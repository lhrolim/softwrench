
(function () {
    'use strict';

    angular.module('maximo_applications').factory('meterReadingService', ['$rootScope', 'applicationService', 'alertService', 'commandService', meterReadingService]);

    function meterReadingService($rootScope, applicationService, alertService, commandService) {

        var service = {
            read: read,
            markComplete: markComplete,
            openReadingModal: openReadingModal,
        };

        return service;

        function openReadingModal(datamap, fieldmetadata, schema) {
            var datamapToSend = {};
            angular.copy(datamap, datamapToSend)
            datamapToSend['multiassetlocci_']=[datamap]
            
            var command = {
                service: "meterReadingService",
                method: "read",
                nextSchemaId: "readings",
                stereotype: "modal",
                properties: {
                    modalclass: "readingsmodal"
                },
                scopeParameters: ['schema', 'datamap']
            };
            var clonedSchema = {};
            angular.copy(schema, clonedSchema)


            var scope = {
                datamap: datamapToSend,
                schema: clonedSchema
            }
            scope.schema.applicationName = "workorder";


            commandService.doCommand(scope, command);

        }

        function read(schema, datamap) {
            var crudData = { crud: datamap };
            return applicationService.invokeOperation("workorder", "readings", "EnterMeter", crudData);
        }

        function markComplete(fieldMetadata, parentdata, compositionitem) {
            //this method is invoked before angular sets the model, so we need to mark the progress flag properly


            var completeMsg = "Are you sure you want to mark this Operation as Complete?";
            var incompleteMsg = "Are you sure you want to mark this Operation as Incomplete?";

            var originalValue = "" + compositionitem["progress2"];
            var msg = originalValue == "0" ? completeMsg : incompleteMsg;

            var reverseValue = originalValue == "0" ? "1" : "0";

            return alertService.confirm(msg).then(function () {
                compositionitem["#isDirty"] = true;
                compositionitem["progress2"] = reverseValue;
                //sending minimum amount of data
                var crudData = {
                    'wonum': parentdata.fields['wonum'],
                    'siteid': parentdata.fields['siteid'],
                    'multiassetlocci_': [compositionitem]
                };


                return applicationService.invokeOperation("workorder", "readings", null, crudData).then(function (data) {
                    compositionitem["#isDirty"] = false;
                }).catch(function (data) {
                    compositionitem["progress2"] = originalValue;
                });
            }, function () {
                compositionitem["progress2"] = originalValue;
                compositionitem["#isDirty"] = false;
                $rootScope.$digest();
            });
        }
    }
})();
