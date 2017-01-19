(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('generalsrService', ["alertService", "contextService", function (alertService, contextService) {

            return {

                //afterchange
                afterchangeowner: function (event) {
                    if (event.fields['owner'] == null) {
                        return;
                    }
                    if (event.fields['owner'] == ' ') {
                        event.fields['owner'] = null;
                        return;
                    }
                    if (event.fields['status'] == 'NEW') {
                        event.fields['status'] = 'QUEUED';
                        //Removing the alert for Kongsberg because Kongsberg uses generalsr_service and they can select both owner and ownergroup
                        //TODO: make a custom service for KOGT
                        if (!contextService.isClient('kongsberg')) {
                            alertService.alert("Owner Group Field will be disabled if the Owner is selected.");
                        }
                        return;
                    }
                },
                //afterchange
                afterchangeownergroup: function (event) {

                    if (event.fields['ownergroup'] == null) {
                        return;
                    }
                    if (event.fields['ownergroup'] == ' ') {
                        event.fields['ownergroup'] = null;
                        return;
                    }
                    if (event.fields['status'] == 'NEW') {
                        event.fields['status'] = 'QUEUED';
                        //Removing the alert for Kongsberg because Kongsberg uses generalsr_service and they can select both owner and ownergroup
                        //TODO: make a custom service for KOGT
                        if (!contextService.isClient('kongsberg')) {
                            alertService.alert("Owner Field will be disabled if the Owner Group is selected.");
                        }
                        return;
                    }
                }

            
            };

        }]);

})(angular);