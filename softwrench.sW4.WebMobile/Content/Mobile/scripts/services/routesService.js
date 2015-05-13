mobileServices.factory('routeService', function (contextService) {

    return {

        loginURL: function (userName, password) {
            return contextService.getFromContext("serverurl") + "/SignIn/SignInReturningUserData?username={0}&password={1}".format(userName, password);
        },

        downloadMetadataURL: function () {
            return contextService.getFromContext("serverurl") + "/api/mobile/DownloadMetadatas";
        },

        syncURL: function () {
            return contextService.getFromContext("serverurl") + "/api/mobile/PullNewData";
        }

    };

});