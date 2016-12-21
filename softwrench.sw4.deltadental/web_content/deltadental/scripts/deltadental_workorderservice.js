(function (angular) {
    "use strict";

    function workorderService() {
        //#region Utils
        //#endregion

        //#region Public methods
        function onStatusChanged($event) {
            var status = $event.fields["status"];
//            if (status === "INPRG") {
//                $event.fields["actstart"] = Date.now();
//            } else if (status === "COMP" || status === "CLOSE") {
//                $event.fields["actfinish"] = Date.now();
//            }
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
    angular.module("sw_layout").service("deltadental.workorderService", [workorderService]);
    //#endregion

})(angular);