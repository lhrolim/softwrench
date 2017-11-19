
(function (angular) {
    "use strict";

    const day = 24 * 60 * 60 * 1000;

    class fsLaborMobileService {

        constructor($rootScope,securityService, searchIndexService, offlineEntities, swdbDAO) {
            this.$rootScope = $rootScope;
            this.securityService = securityService;
            this.searchIndexService = searchIndexService;
            this.entities = offlineEntities;
            this.dao = swdbDAO;

            this.$rootScope.$on("sw_itemrestored",(event,item)=>{
                swdbDAO.executeQuery({query:"delete from DataEntry where application = 'tslabor' and textindex02 = ? and rowstamp is null",args:[item.datamap.wonum] })
            })

        }


        laborCode() {
            const user = this.securityService.currentFullUser();
            if (user == null) {
                return this.securityService.logout();
            }
            return user.properties["laborcode"];
        }

        //#region Menu whereclauses
        // `assignment_`.dateindex01 = scheduled date without time
        getCurrentWeekLaborsWhereClause() {
            const now = new Date();

            const first = now.getDate() - now.getDay(); // First day is the day of the month - the day of the week
            const last = first + 6; // last day is the first day + 6

            const firstday = new Date(now.setDate(first)).getTime();
            const lastday = new Date(now.setDate(last)).getTime();

            const laborcode = this.laborCode();
            //testinga

            return "`root`.dateindex01 >= {0} and `root`.dateindex01 < {1} and `root`.textindex01 = '{2}'".format(firstday, lastday, laborcode);
        }

        getCurrentMonthLaborsWhereClause() {
            const now = new Date();

            const beginOfMonth = new Date(now.getFullYear(), now.getMonth(), 1, 0, 0, 0).getTime();
            const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 1, 0, 0, 0).getTime();

            const laborcode = this.laborCode();

            return "`root`.dateindex01 >= {0} and `root`.dateindex01 < {1} and `root`.textindex01 = '{2}'".format(beginOfMonth, endOfMonth, laborcode);
        }

        generateTsLaborDataEntryQuery(compositionData, runningLabor = false) {
            const isLocal = !compositionData.id;
            const id = persistence.createUUID();
            let remoteId = compositionData.id
            //storing local id as the remoteid for local created entries
            let rowstamp = compositionData.approwstamp;
            if (isLocal){
                //this first flag will control when we should show a running labor icon or not
                compositionData["#runninglabor"] = runningLabor;
                //using this so that replace or insert works for locally created entries
                remoteId = !!compositionData["#localswdbid"] ? compositionData["#localswdbid"]: id;
                rowstamp = null;
            }

            
            const newJson = compositionData.jsonFields || JSON.stringify(compositionData); //keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);
            const datamap = compositionData.jsonFields ? JSON.parse(compositionData.jsonFields) : compositionData; //keeping backwards compatibility //newJson = 
            //TODO: bring indexes from metadata... right now they are hardcoded here
            const idx = this.searchIndexService.buildIndexes(['laborcode','refwo'], [], ['enterdate'], datamap);
            const query = this.entities.DataEntry.insertOrReplacePattern;

            const insertOrUpdateQuery = { query, args: ["tslabor", newJson, remoteId, rowstamp, id, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3] };


            return {query:insertOrUpdateQuery, wonum: datamap["refwo"]};
        }

        insertTsLaborDataEntry(compositionData, runningLabor) {
            const query = this.generateTsLaborDataEntryQuery(compositionData, runningLabor).query;
            return this.dao.executeQuery(query);
        }


    }


    fsLaborMobileService["$inject"] = ["$rootScope","securityService", "searchIndexService", "offlineEntities", "swdbDAO"];

    angular.module("sw_mobile_services").service("fsLaborOfflineService", fsLaborMobileService);

})(angular);