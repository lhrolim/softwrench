mobileServices.factory('html5DBDAO', function () {

    var swdb;

    return {

        init: function () {
            swdb = openDatabase('offlineswdb', '1.0', 'my first database', 2 * 1024 * 1024);
        }

    };

});
mobileServices.factory('sqlLiteDBDAO', function ($cordovaSQLite,$log) {

    var swdb;

    return {

        init : function() {
            swdb = $cordovaSQLite.openDB("offlineswdb");
            $cordovaSQLite.execute(swdb, "CREATE TABLE IF NOT EXISTS metadata (name primary key, definition text)");
        }

    };

});
mobileServices.factory('swdbProxy', function (sqlLiteDBDAO, html5DBDAO) {

    var realInstance = isRippleEmulator() ? html5DBDAO : sqlLiteDBDAO;

    return {

        init : function() {
            realInstance.init();
        }

    };

});
mobileServices.factory('loginService', function () {

    return {

        checkCookieCredentials: function () {
            return false;
        },

        login: function (userName,password) {
            return false;
        }

    };

});
function isRippleEmulator() {
    return document.URL.indexOf('http://') != -1 || document.URL.indexOf('https://') != -1;
}