(function (mobileServices) {
    "use strict";

    function securityService($rootScope, localStorageService, routeService) {

        //#region Utils

        var config = {
            eventnamespace:"security:",
            authkey: "security:auth:user",
            previouskey: "security:auth:previous",
            sessionexpiredmessage: "Your session has expired. Please log in to resume your activities."
        };

        var isLoginState = function() {
            var current = routeService.$state.current.name;
            return current === "login";
        };

        //#endregion

        //#region Public methods

        /**
         * Authenticates the user locally initializing it's client-side session
         * and $broadcasts the event "security:login" in $rootScope with two parameters
         * the username of the current just logged in user and the username of the last logged user.
         * 
         * @param String username 
         */
        var loginLocal = function(username) {
            var previous = localStorageService.get(config.authkey);
            previous = !!previous ? previous : localStorageService.get(config.previouskey);
            localStorageService.put(config.authkey, username);
            $rootScope.$broadcast(config.eventnamespace + "login", username, previous);
        };

        /**
         * @returns true if there's a user logged in, false otherwise 
         */
        var hasAuthenticatedUser = function() {
            var user = localStorageService.get(config.authkey);
            return !!user;
        };

        /**
         * Finishes the current user session, redirects to login state (if not already at login state)
         * and $broadcasts the event "security:logout" with the just now logged out user's username.
         */
        var logout = function () {
            // invalidate current session
            var current = localStorageService.remove(config.authkey);
            // making sure the previous user is always the last "active" user
            if (!!current) {
                localStorageService.put(config.previouskey, current);
            }
            $rootScope.$broadcast(config.eventnamespace + "logout", current);
            // if not already at login state transition to it with a message
            if (!isLoginState()) {
                routeService.go("login", { message: config.sessionexpiredmessage });
            }
        };

        /**
         * For now just calls logout.
         */
        var handleForbiddenStatus = function() {
            logout();
        };

        //#endregion

        //#region Service Instance

        var service = {
            loginLocal: loginLocal,
            hasAuthenticatedUser: hasAuthenticatedUser,
            logout: logout,
            handleForbiddenStatus: handleForbiddenStatus
        };

        return service;

        //#endregion
    }

    //#region Service registration

    mobileServices.factory("securityService", ["$rootScope", "localStorageService", "routeService", securityService]);

    //#endregion

})(mobileServices);