
(function (serviceModule) {
    "use strict";

    var LoginService = function ($http, $log, $q, routeService, securityService) {

        this.login = function (username, password) {

            //this was setted during bootstrap of the application, or on settingscontroller.js (settings screen)
            var loginUrl = routeService.loginURL();
            return $http.post(loginUrl, { username: username, password: password })
                .then(function (response) {
                    var userdata = response.data;
                    if (userdata.Found) {
                        securityService.loginLocal(userdata.UserName);
                        return userdata;
                    }
                    return $q.reject(new Error("Invalid username or password"));
                });


        };
    };

    serviceModule.service("loginService", ["$http", "$log", "$q", "routeService", "securityService", LoginService]);

})(mobileServices);