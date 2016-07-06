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

        const curbalNumberValue = curbal => !curbal ? null : parseInt(window.replaceAll(curbal, ",", "")); 

        //#endregion

        //#region Public methods

        function itemSelected(event) {
            const schema = event.schema;
            const datamap = event.datamap;
            const value = event.newValue;
            datamap["description"] = datamap["#description"] = datamap["item_.description"];
            
            // cleanup
            datamap["storeloc"] = "null$ignorewatch";
            datamap["binnum"] = "null$ignorewatch";
            datamap["lotnum"] = "null$ignorewatch";
            datamap["storeloc_.#bins"] = null;
            datamap["binnum_.#lots"] = null;
            datamap["lotnum_.#itemvalues"] = null;
            datamap["curbal"] = null;
            datamap["conditioncode"] = null;

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
                datamap["curbal"] = selectedOption.extrafields["#itemvalues"].curbal;
                datamap["conditioncode"] = selectedOption.extrafields["#itemvalues"].conditioncode;
                datamap["lotnum"] = `${selectedOption.value}$ignorewatch`;
            }
        }

        function lotSelected(event) {
            const datamap = event.datamap;
            const itemvalues = datamap["lotnum_.#itemvalues"];
            if (!itemvalues) return;
            datamap["curbal"] = itemvalues.curbal;
            datamap["conditioncode"] = itemvalues.conditioncode;
        }

        //#endregion

        //#region Service Instance
        const service = {
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