var app = angular.module('sw_layout');

app.factory('compositionService', function () {

    var nonInlineCompositionsDict = function (schema) {
        if (schema.nonInlineCompositionsDict != undefined) {
            //caching
            return schema.nonInlineCompositionsDict;
        }
        var resultDict = {};
        for (var i = 0; i < schema.nonInlineCompositionIdxs.length; i++) {
            var idx = schema.nonInlineCompositionIdxs[i];
            var composition = schema.displayables[idx];
            resultDict[composition.relationship] = composition;
        }
        schema.nonInlineCompositionsDict = resultDict;
        return resultDict;
    };

    var getCompositionSchema = function (baseSchema, compositionKey, schemaId) {
        var schemas = nonInlineCompositionsDict(baseSchema);
        var thisSchema = schemas[compositionKey];
        schemas = thisSchema.schema.schemas;
        return schemaId == "print" ? schemas.print : schemas.list;
    };

    var getCompositionIdName = function (baseSchema, compositionKey, schemaId) {
        return getCompositionSchema(baseSchema, compositionKey, schemaId).idFieldName;
    };


    return {

        locatePrintSchema: function (baseSchema, compositionKey) {
            var schemas = nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];

            if (thisSchema.schema.schemas.print != null) {
                return thisSchema.schema.schemas.print;
            } else if (thisSchema.schema.schemas.list != null) {
                return thisSchema.schema.schemas.list;
            } else {
                return thisSchema.schema.schemas.detail;
            }
        },

        getTitle: function (baseSchema, compositionKey) {
            var schemas = nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];
            return thisSchema.label;
        },

        getListCommandsToKeep: function (compositionSchema) {
            var listSchema = compositionSchema.schemas.list;
            if (listSchema == null) {
                return null;
            }
            var toKeepProperty = listSchema.properties["composition.mainbuttonstoshow"];
            if (!nullOrEmpty(toKeepProperty)) {
                return toKeepProperty.split(';');
            }
            return [];
        }

    };

});


