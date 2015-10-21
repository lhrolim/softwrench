(function (mobileServices) {
    "use strict";

    mobileServices.factory('routeService', ["$state", "contextService", "settingsService", "localStorageService", function ($state, contextService, settingsService, localStorageService) {

    return {

        loginURL: function () {
            return settingsService.getServerUrl().then(function(url) {
                return url + "/SignIn/SignInReturningUserData";
            });
        },

        go: function (stateName, params) {
            // TODO: insert params in the context and recover
            contextService.insertIntoContext("currentstate", stateName);
            return $state.go(stateName, params);
        },

        loadInitialState: function(authenticated) {
            if (!authenticated) {
                return !localStorageService.get("settings:serverurl") ? this.go("settings") : this.go("login");
            }
            var currentState = contextService.getFromContext("currentstate");
            if (isRippleEmulator() && currentState) {
                return this.go(currentState);
            }
            return this.go("main.home");
        },

        $state: $state

    };

}]);

})(mobileServices)
