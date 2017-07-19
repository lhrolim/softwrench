(function (angular) {
    "use strict";
    angular.module("persistence.offline").config(["offlineMigrationsProvider", function (offlineMigrationsProvider) {
        // never change/remove/comment migrations or change group id/tag or change migration tag if you need to alter an old migration
        // consider creating a new one that undo the changes (unless you are absolutelly sure about it).

        const migrationGroup = offlineMigrationsProvider.createMigrationGroup(5, "offline migrations");

        migrationGroup.addMigration("table OptionFieldData", {
            up: function () {
                this.createTable("OptionFieldData", (t) => {
                     t.text("application");
                     t.text("schema");
                     t.text("providerAttribute");
                     t.text("optionkey");
                     t.text("optionvalue");
                     t.json("extraprojectionvalues");
                });
            },
            down: function () {
                this.dropTable("OptionFieldData");
            }
        });

        migrationGroup.addMigration("table AssociationData", {
            up: function () {
                this.createTable("AssociationData", (t) => {
                    t.text("application");
                    t.json("datamap");
                    t.integer("rowstamp");
                    t.text("textindex01");
                    t.text("textindex02");
                    t.text("textindex03");
                    t.text("textindex04");
                    t.text("textindex05");
                    t.numeric("numericindex01");
                    t.numeric("numericindex02");
                    t.date("dateindex01");
                    t.date("dateindex02");
                    t.date("dateindex03");
                });
                
            },
            down: function () {
                this.dropTable("AssociationData");
            }
        });

        migrationGroup.addMigration("table AssociationCache", {
            up: function () {
                this.createTable("AssociationCache", (t) => {
                    t.json("data");
                });
            },
            down: function () {
                this.dropTable("AssociationCache");
            }
        });

        migrationGroup.addMigration("table Batch", {
            up: function () {
                this.createTable("Batch", (t) => {
                    t.text("application");
                    t.date("sentdate");
                    t.date("completionDate");
                    t.date("lastChecked");
                    t.text("remoteId");
                    t.text("status");
                    t.varchar("syncoperation", 32);
                });
                this.addIndex("Batch", "syncoperation");
            },
            down: function () {
                this.removeIndex("Batch", "syncoperation");
                this.dropTable("Batch");
            }

        });

        migrationGroup.addMigration("table BatchItem", {
            up: function () {
                this.createTable("BatchItem", (t) => {
                    t.text("label");
                    t.text("status");
                    t.text("crudoperation");
                    t.varchar("problem", 32);
                    t.varchar("operation", 32);
                    t.varchar("dataentry", 32);
                    t.varchar("batch", 32);
                });
                this.addIndex("BatchItem", "problem");
                this.addIndex("BatchItem", "operation");
                this.addIndex("BatchItem", "dataentry");
                this.addIndex("BatchItem", "batch");
            },
            down: function () {
                this.removeIndex("BatchItem", "batch");
                this.removeIndex("BatchItem", "dataentry");
                this.removeIndex("BatchItem", "operation");
                this.removeIndex("BatchItem", "problem");
                this.dropTable("BatchItem");
            }
        });

        migrationGroup.addMigration("table CompositionDataEntry", {
            up: function () {
                this.createTable("CompositionDataEntry", (t) => {
                    t.text("application");
                    t.json("datamap");
                    t.text("remoteId");
                    t.boolean("isDirty");
                    t.integer("rowstamp");
                });
            },
            down: function () {
                this.dropTable("CompositionDataEntry");
            }
        });

        migrationGroup.addMigration("table Attachment", {
            up: function () {
                this.createTable("Attachment", (t) => {
                    t.text("application");
                    t.text("parentId");
                    t.text("compositionRemoteId");
                    t.text("docinfoRemoteId");
                    t.text("path");
                    t.boolean("compressed");
                    t.text("content");
                    t.text("mimetype");
                });
            },
            down: function () {
                this.dropTable("Attachment");
            }
        });

        migrationGroup.addMigration("table DataEntry", {
            up: function () {
                this.createTable("DataEntry", (t) => {
                    t.text("application");
                    t.json("originaldatamap");
                    t.json("datamap");
                    t.boolean("pending");
                    t.text("remoteId");
                    t.boolean("isDirty");
                    t.boolean("hasProblem");
                    t.integer("rowstamp");
                    t.text("textindex01");
                    t.text("textindex02");
                    t.text("textindex03");
                    t.text("textindex04");
                    t.text("textindex05");
                    t.numeric("numericindex01");
                    t.numeric("numericindex02");
                    t.date("dateindex01");
                    t.date("dateindex02");
                    t.date("dateindex03");
                });
                this.addIndex("DataEntry", ["application","remoteid"], true);
            },
            down: function () {
                this.dropTable("DataEntry");
            }
        });

        migrationGroup.addMigration("table Operation", {
            up: function () {
                this.createTable("Operation", (t) => {
                    t.json("datamap");
                    t.text("operation");
                    t.date("creationDate");
                    t.varchar("entry", 32);
                });
                this.addIndex("Operation", "entry");
            },
            down: function () {
                this.removeIndex("Operation", "entry");
                this.dropTable("Operation");
            }
        });

        migrationGroup.addMigration("table Problem", {
            up: function () {
                this.createTable("Problem", (t) => {
                    t.text("message");
                });
            },
            down: function () {
                this.dropTable("Problem");
            }
        });

        migrationGroup.addMigration("table SyncOperation", {
            up: function () {
                this.createTable("SyncOperation", (t) => {
                    t.date("startdate");
                    t.date("enddate");
                    t.date("lastcheckdate");
                    t.text("lastsyncServerVersion");
                    t.text("status");
                    t.integer("numberofdownloadeditems");
                    t.integer("numberofdownloadedsupportdata");
                    t.boolean("hasProblems");
                    t.boolean("metadatachange");
                    t.integer("items");
                });
            },
            down: function () {
                this.dropTable("SyncOperation");
            }
        });

        migrationGroup.addMigration("table Settings", {
            up: function () {
                this.createTable("Settings", (t) => {
                    t.text("localversion");
                    t.text("serverurl");
                });
            },
            down: function () {
                this.dropTable("Settings");
            }
        });

        migrationGroup.addMigration("table User", {
            up: function () {
                this.createTable("User", (t) => {
                    t.text("name");
                    t.text("orgid");
                    t.text("siteid");
                });
            },
            down: function () {
                this.dropTable("User");
            }
        });

        migrationGroup.addMigration("table Configuration", {
            up: function () {
                this.createTable("Configuration", (t) => {
                    t.text("key");
                    t.json("value");
                });
            },
            down: function () {
                this.dropTable("Configuration");
            }
        });

        migrationGroup.addMigration("table Application", {
            up: function () {
                this.createTable("Application", (t) => {
                    t.text("application");
                    t.boolean("association");
                    t.boolean("composition");
                    t.json("data");
                });
            },
            down: function () {
                this.dropTable("Application");
            }
        });

        migrationGroup.addMigration("table WhereClause", {
            up: function () {
                this.createTable("WhereClause", (t) => {
                    t.text("application");
                    t.text("parentApplication");
                    t.text("metadataid");
                    t.text("data");
                });
            },
            down: function () {
                this.dropTable("WhereClause");
            }
        });

        migrationGroup.addMigration("table Menu", {
            up: function () {
                this.createTable("Menu", (t) => {
                    t.json("data");
                });
            },
            down: function () {
                this.dropTable("Menu");
            }
        });

        migrationGroup.addMigration("table CommandBar", {
            up: function () {
                this.createTable("CommandBar", (t) => {
                    t.text("key");
                    t.json("data");
                });
            },
            down: function () {
                this.dropTable("CommandBar");
            }
        });

        migrationGroup.addMigration("table Cookie", {
            up: function () {
                this.createTable("Cookie", (t) => {
                    t.text("name");
                    t.text("value");
                });
                this.addIndex("Cookie", "name", true);
            },
            down: function () {
                this.removeIndex("Cookie", "name");
                this.dropTable("Cookie");
            }
        });

        migrationGroup.addMigration("table ActiveLaborTracker", {
            up: function () {
                this.createTable("ActiveLaborTracker", (t) => {
                    t.text("parentid");
                    t.text("laborlocalid");
                });
            },
            down: function () {
                this.dropTable("ActiveLaborTracker");
            }
        });

        migrationGroup.addMigration("search indexes  for CompositionData", {
            up: function () {
                this.addColumn("CompositionDataEntry", "textindex01", "text");
                this.addColumn("CompositionDataEntry", "textindex02", "text");
                this.addColumn("CompositionDataEntry", "textindex03", "text");
                this.addColumn("CompositionDataEntry", "textindex04", "text");
                this.addColumn("CompositionDataEntry", "textindex05", "text");
                this.addColumn("CompositionDataEntry", "numericindex01", "numeric");
                this.addColumn("CompositionDataEntry", "numericindex02", "numeric");
                this.addColumn("CompositionDataEntry", "dateindex01", "date");
                this.addColumn("CompositionDataEntry", "dateindex02", "date");
                this.addColumn("CompositionDataEntry", "dateindex03", "date");
            },
            down: function () {
                this.removeColumn("CompositionDataEntry", "textindex01");
                this.removeColumn("CompositionDataEntry", "textindex02");
                this.removeColumn("CompositionDataEntry", "textindex03");
                this.removeColumn("CompositionDataEntry", "textindex04");
                this.removeColumn("CompositionDataEntry", "textindex05");
                this.removeColumn("CompositionDataEntry", "numericindex01");
                this.removeColumn("CompositionDataEntry", "numericindex02");
                this.removeColumn("CompositionDataEntry", "dateindex01");
                this.removeColumn("CompositionDataEntry", "dateindex02");
                this.removeColumn("CompositionDataEntry", "dateindex03");
            }
        });
    }]);

})(angular, persistence);