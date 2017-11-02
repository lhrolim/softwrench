(function (angular) {
    'use strict';

    function fsdCompositionService(modalService, crudContextHolderService, applicationService, validationService, compositionService) {

        function innerBuildDatamap(displayables, datamap) {
            displayables.forEach(d => {
                if (d.isHidden || d.type === "ApplicationSection") {
                    if (d.displayables) {
                        innerBuildDatamap(d.displayables, datamap);
                    }
                    return;
                }
                if (d.target) {
                    datamap[d.target] = null;
                } else {
                    datamap[d.attribute] = null;
                }
            });
        }

        function buildDatamap(schema) {
            const datamap = {};
            innerBuildDatamap(schema.displayables, datamap);
            return datamap;
        }

        function postSave(saveDatamap, callback, rollback) {
            callback(saveDatamap);
            return applicationService.save({
                dispatchedByModal: false
            }).then((result) => {
                const parentSchema = crudContextHolderService.currentSchema();
                const parentDm = crudContextHolderService.rootDataMap();
                return compositionService.populateWithCompositionData(parentSchema, parentDm, true).then(() => {
                    modalService.hide(true);
                    return result;
                });
            }).catch(() => {
                rollback();
            });
        }

        function postDelete(callback, rollback) {
            callback();
            return applicationService.save({
                dispatchedByModal: false
            }).catch(() => {
                rollback();
            });
        }

        function validateParent() {
            return validationService.validateCurrent().length <= 0;
        }

        function modalProps() {
            return { cssclass: "extra-height-modal", closeAfterSave: false };
        };

        const service = {
            buildDatamap,
            postSave,
            postDelete,
            validateParent,
            modalProps
        };
        return service;
    }

    angular.module("firstsolardispatch").clientfactory("fsdCompositionService", ["modalService", "crudContextHolderService", "applicationService", "validationService", "compositionService", fsdCompositionService]);
})(angular);