
(function (serviceModule) {
    "use strict";

    var LoginService = function ($http, $log, $q, routeService, $cookies) {

        this.$cookies = $cookies;

        this.login = function(userName, password) {
            //this was setted during bootstrap of the application, or on settingscontroller.js (settings screen)
            var loginUrl = routeService.loginURL();
            return $http.post(loginUrl, { username: userName, password: password })
                .success(function (userdata) {
                    if (userdata.Found) {
                        return userdata;
                    }
                    return $q.reject(new Error("Invalid username or password"));
                });
        };

        this.checkCookieCredentials = function() {
            return !!$cookies[".ASPXAUTH"];
        };

    };

    serviceModule.service("loginService", ["$http", "$log", "$q", "routeService", "$cookies", LoginService]);

})(mobileServices);