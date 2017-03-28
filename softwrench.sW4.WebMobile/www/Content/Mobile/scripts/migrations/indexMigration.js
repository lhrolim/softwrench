
(function (angular) {
    "use strict";

    class indexCreatorService {

        constructor(swdbDAO, $log, loadingService) {
            this.swdbDAO = swdbDAO;
            this.$log = $log;
            this.loadingService = loadingService;
        }

        createIndexAfterFirstSync() {

            const queries = [];
            const log = this.$log.get("indexCreatorService#createIndexAfterFirstSync", ["persistence"]);

            
            if (isRippleEmulator()) {
                return;
            }
            log.info("creating sql indexes");
            this.loadingService.show("Applying Indexes");

            queries.push(this.swdbDAO.addIndexQuery("DataEntry", "textindex01", false));
            queries.push(this.swdbDAO.addIndexQuery("DataEntry", "textindex02", false));
//
            queries.push(this.swdbDAO.addIndexQuery("DataEntry", "dateindex01", false));
            queries.push(this.swdbDAO.addIndexQuery("DataEntry", ["application", "remoteid"], true));
//
            queries.push(this.swdbDAO.addIndexQuery("AssociationData", "textindex01", false));
            queries.push(this.swdbDAO.addIndexQuery("AssociationData", "textindex02", false));
//
            queries.push(this.swdbDAO.addIndexQuery("AssociationData", ["application", "textindex01"], false));


            this.swdbDAO.executeQueries(queries).then(r => {
                log.info("sql indexes created successfully");
                this.loadingService.hide();
            });
        }

        dropIndexes() {

            const log = this.$log.get("indexCreatorService#dropIndexes",["persistence"]);

            if (isRippleEmulator()) {
                return;
            }

            log.info("dropping sql indexes");

            this.swdbDAO.dropIndex("DataEntry", "textindex01");
            this.swdbDAO.dropIndex("DataEntry", "textindex02");


            this.swdbDAO.dropIndex("DataEntry", "dateindex01");

            this.swdbDAO.dropIndex("AssociationData", "textindex01");
            this.swdbDAO.dropIndex("AssociationData", "textindex02");
            this.swdbDAO.dropIndex("AssociationData", ["application", "textindex01"]);
            this.swdbDAO.dropIndex("DataEntry", ["application", "remoteid"]);

            log.info("sql indexes dropped successfully");

        }

    }


    indexCreatorService["$inject"] = ["swdbDAO", "$log", "loadingService"];

    angular.module("persistence.offline").service("indexCreatorService", indexCreatorService);

})(angular);


(function (angular) {
    "use strict";

    try {
        angular.module("persistence.offline");
    } catch (err) {
        return;
    }

    //#region audit.offline migrations
    angular.module("persistence.offline").config(["offlineMigrationsProvider", function (offlineMigrationsProvider) {
        const migrationGroup = offlineMigrationsProvider.createMigrationGroup(15, "index migrations");

        //        migrationGroup.addMigration("Data Entry Indexes", {
        //            up: function () {
        //                this.addIndex("DataEntry", "textindex01",false);
        //                this.addIndex("DataEntry", "textindex02",false);
        //                
        //                this.addIndex("DataEntry", "numericindex01", false);
        //                this.addIndex("DataEntry", "numericindex02", false);
        //
        //                this.addIndex("DataEntry", "dateindex01", false);
        //                
        //                this.addIndex("AssociationData", "textindex01", false);
        //                this.addIndex("AssociationData", "textindex02", false);
        //
        //                this.addIndex("AssociationData", ["application","textindex01"], false);
        //
        //            },
        //            down: function () {
//                        this.removeIndex("DataEntry", "textindex01");
        //                this.removeIndex("DataEntry", "textindex02");
        //                this.removeIndex("DataEntry", "textindex03");
        //                this.removeIndex("DataEntry", "textindex04");
        //                this.removeIndex("DataEntry", "textindex05");
        //
        //                this.removeIndex("DataEntry", "numericindex01");
        //                this.removeIndex("DataEntry", "numericindex02");
        //
        //                this.removeIndex("DataEntry", "dateindex01");
        //                this.removeIndex("DataEntry", "dateindex02");
        //                this.removeIndex("DataEntry", "dateindex03");
        //
        //                this.removeIndex("AssociationData", "textindex01");
        //                this.removeIndex("AssociationData", "textindex02");
        //
        //                this.removeIndex("AssociationData", "dateindex01");
        //                this.removeIndex("AssociationData", "dateindex02");
        //                this.removeIndex("AssociationData", "dateindex03");
        //            }
        //        });
    }]);
    //#endregion



})(angular);