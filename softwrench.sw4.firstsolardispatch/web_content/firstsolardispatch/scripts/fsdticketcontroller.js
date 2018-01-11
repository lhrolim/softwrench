
(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .controller("fsdDispatchController", ["$scope", "fsdTicketService", function ($scope, fsdTicketService) {

            function doInit() {
                const localModel = fsdModel;

                const serverUrl = localModel.ServerUrl;
                const hashKey = localModel.HashKey;

                const dmJSON = localModel.TicketJSON;

                const dm = JSON.parse(dmJSON);
                dm["inverters_"] = dm["inverters"];
                dm["site_.siteid"] = dm["siteId"];

                return fsdTicketService.doCreateWorkOrder(serverUrl, dm, hashKey);
            }


            doInit();


        }]);

})(angular);