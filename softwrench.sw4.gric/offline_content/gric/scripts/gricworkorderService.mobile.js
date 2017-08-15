(function (angular) {
    "use strict";

    const day = 24 * 60 * 60 * 1000;

    class gricWorkorderOfflineService {

        constructor() {
        }


        //#region Menu whereclauses
        // dateindex01 = scheduled start date
        getTodayWosWhereClause() {
            const now = new Date();
            const todayTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime();
            const tomorrowTime = todayTime + day;
            return `\`root\`.dateindex01 >= ${todayTime} and 'root'.dateindex01 < ${tomorrowTime}`;
        }

        // workorder.dateindex01 = scheduled start date
        getPastWosWhereClause() {
            const now = new Date();
            const todayTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime();
            return `\`root\`.dateindex01 < ${todayTime}`;
        }

        // workorder.dateindex01 = scheduled start date
        getFutureWosWhereClause() {
            const now = new Date();
            const tomorrowTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0).getTime() + day;
            return `\`root\`.dateindex01 >= ${tomorrowTime}`;
        }
        //#endregion
    }

    gricWorkorderOfflineService["$inject"] = [];

    angular.module("maximo_offlineapplications").service("gricWorkorderOfflineService", gricWorkorderOfflineService);
})(angular);