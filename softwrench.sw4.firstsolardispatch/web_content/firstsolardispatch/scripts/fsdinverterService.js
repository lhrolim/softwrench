(function (angular) {
    "use strict";

    let beforeModalOpen, modalProps;

    class fsdInverterService {

        constructor($q, modalService, schemaCacheService, alertService, crudContextHolderService, applicationService, compositionService, fsdCompositionService) {
            this.$q = $q;
            this.modalService = modalService;
            this.schemaCacheService = schemaCacheService;
            this.alertService = alertService;
            this.crudContextHolderService = crudContextHolderService;
            this.applicationService = applicationService;
            this.compositionService = compositionService;
            this.fsdCompositionService = fsdCompositionService;

            beforeModalOpen = function(item) {
                const parentDm = crudContextHolderService.rootDataMap();
                item["siteid"] = parentDm["site_.siteid"];
                item["locationprefix"] = parentDm["site_.locationprefix"];
                if (item["partsrequired"] !== null && item["partsrequired"] !== undefined) {
                    item["partsrequired"] = item["partsrequired"] + "";
                }
                if (!item["failureclass"]) {
                    item["failureclass"] = "1";
                }
                delete item["parts"];
            }

            modalProps = function() {
                const props = fsdCompositionService.modalProps();
                props["cssclass"] = "largemodal";
                props["removecrudmodalclass"] = true;
                return props;
            }
        }

        openModalNew(item, callback, rollback) {
            if (!this.fsdCompositionService.validateParent()) {
                return;
            }
            this.schemaCacheService.fetchSchema("_Inverter", "newdetail").then((schema) => {
                const mergedItem = this.compositionService.buildMergedDatamap(this.fsdCompositionService.buildDatamap(schema), item);
                beforeModalOpen(mergedItem);
                this.modalService.show(schema, mergedItem, modalProps(), (saveDatamap) => {
                    this.fsdCompositionService.postSave(saveDatamap, callback, rollback);
                });
            });
        }

        openModalEdit(item, callback, rollback) {
            if (!this.fsdCompositionService.validateParent()) {
                return;
            }
            this.schemaCacheService.fetchSchema("_Inverter", "detail").then((schema) => {
                beforeModalOpen(item);

                const compositionPromise = item["parts_"] ? this.$q.when(null) : this.compositionService.getCompositionList("parts_", schema, item, 1, 100);
                compositionPromise.then(result => {
                    if (result) {
                        this.compositionService.resolveCompositions(result);
                    }
                    return result;
                }).then(() => {
                    this.modalService.show(schema, item, modalProps(), (saveDatamap) => {
                        this.fsdCompositionService.postSave(saveDatamap, callback, rollback);
                    });
                });
            });
        }

        deleteRow(item, callback, rollback) {
            if (!this.fsdCompositionService.validateParent()) {
                return;
            }
            this.alertService.confirm("Are you sure you want to delete this inverter?").then(() => {
                return this.fsdCompositionService.postDelete(callback, rollback);
            });
        }

        assetSelected(event) {
            const fields = event.fields;
            if (!fields["asset_"]) {
                return;
            }
            const dm = this.crudContextHolderService.rootDataMap("#modal");
            dm["assetuid"] = fields["asset_.assetuid"];
            dm["manufacturer"] = fields["asset_.manufacturer"];
            dm["model"] = fields["asset_.pluscmodelnum"];
        }

        save() {
            this.applicationService.save();
        }
    }

    fsdInverterService.$inject = ["$q", "modalService", "schemaCacheService", "alertService", "crudContextHolderService", "applicationService", "compositionService", "firstsolardispatch.fsdCompositionService"];

    angular.module("firstsolardispatch").clientfactory("fsdInverterService", fsdInverterService);

})(angular);