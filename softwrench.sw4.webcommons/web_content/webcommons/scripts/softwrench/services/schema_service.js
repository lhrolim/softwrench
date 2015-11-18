(function (modules) {
    "use strict";

    modules.webcommons.factory('schemaService', ["fieldService", "expressionService", function (fieldService, expressionService) {

        //#region private methods

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

        //#endregion


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
        function parseAppAndSchema(schemaKey) {
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
        };

        function getId(datamap, schema) {
            if (datamap.fields) {
                return datamap.fields[schema.idFieldName];
            }
            return datamap[schema.idFieldName];
        };

        function nonTabFields(schema) {
            return fieldService.nonTabFields(schema.displayables, true);
        };

        function hasAnyFieldOnMainTab(schema) {
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
        };

        function isPropertyTrue(schema, propertyName) {
            if (!schema) {
                return false;
            }
            return schema.properties && "true" == schema.properties[propertyName];
        };

        function getProperty(schema, propertyName) {
            if (!schema) {
                return false;
            }
            schema.properties = schema.properties || {};
            return schema.properties[propertyName];
        };

        function buildApplicationKey(schema) {
            var basekey = schema.applicationName + "." + schema.schemaId;
            if (schema.mode && schema.mode !== "none") {
                basekey += "." + schema.mode;
            }
            return basekey;
        };

        function buildApplicationMetadataSchemaKey(schema) {
            return {
                applicationName: schema.applicationName,
                schemaId: schema.schemaId,
                mode: schema.mode,
                platform: platform()
            }
        };

        function getTitle(schema, datamap, smallDevices) {
            if (!schema) {
                return null;
            }

            if (!datamap) {
                return "New " + schema.title;
            }
            var fields = datamap.fields ? datamap.fields : datamap;

            if (schema.properties['detail.titleexpression'] != null) {
                return expressionService.evaluate(schema.properties['detail.titleexpression'], fields);
            }
            var titleId = schema.idDisplayable;
            if (titleId == null) {
                return schema.title;
            }
            var result;
            var idValue = fields[schema.userIdFieldName];
            if (!idValue) {

                idValue = "New " + schema.title;
            }
            if (smallDevices) {
                result = idValue;
            } else {
                result = titleId + " " + idValue;
            }
            if (fields.description != null) {
                if (smallDevices) {
                    result += ": " + fields.description;
                } else {
                    result += " Summary: " + fields.description;
                }

            }
            return result;
        };

        function locateDisplayableByQualifier(schema, qualifier) {
            schema.jscache = schema.jscache || {};
            if (schema.jscache.qualifiercache) {
                //already cached
                return schema.jscache.qualifiercache[qualifier];
            }
            buildQualifierCache(schema);
            return schema.jscache.qualifiercache[qualifier];
        };

        function hasEditableProperty(schema) {
            if (schema.mode === "input") {
                return true;
            }
            schema.jscache = schema.jscache || {};
            if (schema.jscache.editable) {
                return schema.jscache.editable;
            }

            var displayables = schema.displayables;
            for (var i = 0; i < displayables.length; i++) {
                var dis = displayables[i];
                if (fieldService.isPropertyTrue(dis, "editable")) {
                    schema.jscache.editable = true;
                    return true;
                }
            }
            schema.jscache.editable = false;
            return false;
        }


        return {
            buildApplicationKey: buildApplicationKey,
            buildApplicationMetadataSchemaKey: buildApplicationMetadataSchemaKey,
            getId: getId,
            getProperty: getProperty,
            getTitle: getTitle,
            hasAnyFieldOnMainTab: hasAnyFieldOnMainTab,
            hasEditableProperty: hasEditableProperty,
            isPropertyTrue: isPropertyTrue,
            locateDisplayableByQualifier: locateDisplayableByQualifier,
            nonTabFields: nonTabFields,
            parseAppAndSchema: parseAppAndSchema
        };

    }]);

})(modules);