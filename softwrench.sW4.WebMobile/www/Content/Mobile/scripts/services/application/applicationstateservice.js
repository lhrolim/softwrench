(function (mobileServices, angular) {
    "use strict";

    function applicationStateService(swdbDAO, $q, entities, configurationService, $cordovaAppVersion) {

        //#region Utils

        const countAll = app => swdbDAO.countByQuery("DataEntry", `application='${app}'`);
        
        const countPending = app => swdbDAO.countByQuery("DataEntry", `application='${app}' and pending = 1`);

        const countDirty = app => swdbDAO.countByQuery("DataEntry", `application='${app}' and isDirty=1`);

        const countProblematic = app =>
            swdbDAO.executeStatement(`select count(d.id) from DataEntry d where d.application=? and d.isDirty=1 and
                                    (select count(b.id) from BatchItem b where b.dataentry=d.id and problem is not null) > 0`,
                                    [app])
                .then(r => !r ? 0 : r[0]["count(d.id)"]);
        

        //#endregion

        //#region Public methods

        /**
         * Fetches the status of each 'viewable' application (not association nor composition) in the system.
         * Each status is a dictionary: 
         * { 
         *  application: String, // application's name 
         *  all: Integer, // count of all entities
         *  pending: Integer, // count of pending entities 
         *  dirty: Integer, // count of all dirty entities (just dirty, not including problematic)
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
                                dirty: dirty - problematic, // just dirty
                                problematic
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
