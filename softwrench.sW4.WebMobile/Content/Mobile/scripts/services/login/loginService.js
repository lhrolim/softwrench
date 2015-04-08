mobileServices.factory('loginService', function ($http, $q, routeService, dispatcherService) {

    return {

        checkCookieCredentials: function () {
            return true;
        },

        login: function (userName, password) {
            var deferred = dispatcherService.loadBaseDeferred();
            var promise = deferred.promise;

            //this was setted during bootstrap of the application, or on settingscontroller.js (settings screen)
            var loginUrl = routeService.loginURL(userName, password);
            $http.post(loginUrl).success(function(userdata) {
                if (userdata.Found) {
                    deferred.resolve();
                } else {
                    deferred.reject();
                }
            }).error(function(errorData) {
                deferred.reject();
            });
            return promise;
        }

    };

});