(function (angular, _) {
    "use strict";

    function trackingService($log, $q, applicationStateService) {
        //#region Utils
        const config = {
            loggerContext: "tracking",
            get loggerContextKey() { return `log_${this.loggerContext}`; },
            loggerLevel: "trace"
        };
        const defaultTracker = $log.get(config.loggerContext, [config.loggerContext]);

        function getTracker(context) {
            return context
                ? $log.get(`${context}/${config.loggerContext}`, [config.loggerContext])
                : defaultTracker;
        }

        //#endregion

        //#region Public methods

        /**
         * Enables tracking.
         */
        function enable() {
            sessionStorage[config.loggerContextKey] = config.loggerLevel;
        }

        function disable() {
            sessionStorage[config.loggerContextKey] = defaultTracker.globalLogLevel;
        }

        function isEnabled() {
            return sessionStorage[config.loggerContextKey] === config.loggerLevel;
        }

        function track(context, ...messages) {
            const tracker = getTracker(context);
            tracker.trace("[TRACKING]:", ...messages);
        }
        
        /**
         * Logs states of the application passed as parameter
         * (e.g. logged user, settings, configs, application state, device info)
         * along with the context tag and any additional messages.
         * 
         * @param {String} context a tag for tracking
         * @param {Array<String>} states possible values are "settings"|"configs"|"user"|"device"|"applications"
         *                                  if null or empty will track the complete state
         * @param {VarArgs<String>} messages 
         */
        function trackStates(context, states, ...messages) {
            const method = !angular.isArray(states) || states.length <= 0 ? "getFullState" : "getStates";
            applicationStateService[method](states).then(state => {
                if (!state || _.isEmpty(state)) return;
                track(context, state, ...messages);
            });
        }

        /**
         * Logs the complete current state of the application 
         * (i.e. logged user, settings, configs, application state, device info)
         * along with the context tag and any additional messages.
         * 
         * @param {String} context a tag for tracking
         * @param {VarArgs<String>} messages
         */
        function trackFullState(context, ...messages) {
            if (!isEnabled()) return;
            trackStates(context, null, ...messages);
        }

        //#endregion

        //#region Service Instance
        const service = {
            track,
            trackStates,
            trackFullState,
            enable,
            disable,
            get isEnabled() {
                return isEnabled();
            }
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services").factory("trackingService",
        ["$log", "$q", "applicationStateService", "$roll", "$cordovaFile", "$cordovaEmailComposer", "networkConnectionService", trackingService]);

    //#endregion

})(angular, _);