(function (angular) {
    'use strict';
    
    function fsrequestService(modalService, crudContextHolderService, applicationService, validationService) {

        function buildDatamap(schema) {
            const datamap = {};
            schema.displayables.forEach(d => {
                if (d.isHidden) {
                    return;
                }
                if (d.target) {
                    datamap[d.target] = null;
                } else {
                    datamap[d.attribute] = null;
                }
            });
            return datamap;
        }

        function postSave(saveDatamap, callback, rollback) {
            callback(saveDatamap);
            return applicationService.save({
                dispatchedByModal: false
            }).then(() => {
                modalService.hide();
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

        function validatePackage() {
            return validationService.validateCurrent().length <= 0;
        }

        function addAttachments(item, relationship) {
            const parentDataMap = crudContextHolderService.rootDataMap();
            const attachs = parentDataMap[relationship];
            if (attachs && !item[relationship]) {
                item[relationship] = [];
                angular.forEach(attachs, (attach) => {
                    const id = attach["docinfo_.urlparam1"].substr(9);
                    if (id === item["id"] + "") {
                        item[relationship].push(attach);
                    }
                });
            }
        }

        function defaultSendTime() {
            const date = new Date();
            const toNextDay = (date.getUTCHours() <= 7);
            const currentOffSet = date.getTimezoneOffset();
            const diff = (420 - currentOffSet) / 60; // AZ timezone = -7
            date.setHours(17, 0, 0, 0);
            date.addHours(diff);
            if (toNextDay) {
                date.setDate(date.getDate() + 1);
            }
            return date;
        }

        const service = {
            buildDatamap,
            postSave,
            postDelete,
            validatePackage,
            addAttachments,
            defaultSendTime
        };
        return service;
    }

    angular.module("firstsolar").clientfactory("fsrequestService", ["modalService", "crudContextHolderService", "applicationService", "validationService", fsrequestService]);
})(angular);