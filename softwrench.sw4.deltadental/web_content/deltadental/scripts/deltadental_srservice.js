(function (angular) {
    "use strict";

    function srService() {
        //#region Utils
        //#endregion

        //#region Public methods
        function onStatusChanged($event) {
            var status = $event.fields["status"];
            if (status === "INPROG") {
                $event.fields["actualstart"] = Date.now();
            } else if (status === "CLOSED" || status === "RESOLVED") {
                $event.fields["actualfinish"] = Date.now();
            }
        }
        //#endregion

        //#region Service Instance
        var service = {
            onStatusChanged: onStatusChanged
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").factory("deltadental.srService", [srService]);
    //#endregion

})(angular);