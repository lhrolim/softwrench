var app = angular.module('sw_layout');

app.factory('schemaService', function () {
    "ngInject";
 

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

        getProperty: function (schema, propertyName) {
            if (!schema) {
                return false;
            }
            schema.properties = schema.properties || {};
            return schema.properties[propertyName];
        },


        isPropertyTrue: function (schema, propertyName) {
            if (!schema) {
                return false;
            }
            return schema.properties && "true" == schema.properties[propertyName];
        },

        getId: function (datamap, schema) {
            if (datamap.fields) {
                return datamap.fields[schema.idFieldName];
            }
            return datamap[schema.idFieldName];
        },

        nonTabFields: function (schema) {
            return fieldService.nonTabFields(schema.displayables, true);
        },

        
    };

});


