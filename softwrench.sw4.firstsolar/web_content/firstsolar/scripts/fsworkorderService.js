(function (angular, bootbox) {
    "use strict";


    class fsWorkOrderService {

        constructor(crudContextHolderService, applicationService, printService) {
            this.applicationService = applicationService;
            this.crudContextHolderService = crudContextHolderService;
            this.printService = printService;
        }

        completeWorkOrder() {
            const datamap = this.crudContextHolderService.rootDataMap();
            datamap["status"] = 'COMP';
            this.applicationService.save({ datamap });
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

        calculateTotalLaborHours(datamap) {
            if (!datamap["labtrans_"]) {
                return 0;
            }
            return datamap["labtrans_"].map(a => a.regularhrs).reduce((a, b) => a + b, 0);
        }

        printServiceReport() {
            const datamap = this.crudContextHolderService.rootDataMap();
            const schema = this.crudContextHolderService.currentSchema();
            this.printService.printDetail(
                schema, datamap, {
                    printSchemaId: "fsservicereport", shouldPrintMain: true, shouldPageBreak: true, orientation: 'landscape', margin: '0'
                });
        }
    }

    fsWorkOrderService.$inject = ['crudContextHolderService', 'applicationService', 'printService'];

    angular.module('sw_layout').service('fsWorkOrderService', fsWorkOrderService);



})(angular, bootbox);