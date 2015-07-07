
(function () {
    'use strict';

    angular.module('maximo_applications').factory('meterReadingService', ['$rootScope','applicationService', 'alertService', meterReadingService]);

    function meterReadingService($rootScope,applicationService, alertService) {

        var service = {
            read: read,
            markComplete: markComplete
        };

        return service;

        function read(schema, datamap) {
            var crudData = { crud: datamap };
            return applicationService.invokeOperation("workorder", "readings", "EnterMeter", crudData);
        }

        function markComplete(fieldMetadata, parentdata, compositionitem) {
            //this method is invoked before angular sets the model, so we need to mark the progress flag properly


            var completeMsg = "Are you sure you want to mark this Operation as Complete?";
            var incompleteMsg = "Are you sure you want to mark this Operation as Incomplete?";

            var originalValue = ""+compositionitem["progress2"];
            var msg = originalValue == "0" ? completeMsg : incompleteMsg;
            
            var reverseValue = originalValue == "0" ? "1" : "0";

            return alertService.confirmMsg(msg, function () {
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
