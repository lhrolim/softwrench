mobileServices.factory('sqlLiteDBDAO', function ($cordovaSQLite,$log) {

    var swdb;

    return {

        init : function() {
            swdb = $cordovaSQLite.openDB("offlineswdb");
            $cordovaSQLite.execute(swdb, "CREATE TABLE IF NOT EXISTS metadata (name primary key, definition text)");
        }

    };

});