modules.webcommons.factory('schemaService', function (fieldService, expressionService) {

    /// <summary>
    /// builds a cache of the grid qualified displayables to show on small grids
    /// </summary>
    /// <param name="schema"></param>
    function buildQualifierCache(schema) {
        schema.jscache = schema.jscache || {};
        if (schema.jscache.griddisplayables) {
            //already cached
            return;
        }
        schema.jscache.qualifiercache = {};
        var displayables = schema.displayables;
        for (var i = 0; i < displayables.length; i++) {
            var displayable = displayables[i];
            if (displayable.qualifier) {
                schema.jscache.qualifiercache[displayable.qualifier] = displayable;
            }
        }
    };



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

        nonTabFields: function (schema) {
            return fieldService.nonTabFields(schema.displayables, true);
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

        getTitle: function (schema, datamap, smallDevices) {
            var fields = datamap.fields ? datamap.fields : datamap;

            if (schema.properties['detail.titleexpression'] != null) {
                return expressionService.evaluate(schema.properties['detail.titleexpression'], fields);
            }
            var titleId = schema.idDisplayable;
            if (titleId == null) {
                return schema.title;
            }
            var result;
            if (smallDevices) {
                result = fields[schema.userIdFieldName];
            } else {
                result = titleId + " " + fields[schema.userIdFieldName];
            }
            if (fields.description != null) {
                if (smallDevices) {
                    result += ": " + fields.description;
                } else {
                    result += " Summary: " + fields.description;
                }

            }
            return result;
        },

        locateAttributeByQualifier: function (schema, qualifier) {
            schema.jscache = schema.jscache || {};
            if (schema.jscache.qualifiercache) {
                //already cached
                return schema.jscache.qualifiercache[qualifier].attribute;
            }
            buildQualifierCache(schema);
            return schema.jscache.qualifiercache[qualifier].attribute;
        }




    };

});


