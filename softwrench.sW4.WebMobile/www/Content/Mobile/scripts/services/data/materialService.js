(function (angular, _) {
    "use strict";

    function materialService() {
        //#region Utils

        const cleanItemData = (datamap) => {
            datamap["offlineitem_.description"] = null;
            datamap["#description"] = null;
            datamap["description"] = null;
        }

        const cleanMaterialData = (datamap, materialSelected) => {
            const nullNumberValue = materialSelected ? 0 : null;
            datamap["description"] = null;
            datamap["unitcost"] = nullNumberValue;
        }

        //#endregion

        //#region Public methods

        /**
         * @returns {String} whereclause that filters locations that are storerooms 
         */
        const getStoreRoomWhereClause = () =>
            "textindex02='STOREROOM' and textindex03='OPERATING'";

        /**
         * @returns {String} whereclause that filters items that are stocked in the selected storeroom 
         */
        const getAvailableItemsWhereClause = () =>
            "textindex01 in (select textindex01 from AssociationData where application='offlineinventory' and textindex02=@storeloc)";
        
        /**
         * Clears datamap.
         * 
         * @param {events.afterchange} event 
         */
        function lineSelected(event) {
            const datamap = event.datamap;
            const linetype = datamap.linetype;
            cleanItemData(datamap);
            datamap["storeloc"] = "null$ignorewatch";
            datamap["itemnum"] = "null$ignorewatch";
            cleanMaterialData(datamap, linetype === "MATERIAL");
            datamap["qtyrequested"] = 1;
        }

        /**
         * Clear item data.
         * 
         * @param {events.afterchange} event 
         */
        function storeRoomSelected(event) {
            const datamap = event.datamap;
            if (!datamap["itemnum"]) return;
            datamap["itemnum"] = "null$ignorewatch";
            cleanItemData(datamap);
        }

        /**
         * Sets description from selected item.
         * 
         * @param {events.afterchange} event 
         */
        function itemSelected(event) {
            const datamap = event.datamap;
            const value = event.newValue;
            if (!value) {
                cleanItemData(datamap);
                return;
            }
            const description = datamap["offlineitem_.description"];
            datamap["#description"] = description;
            datamap["description"] = description;
        }

        //#endregion

        //#region Service Instance
        const service = {
            lineSelected,
            itemSelected,
            storeRoomSelected,
            getStoreRoomWhereClause,
            getAvailableItemsWhereClause
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services")
        .factory("materialService", [materialService]);

    //#endregion

})(angular, _);