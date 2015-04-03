mobileServices.factory('html5DBDAO', function () {

    var swdb;

    return {

        init: function () {
            swdb = openDatabase('offlineswdb', '1.0', 'my first database', 2 * 1024 * 1024);
        }

    };

});