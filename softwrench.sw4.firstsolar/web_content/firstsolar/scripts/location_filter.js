
(function (angular) {
    "use strict";

    function locationFilterService() {
        //#region Utils

        //#endregion

        //#region Public methods

        function operate(datamap, schema, filter) {
            var location = datamap.location || "";
            return "%" + location + "%";
        }

        //#endregion

        //#region Service Instance
        var service = {
            operate: operate
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("locationFilterService", locationFilterService);

    //#endregion

})(angular);