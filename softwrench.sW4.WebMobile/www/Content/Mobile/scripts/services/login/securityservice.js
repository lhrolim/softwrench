(function (mobileServices, ionic, _) {
    "use strict";

    function securityService($rootScope,$state, localStorageService, routeService, $http, $q, swdbDAO, $ionicHistory) {

        //#region Utils

        const config = {
            eventnamespace:"sw4:security:",
            authkey: "security:auth:user",
            previouskey: "security:auth:previous",
            message: {
                sessionexpired: "Your session has expired. Please log in to resume your activities. ",
                unauthorizedaccess: "You're not authorized to access this resource. " +
                                    "Contact support if you're receiving this message in error."
            }
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


        /**
         * Authenticates the user locally initializing it's client-side session
         * and $broadcasts the event "security:login" in $rootScope with two parameters
         * the current just logged in user and the last logged user.
         * Users have the following format:
         * {
         * "UserName": String,
         * "OrgId": String,
         * "SiteId": String
         * }
         * 
         * @param {} user 
         */
        const loginLocal = user => {
            var previous = localStorageService.get(config.authkey);
            previous = !!previous ? previous : localStorageService.get(config.previouskey);
            setUserProperties(user, user["Properties"]);
            localStorageService.put(config.authkey, user);
            $rootScope.$broadcast($event("login"), user, previous);
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
         * @param String username 
         * @param String password 
         * @returns Promise resolved with the user retuned from the server
         */
        const login = function (username, password) {
            //this was setted during bootstrap of the application, or on settingscontroller.js (settings screen)
            return routeService.loginURL().then(url => {
                return $http({
                    method: "POST",
                    url: url,
                    data: { username: username, password: password, userTimezoneOffset: new Date().getTimezoneOffset() },
                    timeout: 20 * 1000 // 20 seconds
                });
            })
            .then(response => {
                //cleaning history so that back button does not return user to login page
                $ionicHistory.clearCache();

                const userdata = response.data;
                if (!!userdata["Found"]) {
                    loginLocal(userdata);
                    return userdata;
                }
                return $q.reject(new Error("Invalid username or password"));
            });
        }

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
        const logout = function () {
            // invalidate current session
            const current = localStorageService.remove(config.authkey); 
            // making sure the previous user is always the last "active" user
            if (!!current) {
                localStorageService.put(config.previouskey, current);
            }
            $rootScope.$broadcast($event("logout"), current);

            return swdbDAO.resetDataBase(["Settings"]).then(() => {
                $ionicHistory.clearCache(); // clean cache otherwise some views may remain after a consecutive login
                return current;
            });
        };

        /**
         * Updates the current user's properties.
         * Get the updated properties by using {@link #currentFullUser}.properties.
         * 
         * @param {Object} properties 
         */
        const updateCurrentUserProperties = function (properties) {
            const current = currentFullUser();
            setUserProperties(current, properties);
            localStorageService.put(config.authkey, current);
        };

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
            updateCurrentUserProperties
        };
        return service;

        //#endregion
    }

    //#region Service registration

    mobileServices.factory("securityService", ["$rootScope","$state", "localStorageService", "routeService", "$http", "$q", "swdbDAO", "$ionicHistory", securityService]);

    //#endregion

})(mobileServices, ionic, _);