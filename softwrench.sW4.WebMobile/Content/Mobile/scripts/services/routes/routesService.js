mobileServices.factory('routeService', function ($state,contextService) {

    return {

        loginURL: function (userName, password) {
            return contextService.getFromContext("serverurl") + "/SignIn/SignInReturningUserData?username={0}&password={1}".format(userName, password);
        },

        downloadMetadataURL: function () {
            return contextService.getFromContext("serverurl") + "/api/mobile/DownloadMetadatas";
        },

        syncURL: function () {
            return contextService.getFromContext("serverurl") + "/api/mobile/PullNewData";
        },

        go:function(stateName) {
            contextService.insertIntoContext("currentstate", stateName);
            return $state.go(stateName);
        },

        loadInitialState:function(cookieAuthenticated) {
            if (!cookieAuthenticated) {
                return this.go("login");
            }
            var currentState = contextService.getFromContext("currentstate");
            if (isRippleEmulator() && currentState) {
                return this.go(currentState);
            }
            return this.go("main.home");
        }



    };

});