
(function (angular) {
    "use strict";

    function dummyService() {
        //#region Utils

        //#endregion

        //#region Public methods

        function operate(datamap, schema, filter) {
            console.log(schema);
            console.log(filter);
            var a = datamap.a || "";
            var b = datamap.b || "";
            return "%" + a + b + "%";
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

    angular.module("sw_layout").factory("dummyService", dummyService);

    //#endregion

})(angular);