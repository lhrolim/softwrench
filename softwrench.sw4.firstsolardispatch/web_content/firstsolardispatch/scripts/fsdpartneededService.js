(function (angular) {
    "use strict";

    class fsdPartNeededService {

        constructor(crudContextHolderService) {
            this.crudContextHolderService = crudContextHolderService;
        }

        deliveryMethodSelected({ fields, parentdata }) {
            if (fields && fields.deliverymethod === "shipment") {
                fields.deliverylocation = parentdata["site_.wherehouseaddress"];
            } else {
                fields.deliverylocation = null;
                fields.expecteddate = null;
            }
        }
    }

    fsdPartNeededService.$inject = ["crudContextHolderService"];

    angular.module("firstsolardispatch").clientfactory("fsdPartNeededService", fsdPartNeededService);

})(angular);