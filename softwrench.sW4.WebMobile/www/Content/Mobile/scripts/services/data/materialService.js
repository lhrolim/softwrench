(function (angular, _) {
    "use strict";

    function materialService(dao, associationConstants, offlineSchemaService, offlineAssociationService, swAlertPopup) {
        //#region Utils

        function setOptionList(schema, datamap, target, options) {
            const targetField = offlineSchemaService.getFieldByAttribute(schema, target);
            targetField.options = options; // providing options

            if (options.length === 1) { // only a single option: select it -> expose extrafields and set value 
                const selectedOption = options[0];
                offlineAssociationService.updateExtraProjectionsForOptionField(selectedOption, targetField.associationKey);
                datamap[target] = selectedOption.value;
            }
        }

        const cleanItemData = (datamap) => {
            datamap["item_.description"] = null;
            datamap["#description"] = null;
        }

        const cleanMaterialData = (datamap, materialSelected) => {
            const nullNumberValue = materialSelected ? 0 : null;
            datamap["description"] = null;
            datamap["unitcost"] = nullNumberValue;
        }

        //#endregion

        //#region Public methods

        function lineSelected(event) {
            const datamap = event.datamap;
            const linetype = datamap.linetype;
            cleanItemData(datamap);
            cleanMaterialData(datamap, linetype === "MATERIAL");
        }

        function itemSelected(event) {
            const schema = event.schema;
            const datamap = event.datamap;
            const value = event.newValue;

            if (!value) {
                cleanItemData(datamap);
                return;
            }

            datamap["#description"] = datamap["item_.description"];
            return dao.findByQuery("AssociationData", `application='offlineinventory' and datamap like '%"itemnum":"${value}"%'`)
                .then(r => {
                    if (!r || r.length <= 0) {
                        // should not happen: server whereclauses should only bring items that have inventory entries associated with it
                        return swAlertPopup.show({
                            title: "Item not available",
                            template: "This item is not available in the inventory.<br>Please select a different one"
                        }, 3000)
                        .then(() => {
                            cleanItemData(datamap);
                            datamap["itemnum"] = "null$ignorewatch";
                        });
                    }
                    const options = _.chain(r)
                        .map(a => a.datamap["location"])
                        .uniq()
                        .map(l => new associationConstants.Option(l))
                        .value();

                    setOptionList(schema, datamap, "storeloc", options);
                });
        }

        //#endregion

        //#region Service Instance
        const service = {
            lineSelected,
            itemSelected
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services")
        .factory("materialService", ["swdbDAO", "associationConstants", "offlineSchemaService", "offlineAssociationService", "swAlertPopup", materialService]);

    //#endregion

})(angular, _);