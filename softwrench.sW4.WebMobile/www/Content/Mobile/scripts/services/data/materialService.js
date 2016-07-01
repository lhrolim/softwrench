(function (angular, _) {
    "use strict";

    function materialService(dao, associationConstants, offlineSchemaService) {
        //#region Utils

        //#endregion

        //#region Public methods

        function itemSelected(event) {
            const schema = event.schema;
            const datamap = event.datamap;
            const value = event.newValue;
            datamap["description"] = datamap["item_.description"];
            dao.findByQuery("AssociationData", `application='offlineinvbalances' and datamap like '%"itemnum":"${value}"%'`)
                .then(r => {
                    // set options for storeroom
                    const options = _.chain(r)
                        .map(e => e.datamap)
                        .groupBy("location")
                        .pairs()
                        .map(kv => {
                            const [storeloc, invbals] = kv;
                            const bins = invbals.map(i => _.omit(i, ["location"]));
                            return new associationConstants.Option(storeloc, storeloc, { bins });
                        })
                        .value();

                    const storeRoomField = offlineSchemaService.getFieldByAttribute(schema, "storeloc");
                    storeRoomField.options = options;
                });
        }

        //#endregion

        //#region Service Instance
        const service = {
            itemSelected,
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services")
        .factory("materialService", ["swdbDAO", "associationConstants", "offlineSchemaService", materialService]);

    //#endregion

})(angular, _);