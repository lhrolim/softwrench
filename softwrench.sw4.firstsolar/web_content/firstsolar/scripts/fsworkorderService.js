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

    }

    fsWorkOrderService.$inject = ['crudContextHolderService','applicationService'];

    angular.module('sw_layout').service('fsWorkOrderService', fsWorkOrderService);

})(angular, bootbox);