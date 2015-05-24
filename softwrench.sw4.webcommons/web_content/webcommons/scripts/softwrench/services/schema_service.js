modules.webcommons.factory('schemaService', function (fieldService) {



    return {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaKey">a string representing a full schema, with or without reference to the application</param>
        /// <returns type="">an object with the following properties
        /// app: the application name (could be null)
        /// schemaId: the schema Id (can´t be null)
        /// mode: the mode of the schema(or null)
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

        getId: function (datamap, schema) {
            if (datamap.fields) {
                return datamap.fields[schema.idFieldName];
            }
            return datamap[schema.idFieldName];
        },

        hasAnyFieldOnMainTab: function (schema) {
            if (!schema) {
                return false;
            }

            schema.jscache = schema.jscache || {};
            if (schema.jscache.hasAnyFieldOnMainTab) {
                return schema.jscache.hasAnyFieldOnMainTab;
            }
            var fields = fieldService.nonTabFields(schema.displayables, false);
            var result = fields.length > 0;
            schema.jscache.hasAnyFieldOnMainTab = result;
            return result;
        },

        isPropertyTrue: function (schema, propertyName) {
            if (!schema) {
                return false;
            }
            return schema.properties && "true" == schema.properties[propertyName];
        },

        getProperty: function (schema, propertyName) {
            if (!schema) {
                return false;
            }
            schema.properties = schema.properties || {};
            return schema.properties[propertyName];
        },

        buildApplicationKey: function (schema) {
            var basekey = schema.applicationName + "." + schema.schemaId;
            if (schema.mode && schema.mode != "none") {
                basekey += "." + schema.mode;
            }
            return basekey;
        },

     


    };

});


