
(function (offlineMaximoApplications) {
    "use strict";

    function workorderOfflineService() {

        //#region Utils

        //#endregion

        //#region Public methods

        function preSync(datamap) {
            datamap['reportdate'] = new Date();
        }

        //#endregion

        //#region Service Instance

        var service = {
            preSync: preSync
        };

        return service;

        //#endregion
    }

    //#region Service registration

    offlineMaximoApplications.factory("workorderOfflineService", [workorderOfflineService]);

    //#endregion

})(offlineMaximoApplications);
