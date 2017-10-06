(function (angular, bootbox) {
    "use strict";


    class fsWorkOrderService {

        constructor(crudContextHolderService, applicationService) {
            this.applicationService = applicationService;
            this.crudContextHolderService = crudContextHolderService;
        }

        completeWorkOrder() {
            const datamap = this.crudContextHolderService.rootDataMap();
            datamap["status"] = 'COMP';
            this.applicationService.save({ datamap});
        }

        failureClassChanged(event) {
            event.fields["problemcode"] = null;
        }

        problemCodeChanged(event) {
            event.fields["fr1code"] = null;
        }

        causeChanged(event) {
            event.fields["fr2code"] = null;
        }
    }

    fsWorkOrderService.$inject = ['crudContextHolderService','applicationService'];

    angular.module('sw_layout').service('fsWorkOrderService', fsWorkOrderService);

})(angular, bootbox);