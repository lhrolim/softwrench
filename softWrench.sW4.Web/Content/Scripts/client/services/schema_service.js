var app = angular.module('sw_layout');

app.factory('schemaService', function () {

 

    return {
        parseAppAndSchema: function (schemaKey) {            
            var keys = schemaKey.split('.');
            if (keys.length == 1) {
                //in this case we are passing only the schemaId  
                return { app: null, schemaId: schemaKey, mode: null };
            }
            var mode = null;
            var application = keys[0];
            var schemaId = keys[1];
            if (keys.length == 3) {
                 mode = keys[2];
            }
            return { app: application, schemaId: schemaId, mode: mode };
        },

        
    };

});


