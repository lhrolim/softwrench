mobileServices.factory('loginService', function ($q) {

    return {

        checkCookieCredentials: function () {
            return true;
        },

        login: function (userName,password) {
            var deferred = $q.defer();
            var promise = deferred.promise;

            //use rest service later
            if (userName == 'swadmin' && password == 'sw@dm1n') {
                deferred.resolve('Welcome ' + name + '!');
            } else {
                deferred.reject('Wrong credentials.');
            }
            promise.success = function (fn) {
                promise.then(fn);
                return promise;
            }
            promise.error = function (fn) {
                promise.then(null, fn);
                return promise;
            }
            return promise;
        }

    };

});