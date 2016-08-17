(function (angular) {
    /**
     * Same of offlineEntities.js but for migrations.
     */
    angular.module("persistence.offline").provider("offlineMigrations", function () {
        // service instance
        const migrationGroups = {};

        function pad(num, size) {
            var s = num+"";
            while (s.length < size) s = `0${s}`;
            return s;
        }

        class MigrationGroup {
            // id has max size = 99999999
            // the id defines the order of migration groups run
            constructor(id, tag) {
                this.id = id;
                this.tag = tag;
                this.stringId = pad(id, 8);
                this.migrations = [];
            }

            // tag has max length = 26 characters
            addMigration(tag, migration) {
                migration.id = this.stringId + "-" + (this.tag || "") + "-" + (tag || "");
                this.migrations.push(migration);
            }
        }

        const provider = {
            // access to the service instance in config time
            migrationGroups: migrationGroups,
            createMigrationGroup: function (id, tag) {
                const group = new MigrationGroup(id, tag);
                migrationGroups[id] = group;
                return group;
            },
            // service constructor
            $get: [function () {
                var migrations = [];
                var groupIds = [];
                angular.forEach(migrationGroups, (migrationGroup, id) => {
                    if (!id || !migrationGroups.hasOwnProperty(id)) {
                        return;
                    }
                    groupIds.push(id);
                });
                groupIds = groupIds.sort();
                angular.forEach(groupIds, (id) => {
                    migrations = migrations.concat(migrationGroups[id].migrations);
                });

                return migrations;
            }]
        };
        return provider;
    });

})(angular);