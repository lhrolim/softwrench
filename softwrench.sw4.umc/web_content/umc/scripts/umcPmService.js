(function (angular) {
    "use strict";

    let pollingUnregisterFn;

    class umcPdService {

        constructor($interval, $rootScope, crudContextHolderService, searchService, modalService, schemaService, alertService, restService, redirectService, validationService) {
            this.$interval = $interval;
            this.$rootScope = $rootScope;
            this.searchService = searchService;
            this.crudContextHolderService = crudContextHolderService;
            this.modalService = modalService;
            this.schemaService = schemaService;
            this.alertService = alertService;
            this.restService = restService;
            this.redirectService = redirectService;
            this.validationService = validationService;
            this.$rootScope.$on(JavascriptEventConstants.ApplicationRedirected, () => {
                if (pollingUnregisterFn) {
                    //cancelling polling interval
                    $interval.cancel(pollingUnregisterFn);
                    pollingUnregisterFn = null;
                }

            });


        }

        createBatch() {

        }

        openUpdateLdModal() {
            this.schemaService.getSchema("pm", "updateld").then(schema => {
                this.modalService.show(
                    schema,
                    { dummy: "dummy" },  // to force datamap load
                    { cssclass: "largemodal", removecrudmodalclass: true },
                    this.updateLd.bind(this),
                    () => this.modalService.hide()
                );
            });
        }

        updateLd() {
            const validation = this.validationService.validateCurrent("#modal");
            if (validation.length > 0) {
                return;
            }
            const dm = this.crudContextHolderService.rootDataMap("#modal");
            const json = {};
            const selectionModel = this.crudContextHolderService.getSelectionModel();
            const selectionBuffer = selectionModel.selectionBuffer;
            json.data = [];
            angular.forEach(selectionBuffer, (entry, id) => {
                if (!selectionBuffer.hasOwnProperty(id)) return;
                json.data.push({ id, pmnum: entry.pmnum });
            });
            json.longDescription = dm["commonld"];
            json.alias = dm["alias"];
            return this.alertService.confirm(`Are you sure you want to update these ${json.data.length} pms`)
                .then(() => {
                    return this.restService.invokePost("UmcPmBatch", "SubmitBatch",
                        {}, json, (data) => {
                            this.modalService.hide();
                            const batchId = data.resultObject.batchid;
                            this.alertService.success(`Batch ${batchId} successfully created. You can check its status at the Update History Menu`, true);
                           
                        },
                        null);
                }).finally(()  => {
                    this.modalService.hide();
                });
        }

        isCommandEnabled() {
            const selectionModel = this.crudContextHolderService.getSelectionModel();
            const selectionBuffer = selectionModel.selectionBuffer;
            return (selectionBuffer && Object.values(selectionBuffer).length > 0);
        }

        configurePolling(scope, schema, datamap, parameters) {
            if (pollingUnregisterFn) {
                return;
            }

            var anySubmitting = datamap.some(a => status.toLowerCase().startsWith("submitting"));

            if (!!anySubmitting) {
                //there are no items being submitted, no need to configure a polling
                return;
            }
            var that = this;
            pollingUnregisterFn = this.$interval(function () {
                that.searchService.refreshGrid(null, null, { avoidspin: true });
            }, 5000);
        }

    }

    umcPdService.$inject = ["$interval", "$rootScope", "crudContextHolderService", "searchService", "modalService", "schemaService", "alertService", "restService", "redirectService", "validationService"];

    angular.module("umc").clientfactory("umcPdService", umcPdService);

})(angular);