﻿(function (mobileServices) {
    "use strict";

    function securityService($rootScope, localStorageService, routeService, $http, $q, swdbDAO, $ionicHistory) {

        //#region Utils

        var config = {
            eventnamespace:"sw4:security:",
            authkey: "security:auth:user",
            previouskey: "security:auth:previous",
            message: {
                sessionexpired: "Your session has expired. Please log in to resume your activities.",
                unauthorizedaccess: "You're not authorized to access this resource.<br>" +
                    "Contact support if you think you're not supposed to receive this message."
            }
        };

        var isLoginState = function () {
            var current = routeService.$state.current.name;
            return current === "login";
        };

        /**
         * Authenticates the user locally initializing it's client-side session
         * and $broadcasts the event "security:login" in $rootScope with two parameters
         * the username of the current just logged in user and the username of the last logged user.
         * 
         * @param String username 
         */
        var loginLocal = function (username) {
            var previous = localStorageService.get(config.authkey);
            previous = !!previous ? previous : localStorageService.get(config.previouskey);
            localStorageService.put(config.authkey, username);
            $rootScope.$broadcast(config.eventnamespace + "login", username, previous);
        };

        //#endregion

        //#region Public methods

        /**
         * Authenticates the user remotelly then locally.
         * 
         * @param String username 
         * @param String password 
         * @returns Promise resolved with username 
         */
        var login = function(username, password) {
            //this was setted during bootstrap of the application, or on settingscontroller.js (settings screen)
            var loginUrl = routeService.loginURL();
            return $http.post(loginUrl, { username: username, password: password })
                .then(function (response) {
                    var userdata = response.data;
                    if (userdata.Found) {
                        loginLocal(userdata.UserName);
                        return userdata;
                    }
                    return $q.reject(new Error("Invalid username or password"));
                });
        }

        /**
         * @returns username of the logged user. 
         */
        var currentUser = function() {
            return localStorageService.get(config.authkey);
        };

        /**
         * @returns true if there's a user logged in, false otherwise 
         */
        var hasAuthenticatedUser = function() {
            var user = currentUser();
            return !!user;
        };

        /**
         * Finishes the current user session, wipes the database
         * and $broadcasts the event "security:logout" with the just now logged out user's username.
         * 
         * @return Promise resolved with username of the logged out user 
         */
        var logout = function () {
            // invalidate current session
            var current = localStorageService.remove(config.authkey);
            // making sure the previous user is always the last "active" user
            if (!!current) {
                localStorageService.put(config.previouskey, current);
            }
            $rootScope.$broadcast(config.eventnamespace + "logout", current);

            return swdbDAO.resetDataBase(["Settings"]).then(function () {
                $ionicHistory.clearCache(); // clean cache otherwise some views may remain after a consecutive login
                return current;
            });
        };

        /**
         * Handles the case in which the current user is received an "unauthorized"
         * response status (indicating the user requires remote authentication).
         * For now just calls logout.
         */
        var handleUnauthorizedRemoteAccess = function () {
            var logoutPromise = logout();
            // not at login state, transition to it with proper message
            if (!isLoginState()) {
                logoutPromise.then(function () {
                    routeService.go("login", { message: config.message.unauthorizedaccess });
                });
            }
        };

        //#endregion

        //#region Service Instance

        var service = {
            login: login,
            currentUser: currentUser,
            hasAuthenticatedUser: hasAuthenticatedUser,
            logout: logout,
            handleUnauthorizedRemoteAccess: handleUnauthorizedRemoteAccess
        };

        return service;

        //#endregion
    }

    //#region Service registration

    mobileServices.factory("securityService", ["$rootScope", "localStorageService", "routeService", "$http", "$q", "swdbDAO", "$ionicHistory", securityService]);

    //#endregion

})(mobileServices);