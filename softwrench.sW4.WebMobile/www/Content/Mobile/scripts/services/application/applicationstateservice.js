(function (mobileServices, angular, _) {
    "use strict";

    function applicationStateService(dao, $q, entities, configurationService, $cordovaAppVersion) {

        //#region Utils

        const countAll = app => dao.countByQuery("DataEntry", `application='${app}'`);
        
        const countPending = app => dao.countByQuery("DataEntry", `application='${app}' and pending = 1`);

        const countDirty = app => dao.countByQuery("DataEntry", `application='${app}' and isDirty=1 and (hasProblem = 0 or hasProblem is null)`);

        const countProblematic = app => dao.countByQuery("DataEntry", `application='${app}' and hasProblem = 1`);

        function associationState() {
            return dao.findByQuery("Application", "association=1")
                .then(r => r.map(a => ({
                    name: a.application, title: a.data.title
                })))
                .then(a => {
                    const titleLookupTable = _.indexBy(a, "name");
                    return dao.executeStatement("select application,count(id) from AssociationData group by application")
                        .then(c => c.map(i => ({ application: i.application, count: i["count(id)"], title: titleLookupTable[i.application].title })));
                });
        }

        function topLevelApplicationState() {
            return dao.findByQuery("Application", "composition=0 and association=0")
                .then(results => {
                    const apps = results.map(app => app.application);

                    const promises = apps.map(app => {
                        const countPromises = [
                            countAll(app), // all
                            countPending(app), // pending
                            countDirty(app), // dirty
                            countProblematic(app) // problematic
                        ];

                        return $q.all(countPromises)
                            .spread((all, pending, dirty, problematic) => ({
                                application: app,
                                all,
                                pending,
                                dirty,
                                problematic
                            }));
                    });
                    return $q.all(promises);
                });
        }

        //#endregion

        //#region Public methods

        /**
         * Fetches the status of each 'top level' application and each association in the system and groups them:
         * { 
         * applications: [{ 
         *      application: String, // application's name 
         *      all: Integer, // count of all entities
         *      pending: Integer, // count of pending entities 
         *      dirty: Integer, // count of all dirty entities (just dirty, not including problematic)
         *      problematic: Integer, // count of problematic entities 
         *  }],
         *  associations: [{
         *      application: String, // application's name
         *      title: String, // application's title
         *      count: Integer, // count of all associations
         *  }]
         * }
         * 
         * @returns Promised resolved with array of each application's states 
         */
        function currentState() {
            return $q.all([topLevelApplicationState(), associationState()])
                .spread((applications, associations) => ({ applications, associations }));
        }
        
        /**
         * Fetches the app's configuration (server and client info).
         * 
         * @returns Promise resolved with dictionary containing 'server' and 'client' configuration (both are dictionaries) 
         */
        function getAppConfig() {
            const serverConfigPromise = configurationService.getConfig("serverconfig");
            const clientVersionPromise = isRippleEmulator() ? $q.when("Ripple") : $cordovaAppVersion.getVersionNumber();
            return $q.all([serverConfigPromise, clientVersionPromise])
                .spread((serverConfig, appVersion) => ({
                    'server': serverConfig,
                    'client': { 'version': appVersion }
                })
            );
        }

        //#endregion

        //#region Service Instance
        const service = {
            currentState,
            getAppConfig
        };
        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("applicationStateService", ["swdbDAO", "$q", "offlineEntities", "configurationService", "$cordovaAppVersion", applicationStateService]);

    //#endregion

})(mobileServices, angular, _);
