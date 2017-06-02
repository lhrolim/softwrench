
(function (angular) {
    'use strict';

    angular
        .module('sw_layout')
        .controller('fsEmailRequestController', ['$scope', 'restService', 'alertService', fsEmailRequestController]);

    fsEmailRequestController.$inject = ["$scope", "restService", "alertService"];

    function fsEmailRequestController($scope, restService, alertService) {

        $scope.vm = {
            initial: true
        }

        $scope.addNotes = function () {

            const token = $(hddn_fstoken).val();
            const fsType = $(hddn_fstype).val();

            if (fsType.equalsIc("callout")) {
                return restService.put("FirstSolarEmailRest",
                    "AddWorkLogToCallOut",
                    { token, worklog: $scope.additionalNotes }).then(data => {
                        $scope.vm.initial = false;
                        alertService.success("Additional notes have been successfully submitted");
                    });
            }


        }

    }


})(angular);
