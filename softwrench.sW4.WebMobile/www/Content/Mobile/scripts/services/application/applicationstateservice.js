(function (mobileServices, angular, _) {
    "use strict";

    function applicationStateService(dao, $q, entities, configurationService, $cordovaAppVersion, settingsService, securityService, $cordovaDevice) {

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

        const stateResolver = {
            settings() {
                return settingsService.getSettings().then(s => _.pick(s, "serverurl"));
            },
            configs() {
                return getAppConfig();
            },
            applications() {
                return currentApplicationsState();
            },
            user() {
                const user = securityService.currentFullUser();
                return $q.when(user);
            },
            device() {
                const device = _.pick($cordovaDevice.getDevice(), "platform", "version", "model");
                return $q.when(device);
            }
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
        function currentApplicationsState() {
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

        /**
         * Resolves the states of the app (e.g. logged user, settings, configs, application state, device info) 
         * passed as parameter.
         * 
         * @param {Array<String>} states possible values are "settings"|"configs"|"user"|"device"|"applications"
         * @returns {Promise<Object>} state 
         */
        function getStates(states) {
            if (!angular.isArray(states) || states.length <= 0) return $q.when();
            const promises = [];
            angular.forEach(states, state => {
                const resolver = stateResolver[state];
                if (!resolver || !angular.isFunction(resolver)) return;
                // state value indexed by it's name
                const promise = resolver().then(result => ({ state, result }));
                promises.push(promise);
            });
            if (promises.length <= 0) return $q.when();
            return $q.all(promises).then(result => {
                const fullState = {};
                // build object { <state_name>: <state_value> }
                angular.forEach(result, (value) => {
                    fullState[value.state] = value.result;
                });
                return fullState;
            });
        }

        function getServerDeviceData() {
            return this.getStates(["configs", "device"]).then(state => {
                const deviceData = state.device;
                deviceData["clientVersion"] = state.configs.client.version;
                return deviceData;
            });
        }

        /**
         * Resolves all possible states of the app
         * @see #getStates
         * @returns {Promise<Object>} complete state
         */
        function getFullState() {
            return getStates(Object.keys(stateResolver));
        }

        //#endregion

        //#region Service Instance
        const service = {
            currentApplicationsState,
            getAppConfig,
            getStates,
            getServerDeviceData,
            getFullState
        };
        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("applicationStateService",
        ["swdbDAO", "$q", "offlineEntities", "configurationService", "$cordovaAppVersion", "settingsService", "securityService", "$cordovaDevice", applicationStateService]);

    //#endregion

})(mobileServices, angular, _);
