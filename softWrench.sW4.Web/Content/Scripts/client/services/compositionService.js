var app = angular.module('sw_layout');

app.factory('compositionService', function ($log, $http, submitService) {

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
        },

        /*
        * this method will hit the server to fetch associated composition data on a second request making the detail screens faster
        *
        */
        populateWithCompositionData: function (schema, datamap) {
            var fieldsTosubmit = submitService.removeExtraFields(datamap, true, schema);
            var applicationName = schema.applicationName;
            var parameters = {
                application: applicationName,
                key: {
                    schemaId: schema.schemaId,
                    mode: schema.mode,
                    platform: platform(),
                },
            };
            var urlToUse = url("/api/generic/Data/GetCompositionData?" + $.param(parameters));
            var jsonString = angular.toJson(fieldsTosubmit);
            var log = $log.getInstance('compositionservice#populateWithCompositionData');
            log.info('going to server fetching composition data of {0}, schema {1}.'.format(applicationName, schema.schemaId));

            $http.post(urlToUse, jsonString).success(function (data) {
                var newDatamap = data.resultObject;
                for (var field in newDatamap.fields) {
                    if (!datamap.hasOwnProperty(field)) {
                        datamap[field] = newDatamap.fields[field];
                    }
                }
                log.info('compositions returned');
            });
        }



    };

});


