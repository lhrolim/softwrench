(function (angular, persistence) {
    "use strict";
    function offlineMigrationService($q, $log, offlineMigrations) {

        const decorateMigrator = function () {
            const migrator = persistence.migrations.Migrator;
            const Migration = persistence.migrations.Migration;
            const migrations = migrator.migrations.slice(1);

            const log = $log.getInstance("offlineMigrationService");

            // migration method that uses a table to control each migration
            migrator.migrateSw = function() {
                const dbMigrationsIds = [];
                return persistence.runSql("select id from migrations").then((dbMigrations) => {
                    if (!dbMigrations) {
                        return $q.when();
                    }

                    angular.forEach(dbMigrations, (dbMigration) => {
                        dbMigrationsIds.push(dbMigration.id);
                    });

                    const migrationsToRun = [];
                    angular.forEach(migrations, (migration) => {
                        if (!migration.body.id) {
                            log.warn("Migration ignored due to absence of id.");
                        }

                        if (migration.body.id && dbMigrationsIds.indexOf(migration.body.id) < 0) {
                            migrationsToRun.push(migration);
                        }
                    });

                    const deferred = $q.defer();
                    const callback = function () {
                        deferred.resolve();
                    }

                    const reversedMigrationsToRun = migrationsToRun.reverse(); // migrations reversed to use pop
                    const migrateOne = function () {
                        const migration = reversedMigrationsToRun.pop();
                        if (!migration) {
                            callback();
                            return;
                        }

                        const id = migration.body.id;
                        migration.up(function () {
                            persistence.runSql("insert into migrations (id,applied) values (?,?)", [id, new Date().getTime()]).then(() => {
                                if (reversedMigrationsToRun.length > 0) {
                                    migrateOne();
                                } else {
                                    callback();
                                }
                            });
                        });
                    }
                    migrateOne();

                    return deferred.promise;
                });
            };

            // new method to declare varchar and numeric types
            const originalPrototype = Migration.prototype.createTable;
            Migration.prototype.createTable = function(tableName, callback) {
                const wrapper = (table) => {
                    table.varchar = function (columnName, size) {
                        table.columns.unshift(`${columnName} VARCHAR(${size})`);
                    }
                    table.numeric = function (columnName) {
                        table.columns.unshift(`${columnName} NUMERIC`);
                    }
                    callback(table);
                }
                originalPrototype.call(this, tableName, wrapper);
            }

            // adding an extra "_" on index name to be the same pattern as indexes created from persistence.js hasOne and hasMany
            Migration.prototype.addIndex = function (tableName, columnName, unique, indexName) {
                var originalColumnName = columnName;
                if (columnName instanceof Array) {
                    originalColumnName = columnName.join("_");
                    columnName = columnName.join(',');
                }

                const name = indexName || tableName + "__" + originalColumnName;
                const uniqueString = unique === true ? "UNIQUE" : "";
                const sql = `CREATE ${uniqueString} INDEX ${name} ON ${tableName} (${columnName})`;
                this.executeSql(sql);
            }
        }

        const setupMigrator = function() {
            return persistence.runSql("CREATE TABLE IF NOT EXISTS migrations (id VARCHAR(255) PRIMARY KEY, applied DATE)").then(() => {
                decorateMigrator();
            });
        }


        const migrate = function () {
            angular.forEach(offlineMigrations, (offlineMigration, version) => {
                persistence.defineMigration(version + 1, offlineMigration);
            });

            const deferred = $q.defer();
            persistence.migrations.init(() => {
                setupMigrator().then(() => {
                    return persistence.migrations.Migrator.migrateSw();
                }).then(() => {
                    deferred.resolve();
                });
            });

            return deferred.promise;
        }

        const service = {
            migrate
        };
        return service;
    }


    //#region Service registration
    angular.module("persistence.offline").factory("offlineMigrationService", ["$q", "$log", "offlineMigrations", offlineMigrationService]);
    //#endregion

})(angular, persistence);