(function (mobileServices) {
    "use strict";

mobileServices.factory('routeService', ["$state", "contextService", function ($state, contextService) {

    return {

        loginURL: function () {
            return contextService.getFromContext("serverurl") + "/SignIn/SignInReturningUserData";
        },

        downloadMetadataURL: function () {
            return contextService.getFromContext("serverurl") + "/api/mobile/DownloadMetadatas";
        },

        syncURL: function () {
            return contextService.getFromContext("serverurl") + "/api/mobile/PullNewData";
        },

        go: function (stateName, params) {
            // TODO: insert params in the context and recover
            contextService.insertIntoContext("currentstate", stateName);
            return $state.go(stateName, params);
        },

        loadInitialState: function(authenticated) {
            if (!authenticated) {
                return !contextService.get("serverurl") ? this.go("settings") : this.go("login");
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
