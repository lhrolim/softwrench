mobileServices.factory('swdbProxy', function (sqlLiteDBDAO, html5DBDAO) {

    var realInstance = isRippleEmulator() ? html5DBDAO : sqlLiteDBDAO;

    return {

        init : function() {
            realInstance.init();
        }

    };

});