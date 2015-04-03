mobileServices.factory('loginService', function () {

    return {

        checkCookieCredentials: function () {
            return true;
        },

        login: function (userName,password) {
            return false;
        }

    };

});