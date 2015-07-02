﻿var app = angular.module('sw_layout');

app.factory('compositionService', function ($log, $http, $rootScope, $timeout, submitService, schemaService) {

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

        buildMergedDatamap: function (datamap, parentdata) {
            var clonedDataMap = angular.copy(parentdata);
            if (datamap) {
                var item = datamap;
                for (var prop in item) {
                    if (item.hasOwnProperty(prop)) {
                        clonedDataMap[prop] = item[prop];
                    }
                }
            }
            return clonedDataMap;
        },

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
                id: schemaService.getId(datamap, schema)
            };
            var urlToUse = url("/api/generic/ExtendedData/GetCompositionData?" + $.param(parameters));
            var jsonString = angular.toJson(fieldsTosubmit);
            var log = $log.getInstance('compositionservice#populateWithCompositionData');
            log.info('going to server fetching composition data of {0}, schema {1}.'.format(applicationName, schema.schemaId));

            
            return $http.post(urlToUse, jsonString,{avoidspin:true}).success(function(data) {

                var compositionArray = data.resultObject;
                for (var composition in compositionArray) {
                    if (!compositionArray.hasOwnProperty(composition)) {
                        continue;
                    }
                    datamap[composition] = compositionArray[composition].resultList;
                }
                log.info('compositions returned');
                $timeout(function() {
                    $rootScope.$broadcast("sw_compositiondataresolved", datamap);

                }, 100, false);

            });
        }



    };

});


