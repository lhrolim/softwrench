(function (angular) {
    "use strict";

    var app = angular.module('sw_layout');

    app.factory('compositionService',
        ["$log", "$http", "$rootScope", "$timeout", "submitService", "schemaService", "searchService", "$q", "fieldService",
        function ($log, $http, $rootScope, $timeout, submitService, schemaService, searchService, $q, fieldService) {

            var __deafultPageSize__ = 10;

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
                return schemaId === "print" ? schemas.print : schemas.list;
            };

            var getCompositionIdName = function (baseSchema, compositionKey, schemaId) {
                return getCompositionSchema(baseSchema, compositionKey, schemaId).idFieldName;
            };

            var buildPaginatedSearchDTO = function (pageNumber) {
                var dto = searchService.buildSearchDTO();
                dto.pageNumber = pageNumber || 1;
                dto.pageSize = __deafultPageSize__;
                dto.totalCount = 0;
                // dto.paginationOptions = [__deafultPageSize__];
                return dto;
            };

            var getLazyCompositions = function (schema) {
                if (!schema || !schema["cachedCompositions"]) {
                    return null;
                }
                var compositions = [];
                var cachedCompositions = schema.cachedCompositions;
                for (var composition in cachedCompositions) {
                    if (!cachedCompositions.hasOwnProperty(composition)) {
                        continue;
                    }
                    if (cachedCompositions[composition].fetchType == "Lazy") {
                        compositions.push(composition);
                    }
                }
                return compositions;
            };

            var fetchCompositions = function (requestDTO, datamap, showLoading) {
                var log = $log.getInstance('compositionservice#fetchCompositions');
                var urlToUse = url("/api/generic/ExtendedData/GetCompositionData");
                return $http.post(urlToUse, requestDTO, { avoidspin: !showLoading })
                    .then(function (response) {
                        var data = response.data;
                        var compositionArray = data.resultObject;
                        var result = {};
                        for (var composition in compositionArray) {
                            
                            if (!compositionArray.hasOwnProperty(composition)) {
                                continue;
                            }
                            var resultList = compositionArray[composition].resultList;
                            log.info('composition {0} returned with {1} entries'.format(composition, resultList.length));
                            //this datamap entry is bound to the whole screen, so we need to set it here as well
                            datamap[composition] = resultList;
                            result[composition] = {
                                relationship: composition,
                                list: resultList,
                                paginationData: compositionArray[composition].paginationData
                            };
                        }
                        return result;
                    });
            };

            var doPopulateWithCompositionData = function (requestDTO, datamap) {
                
                return fetchCompositions(requestDTO, datamap)
                    .then(function (result) {
                        
                        $timeout(function () {
                            $rootScope.$broadcast("sw_compositiondataresolved", result);
                        });
                        return result;
                    });
            };

            var buildFetchRequestDTO = function (schema, datamap, compositions, paginatedSearch) {
                var applicationName = schema.applicationName;
                // sanitizing data to submit
                var fieldsTosubmit = submitService.removeExtraFields(datamap, true, schema);
                var compositionNames = getLazyCompositions(schema);
                angular.forEach(compositionNames, function (composition) {
                    if (!fieldsTosubmit[composition] || !fieldsTosubmit.hasOwnProperty(composition)) {
                        return;
                    }
                    delete fieldsTosubmit[composition];
                });
                var parameters = {
                    key: {
                        schemaId: schema.schemaId,
                        mode: schema.mode,
                        platform: platform()
                    },
                    id: schemaService.getId(datamap, schema),
                    paginatedSearch: paginatedSearch || buildPaginatedSearchDTO()
                };
                if (compositions && compositions.length > 0) {
                    parameters.compositionList = compositions;
                }
                return {
                    application: applicationName,
                    request: parameters,
                    data: fieldsTosubmit
                };
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

                hasEditableProperty: function (listSchema) {
                    listSchema.jscache = listSchema.jscache || {};
                    if (listSchema.jscache.editable) {
                        return listSchema.jscache.editable;
                    }

                    var displayables = listSchema.displayables;
                    for (var i = 0; i < displayables.length; i++) {
                        var dis = displayables[i];
                        if (fieldService.isPropertyTrue(dis, "editable")) {
                            listSchema.jscache.editable = true;
                            return true;
                        }
                    }
                    listSchema.jscache.editable = false;
                    return false;
                },

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

                /*
                * this method will hit the server to fetch associated composition data on a second request making the detail screens faster
                *
                */
                populateWithCompositionData: function (schema, datamap) {
                    var applicationName = schema.applicationName;

                    var log = $log.getInstance('compositionservice#populateWithCompositionData');
                    log.info('going to server fetching composition data of {0}, schema {1}.'.format(applicationName, schema.schemaId));

                    var compositions = getLazyCompositions(schema);
                    // couldn't resolve compositions's names: fetch all
                    if (!compositions || compositions.length <= 0) {
                        var dto = buildFetchRequestDTO(schema, datamap);
                        return doPopulateWithCompositionData(dto, datamap);
                    }
                    // if compositions's names are resolved: one request per composition
                    var promises = compositions.map(function (composition) {
                        var localDTO = buildFetchRequestDTO(schema, datamap, [composition]);
                        return doPopulateWithCompositionData(localDTO, datamap);
                    });
                    return $q.all(promises);
                },

                /**
                 * Fetches a paginated list of associated compositions with relationship name matching composition
                 * 
                 * @param String composition name of the composition relationship
                 * @param Object schema composition's parent's schema
                 * @param Object datamap composition's parent's datamap
                 * @param Integer pageNumber number of the requested page
                 * @returns Promise 
                 *              resolved with parent's datamap populated with the fetched composition list 
                 *              and pagination data (datamap[composition] = { list: [Object], paginationData: Object });
                 *              rejected with HTTP error 
                 */
                getCompositionList: function (composition, schema, datamap, pageNumber) {
                    var pageRequest = buildPaginatedSearchDTO(pageNumber);
                    var dto = buildFetchRequestDTO(schema, datamap, [composition], pageRequest);
                    return fetchCompositions(dto, datamap, true);
                }

            };

        }]);

})(angular);