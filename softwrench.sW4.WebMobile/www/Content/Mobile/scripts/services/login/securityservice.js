(function (mobileServices, ionic, _) {
    "use strict";

    function securityService($rootScope, $state, localStorageService, routeService, $http, $q, dao, $ionicHistory, cookieService, $injector, indexCreatorService) {

        //#region Utils

        const config = {
            eventnamespace:"sw4:security:",
            authkey: "security:auth:user",
            previouskey: "security:auth:previous",
            authCookieName: "swcookie",
            message: {
                sessionexpired: "Your session has expired. Please log in to resume your activities. ",
                unauthorizedaccess: "You're not authorized to access this resource. " +
                                    "Contact support if you're receiving this message in error."
            },
            keyblacklist: [ "security:", "settings:" ]
        };

        const $event = name => config.eventnamespace + name;

        const isLoginState = () => $state.current.name === "login";

        const setUserProperties = (user, properties) => {
            if (!properties || _.isEmpty(properties)) {
                delete user["properties"];
            } else {
                user["properties"] = properties;
                if (!!properties["siteid"]) user["SiteId"] = properties["siteid"];
                if (!!properties["orgid"]) user["OrgId"] = properties["orgid"];
            }
            delete user["Properties"];
        };

        const trackLogin = () => {
            const tracking = $injector.get("trackingService");
            tracking.trackStates("securityService#login", ["settings", "configs", "user", "device"], "User successfully logged in");
        };

        /**
         * Authenticates the user locally initializing it's client-side session, persisting the authentication cookie
         * and $broadcasts the event "security:login" in $rootScope with two parameters
         * the current just logged in user and the last logged user.
         * Users have the following format:
         * {
         * "UserName": String,
         * "OrgId": String,
         * "SiteId": String
         * }
         * 
         * @param {Object} user
         * @return {Object} user
         */
        const loginLocal = user => 
            cookieService.persistCookie(config.authCookieName).then(() => {
                var previous = localStorageService.get(config.authkey);
                previous = !!previous ? previous : localStorageService.get(config.previouskey);
                setUserProperties(user, user["Properties"]);
                localStorageService.put(config.authkey, user);
                $rootScope.$broadcast($event("login"), user, previous);
                trackLogin();
                return user;
            });

        const cleanLocalStorage = () => {
            Object.keys(localStorage)
                .filter(k => !config.keyblacklist.some(b => k.startsWith(b)))
                .forEach(k => localStorage.removeItem(k));
        };

        const setHasChanged = (user, changed) => {
            if (!user.meta) user.meta = {};
            user.meta.changed = changed;
        };

        //#endregion

        //#region Public methods

        /**
         * Authenticates the user remotelly then locally.
         * User has the following format:
         * {
         * "UserName": String,
         * "OrgId": String,
         * "SiteId": String
         * }
         * 
         * @param {String} username 
         * @param {String} password 
         * @returns {Promise<UserData>} resolved with the user retuned from the server
         */
        const login = (username, password) => 
            //this was set during bootstrap of the application, or on settingscontroller.js (settings screen)
            routeService.loginURL().then(url => 
                $http({
                    method: "POST",
                    url: url,
                    data: { username: username, password: password, userTimezoneOffset: new Date().getTimezoneOffset() },
                    timeout: 20 * 1000 // 20 seconds
                })
            )
            .then(response => {
                //cleaning history so that back button does not return user to login page
                $ionicHistory.clearCache();

                const userdata = response.data;

                return !!userdata["Found"]
                    ? loginLocal(userdata)
                    : $q.reject(new Error("Invalid username or password"));
            });
        

        /**
         * User has the following format:
         * {
         * "UserName": String,
         * "OrgId": String,
         * "SiteId": String
         * }
         * 
         * @returns logged user 
         */
        const currentFullUser = function () {
            return localStorageService.get(config.authkey);
        };

        /**
         * @returns username of the logged user. 
         * @deprecated use currentFullUser and querry it for wanted property instead
         */
        const currentUser = function () {
            const user = currentFullUser();
            if (!user) {
                return null;
            }
            return user["UserName"];
        };

        /**
         * @returns true if there's a user logged in, false otherwise 
         */
        const hasAuthenticatedUser = function () {
            const user = currentFullUser();
            return !!user;
        };

        /**
         * Finishes the current user session, wipes the database
         * and $broadcasts the event "security:logout" with the just now logged out user.
         * User has the following format:
         * {
         * "UserName": String,
         * "OrgId": String,
         * "SiteId": String
         * }
         * 
         * @return Promise resolved with the logged out user 
         */
        const logout = function (clearSettings =false) {
            // invalidate current session
            const current = localStorageService.remove(config.authkey); 
            // making sure the previous user is always the last "active" user
            if (!!current) {
                localStorageService.put(config.previouskey, current);
            }
            $rootScope.$broadcast($event("logout"), current);
            const toAvoid = clearSettings ? [] : ["Settings"];

            return $q.all([
                dao.resetDataBase(toAvoid),
                cookieService.clearCookies() // clear cookies 
            ]).then(() => {
                $ionicHistory.clearCache(); // clean cache otherwise some views may remain after a consecutive login
                cleanLocalStorage(); // clean non-blacklisted localstorage entries used by apps as cache
                indexCreatorService.dropIndexes(); //drop sql indexes to allow fast sync insertion
                return current;
            });
        };

        /**
         * Updates the current user's properties.
         * (will update the user properties declared in properties parameters instead of completely overriding them).
         * Will mark the current user as changed (<current_user>.metad.chaged === true).
         * Get the updated properties by using {@link #currentFullUser}.properties.
         * 
         * @param {Object} properties
         * @param {Boolean} merge if true will only 
         */
        const updateCurrentUserProperties = function (properties) {
            const current = currentFullUser();
            const merged = current.properties || {};
            angular.forEach(properties || {}, (value, key) => merged[key] = value);
            setUserProperties(current, merged);
            setHasChanged(current, true);
            localStorageService.put(config.authkey, current);
        };

        /**
         * Overrides the current user properties.
         * Will mark the current user as not changed (<current_user>.metad.chaged === false).
         * Get the updated properties by using {@link #currentFullUser}.properties.
         * 
         * @param {Object} properties 
         */
        const overrideCurrentUserProperties = function(properties) {
            const current = currentFullUser();
            setUserProperties(current, properties);
            setHasChanged(current, false);
            localStorageService.put(config.authkey, current);
        }

        /**
         * Handles the case in which the current user is received an "unauthorized"
         * response status (indicating the user requires remote authentication).
         * For now just calls logout.
         */
        const handleUnauthorizedRemoteAccess = ionic.debounce(() => {
            const logoutPromise = logout();
            // not at login state, transition to it with proper message
            if (!isLoginState()) {
                logoutPromise.then(() => routeService.go("login", { message: config.message.unauthorizedaccess }));
            }
        }, 0, true); //debouncing for when multiple parallel requests are unauthorized

        /**
         * Restores locally persisted authentication cookie to the webview.
         * 
         * @returns {Promise<String>} cookie value 
         */
        const restoreAuthCookie = function() {
            return cookieService.restoreCookie(config.authCookieName);
        };

        //#endregion

        //#region Service Instance

        const service = {
            login,
            loginLocal,
            currentUser,
            currentFullUser,
            hasAuthenticatedUser,
            logout,
            handleUnauthorizedRemoteAccess,
            updateCurrentUserProperties,
            overrideCurrentUserProperties,
            restoreAuthCookie
        };
        return service;

        //#endregion
    }
    //#region Service registration

    mobileServices.factory("securityService",
        ["$rootScope", "$state", "localStorageService", "routeService", "$http", "$q", "swdbDAO", "$ionicHistory", "cookieService", "$injector","indexCreatorService", securityService]);

    //#endregion

})(mobileServices, ionic, _);