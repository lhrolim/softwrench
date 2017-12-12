(function (angular) {
    "use strict";

    let clearItem;

    class fsMaterialService {

        constructor(crudContextHolderService) {
            this.crudContextHolderService = crudContextHolderService;

            clearItem = (dm) => {
                dm["itemnum"] = "null$ignorewatch";
                dm["description"] = null;
                dm["#description"] = null;
            };
        }

        lineSelected(event) {
            const dm = event.fields;
            clearItem(dm);
            dm["storeloc"] = "null$ignorewatch";
            dm["unitcost"] = 0.00;
        }

        storeRoomSelected(event) {
            clearItem(event.fields);
        }

        categorySelected(event) {
            clearItem(event.fields);
        }

        itemSelected(event) {
            const dm = event.fields;
            const desc = dm["item_.description"];
            dm["description"] = desc;
            dm["#description"] = desc;
        }
    }

    fsMaterialService.$inject = ['crudContextHolderService'];

    angular.module("firstsolar").clientfactory('fsMaterialService', fsMaterialService);

})(angular);