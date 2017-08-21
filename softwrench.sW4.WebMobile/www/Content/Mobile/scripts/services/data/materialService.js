(function (angular, _) {
    "use strict";

    function materialService(dao) {
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
         * textindex02 = offlinelocation.type
         * textindex03 = offlinelocation.status
         */
        const getStoreRoomWhereClause = function() {
            return "textindex02='STOREROOM' and textindex03='OPERATING' and textindex01 in (select textindex02 from AssociationData where application='offlineinventory' and textindex01 = @itemnum and textindex03 = @category)";
        };

        /**
         * @returns {String} whereclause that filters items that are stocked in the selected storeroom
         * textindex01 = offlineitem.itemnum
         * textindex01 = offlineinventory.itemnum
         * textindex02 = offlineinventory.location
         * textindex03 = offlineinventory.category
         */
        const getAvailableItemsWhereClause = () =>
            "textindex01 in (select textindex01 from AssociationData where application='offlineinventory' and textindex02 in (select textindex01 from AssociationData where application='offlinelocation' and textindex02='STOREROOM' and textindex03='OPERATING') and textindex03 = @category)";
        
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
            datamap["storeloc"] = null;
        }

        /**
         * Clear item data.
         * 
         * @param {events.afterchange} event
         */
        function categorySelected(event) {
            const datamap = event.datamap;
            if (!datamap["itemnum"]) return;
            datamap["itemnum"] = "null$ignorewatch";
            cleanItemData(datamap);
            datamap["storeloc"] = null;
        }

        //#endregion

        //#region Service Instance
        const service = {
            lineSelected,
            itemSelected,
            categorySelected,
            getStoreRoomWhereClause,
            getAvailableItemsWhereClause
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services").factory("materialService", ["swdbDAO", materialService]);

    //#endregion

})(angular, _);