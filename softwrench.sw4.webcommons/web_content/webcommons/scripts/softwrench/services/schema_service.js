var app = angular.module('sw_layout');

app.factory('schemaService', function () {



    return {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaKey"></param>
        /// <returns type="">an object with the following
        /// app: the application name (not null)
        /// schemaId: the schema Id (or null)
        /// mode: the mode of itar (or null)
        /// 
        /// </returns>
        parseAppAndSchema: function (schemaKey) {
            if (schemaKey == null) {
                return null;
            }

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

        buildApplicationKey: function (schema) {
            var basekey = schema.applicationName + "." + schema.schemaId;
            if (schema.mode && schema.mode != "none") {
                basekey += "." +schema.mode;
            }
            return basekey;
        },


    };

});


