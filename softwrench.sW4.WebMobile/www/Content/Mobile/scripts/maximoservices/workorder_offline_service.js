
(function (offlineMaximoApplications) {
    "use strict";

    function workorderOfflineService() {


        const day = 24 * 60 * 60 * 1000;

        //#region Utils

        //#endregion

        //#region Public methods

        function preSync(datamap,originaldatamap) {
//            datamap['reportdate'] = new Date();
            //            originaldatamap['reportdate'] = datamap['reportdate'];
            if (originaldatamap != null && datamap.status !== originaldatamap.status) {
                datamap["#hasstatuschange"] = true;
                datamap["#forcestatuschance"] = true;

            }
        }


         //#region Menu whereclauses
        // dateindex01 = scheduled start date
        function getTodayWosWhereClause() {
            const now = new Date();
            const todayTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime();
            const tomorrowTime = todayTime + day;
            return `\`root\`.dateindex01 >= ${todayTime} and 'root'.dateindex01 < ${tomorrowTime}`;
        }

        // workorder.dateindex01 = scheduled start date
        function getPastWosWhereClause() {
            const now = new Date();
            const todayTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime();
            return `\`root\`.dateindex01 < ${todayTime}`;
        }

        // workorder.dateindex01 = scheduled start date
        function getFutureWosWhereClause() {
            const now = new Date();
            const tomorrowTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime() + day;
            return `\`root\`.dateindex01 >= ${tomorrowTime}`;
        }

        function getProblematicWosWhereClause() {
            return "`root`.hasProblem = 1";
        }

        function getCreatedWosWhereClause() {
            return "`root`.remoteId is null";
        }


        //#endregion

        //#region Service Instance

        var service = {
            preSync,
            getFutureWosWhereClause,
            getPastWosWhereClause,
            getProblematicWosWhereClause,
            getCreatedWosWhereClause,
            getTodayWosWhereClause
        };

        return service;

        //#endregion
    }

    //#region Service registration

    offlineMaximoApplications.factory("workorderOfflineService", [workorderOfflineService]);

    //#endregion

})(offlineMaximoApplications);
