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

        migrationGroup.addMigration("Data Entry Indexes", {
            up: function () {
                this.addIndex("DataEntry", "textindex01",false);
                this.addIndex("DataEntry", "textindex02",false);
                this.addIndex("DataEntry", "textindex03",false);
                this.addIndex("DataEntry", "textindex04",false);
                this.addIndex("DataEntry", "textindex05", false);

                this.addIndex("DataEntry", "numericindex01", false);
                this.addIndex("DataEntry", "numericindex02", false);

                this.addIndex("DataEntry", "dateindex01", false);
                this.addIndex("DataEntry", "dateindex02", false);
                this.addIndex("DataEntry", "dateindex03", false);

                this.addIndex("AssociationData", "textindex01", false);

                this.addIndex("AssociationData", ["application","textindex01"], false);
                this.addIndex("AssociationData", ["application","textindex02"], false);

                this.addIndex("AssociationData", "textindex02", false);


                this.addIndex("AssociationData", "dateindex01", false);
                this.addIndex("AssociationData", "dateindex02", false);
                this.addIndex("AssociationData", "dateindex03", false);
            },
            down: function () {
                this.removeIndex("DataEntry", "textindex01");
                this.removeIndex("DataEntry", "textindex02");
                this.removeIndex("DataEntry", "textindex03");
                this.removeIndex("DataEntry", "textindex04");
                this.removeIndex("DataEntry", "textindex05");

                this.removeIndex("DataEntry", "numericindex01");
                this.removeIndex("DataEntry", "numericindex02");

                this.removeIndex("DataEntry", "dateindex01");
                this.removeIndex("DataEntry", "dateindex02");
                this.removeIndex("DataEntry", "dateindex03");

                this.removeIndex("AssociationData", "textindex01");
                this.removeIndex("AssociationData", "textindex02");

                this.removeIndex("AssociationData", "dateindex01");
                this.removeIndex("AssociationData", "dateindex02");
                this.removeIndex("AssociationData", "dateindex03");
            }
        });
    }]);
    //#endregion

    

})(angular);