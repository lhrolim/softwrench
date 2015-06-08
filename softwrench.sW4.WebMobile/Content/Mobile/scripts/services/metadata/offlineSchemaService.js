mobileServices.factory('offlineSchemaService', function ($log) {

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

        locateSchema: function (application, schemaId) {
            var schemasList = application.data.schemasList;

            for (var i = 0; i < schemasList.length; i++) {
                var schema = schemasList[i];
                if ("list".equalsIc(schema.stereotype)) {
                    //build dict cache
                    buildQualifierCache(schema);
                }
                if (schema.schemaId.equalsIc(schemaId)) {
                    return schema;
                }
            }
            return null;
        },

        locateSchemaByStereotype: function (application, stereotype) {
            var schemasList = application.data.schemasList;

            for (var i = 0; i < schemasList.length; i++) {
                var schema = schemasList[i];
                if (schema.stereotype == "list") {
                    //build dict cache
                    buildQualifierCache(schema);
                }
                if (stereotype.equalsIc(schema.stereotype)) {
                    return schema;
                }
                else if ("detail".equalsIc(stereotype) && schema.schemaId.equalsIc("detail")) {
                    return schema;
                }
            }
            return null;
        },


        fillDefaultValues: function (schema, item) {

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

    }
});