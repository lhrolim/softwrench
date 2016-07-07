(function (angular, _) {
    "use strict";

    function materialService(dao, associationConstants, offlineSchemaService, offlineAssociationService) {
        //#region Utils

        function provideOptionList(schema, datamap, list, parentKey, target, extraChildrenFieldName) {
            const options = _.chain(list)
                        .groupBy(parentKey) // grouping by parent key (denormalized list, has repetitions of locations and binnums)
                        .pairs()
                        .map(kv => {
                            const [key, children] = kv;
                            const childrenList = children.map(i => _.omit(i, [parentKey]));
                            return new associationConstants.Option(key, key, { [extraChildrenFieldName]: childrenList });
                        })
                        .value();

            const targetField = offlineSchemaService.getFieldByAttribute(schema, target);
            targetField.options = options; // providing options

            if (options.length === 1) { // only a single option: select it -> expose extrafields and set value 
                const selectedOption = options[0];
                offlineAssociationService.updateExtraProjectionsForOptionField(selectedOption, targetField.associationKey);
                datamap[target] = selectedOption.value;
            }
        }

        const numberValue = value => !value ? 0 : parseFloat(value.replace(",", "."));

        const setItemValues = (datamap, values) => {
            datamap["curbal"] = numberValue(values["curbal"]);
            datamap["physcnt"] = numberValue(values["physcnt"]);
            datamap["itemsetid"] = values["itemsetid"];
            datamap["conditioncode"] = values["conditioncode"];
        };

        const cleanItemData = (datamap, itemSelected) => {
            const numberValue = itemSelected ? 0 : null;
            datamap["storeloc"] = "null$ignorewatch";
            datamap["binnum"] = "null$ignorewatch";
            datamap["lotnum"] = "null$ignorewatch";
            datamap["storeloc_.#bins"] = null;
            datamap["binnum_.#lots"] = null;
            datamap["lotnum_.#itemvalues"] = null;
            datamap["curbal"] = numberValue;
            datamap["physcnt"] = numberValue;
            datamap["itemsetid"] = null;
            datamap["conditioncode"] = null;
            datamap["item_.description"] = null;
        }

        const cleanMaterialData = (datamap, materialSelected) => {
            const numberValue = materialSelected ? 0 : null;
            datamap["description"] = null;
            datamap["unitcost"] = numberValue;
        }

        //#endregion

        //#region Public methods

        function lineSelected(event) {
            const datamap = event.datamap;
            const linetype = datamap.linetype;
            cleanItemData(datamap, linetype === "ITEM");
            cleanMaterialData(datamap, linetype === "MATERIAL");
        }

        function itemSelected(event) {
            const schema = event.schema;
            const datamap = event.datamap;
            const value = event.newValue;
            datamap["#description"] = datamap["item_.description"];
            
            // cleanup
            cleanItemData(datamap, true);

            if (!value) return;

            // provide option list of storerooms for the selected item
            dao.findByQuery("AssociationData", `application='offlineinvbalances' and datamap like '%"itemnum":"${value}"%'`)
                .then(r => 
                    provideOptionList(schema, datamap, r.map(e => e.datamap), "location", "storeloc", "#bins"));
        }

        function storeRoomSelected(event) {
            const value = event.newValue;
            if (!value) return;
            const schema = event.schema;
            const datamap = event.datamap;
            const bins = datamap["storeloc_.#bins"];
            if(!bins) return;
            // provide option list of bins
            provideOptionList(schema, datamap, bins, "binnum", "binnum", "#lots");
        }

        function binSelected(event) {
            const schema = event.schema;
            const datamap = event.datamap;
            const lots = datamap["binnum_.#lots"];
            if (!lots) return;
            // provide list of lots
            const lotnumField = offlineSchemaService.getFieldByAttribute(schema, "lotnum");
            const options = lots.map(l => new associationConstants.Option(l.lotnum, l.lotnum, { "#itemvalues": l }));
            if (options.length === 1) { // selecting single option
                const selectedOption = options[0];
                offlineAssociationService.updateExtraProjectionsForOptionField(selectedOption, lotnumField.associationKey);
                setItemValues(datamap, selectedOption.extrafields["#itemvalues"]);
                datamap["lotnum"] = `${selectedOption.value}$ignorewatch`;
            }
        }

        function lotSelected(event) {
            const datamap = event.datamap;
            const itemvalues = datamap["lotnum_.#itemvalues"];
            if (!itemvalues) return;
            setItemValues(datamap, itemvalues);
        }

        //#endregion

        //#region Service Instance
        const service = {
            lineSelected,
            itemSelected,
            storeRoomSelected,
            binSelected,
            lotSelected
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services")
        .factory("materialService", ["swdbDAO", "associationConstants", "offlineSchemaService", "offlineAssociationService", materialService]);

    //#endregion

})(angular, _);