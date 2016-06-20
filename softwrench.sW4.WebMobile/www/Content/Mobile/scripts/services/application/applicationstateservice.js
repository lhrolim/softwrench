(function (mobileServices, angular) {
    "use strict";

    function applicationStateService(swdbDAO, $q, entities, configurationService, $cordovaAppVersion) {

        //#region Utils

        const countAll = app => swdbDAO.countByQuery("DataEntry", `application='${app}'`);
        
        const countPending = app => swdbDAO.countByQuery("DataEntry", `application='${app}' and pending = 1`);

        const dirtyAndProblematicCount = app => 
            swdbDAO.findByQuery("DataEntry", `application='${app}' and isDirty=1`)
                // all dirty entries
                .then( entries => {
                    // no dirty entries means no problematic entries
                    if (!entries || entries.length <= 0) {
                        return { dirty: 0, problematic: 0 };
                    }

                    const ids = entries.map(entry => `'${entry.id}'`);

                    return swdbDAO.countByQuery("BatchItem", `dataentry in (${ids}) and problem is not null`)
                        // count of problematic batches related to the entries
                        .then(problematic => {
                            const justDirty = entries.length - problematic;
                            return { dirty: justDirty, problematic: problematic };
                        });
                });
        

        //#endregion

        //#region Public methods

        /**
         * Fetches the status of each 'viewable' application (not association nor composition) in the system.
         * Each status is a dictionary: 
         * { 
         *  application: String, // application's name 
         *  all: Integer, // count of all entities
         *  pending: Integer, // count of pending entities 
         *  dirty: Integer, // count of all dirty entities
         *  problematic: Integer, // count of problematic entities 
         * }
         * 
         * @returns Promised resolved with array of each application's states 
         */
        function currentState() {
            return swdbDAO.findByQuery("Application", "composition=0 and association=0")
                .then(results => {
                    const apps = results.map(app => app.application);

                    const promises = apps.map(app => {
                        const countPromises = [];

                        //TODO: verify (then fix) why using swdbdao made the first 2 queries be overriden by "application='<app>' and isDirty=1"
                        //var basequery = "application='{0}'".format(app);
                        //countPromises.push(swdbDAO.countByQuery("DataEntry", basequery)); // all
                        //countPromises.push(swdbDAO.countByQuery("DataEntry", basequery + " and isPending=1")); // pending

                        countPromises.push(countAll(app)); // all
                        countPromises.push(countPending(app)); // pending
                        countPromises.push(dirtyAndProblematicCount(app)); // dirty + problematic -> because problematic=dirty+batchitem.problem!==null

                        return $q.all(countPromises).then(counts => ({
                            application: app,
                            all: counts[0],
                            pending: counts[1],
                            dirty: counts[2].dirty,
                            problematic: counts[2].problematic
                        }));
                    });
                    return $q.all(promises);
                });
        }
        
        /**
         * Fetches the app's configuration (server and client info).
         * 
         * @returns Promise resolved with dictionary containing 'server' and 'client' configuration (both are dictionaries) 
         */
        function getAppConfig() {
            const serverConfigPromise = configurationService.getConfig("serverconfig");
            const clientVersionPromise = isRippleEmulator() ? $q.when("Ripple") : $cordovaAppVersion.getVersionNumber();
            return $q.all([serverConfigPromise, clientVersionPromise]).then(results => {
                const serverConfig = results[0];
                const appVersion = results[1];
                const config = {
                    'server': serverConfig,
                    'client': {
                        'version': appVersion
                    }
                };
                return config;
            });
        }

        //#endregion

        //#region Service Instance
        const service = {
            currentState: currentState,
            getAppConfig: getAppConfig
        };
        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("applicationStateService", ["swdbDAO", "$q", "offlineEntities", "configurationService", "$cordovaAppVersion", applicationStateService]);

    //#endregion

})(mobileServices, angular);
