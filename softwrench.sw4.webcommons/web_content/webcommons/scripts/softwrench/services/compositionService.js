﻿(function (angular) {
    "use strict";

    var app = angular.module('sw_layout');

    app.factory('compositionService',
        ["$log", "$http", "$rootScope", "$timeout", "contextService", "submitService", "schemaService", "searchService", "$q", "fieldService", "compositionCommons",
        function ($log, $http, $rootScope, $timeout, contextService, submitService, schemaService, searchService, $q, fieldService, compositionCommons) {

            var __deafultPageSize__ = 10;

            //stores the context of the current detail loaded compositions
            var compositionContext = {};

            var api = {
                locatePrintSchema: locatePrintSchema,
                getTitle: getTitle,
                getListCommandsToKeep: getListCommandsToKeep,
                hasEditableProperty: hasEditableProperty,
                buildMergedDatamap: buildMergedDatamap,
                populateWithCompositionData: populateWithCompositionData,
                getCompositionList: getCompositionList,
                isCompositionLodaded: isCompositionLodaded
            };

            return api;


            //private methods

            function nonInlineCompositionsDict(schema) {
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

            function buildPaginatedSearchDTO(pageNumber) {
                var dto = searchService.buildSearchDTO();
                dto.pageNumber = pageNumber || 1;
                dto.pageSize = __deafultPageSize__;
                dto.totalCount = 0;
                // dto.paginationOptions = [__deafultPageSize__];
                return dto;
            };

            function getLazyCompositions(schema, datamap) {
                if (!schema || !schema["cachedCompositions"]) {
                    return null;
                }
                var compositions = [];
                var cachedCompositions = schema.cachedCompositions;
                for (var composition in cachedCompositions) {
                    if (!cachedCompositions.hasOwnProperty(composition)) {
                        continue;
                    }
                    if ("lazy".equalsIc(cachedCompositions[composition].fetchType)) {
                        compositions.push(composition);
                    } else if ("eager".equalsIc(cachedCompositions[composition].fetchType)) {
                        compositionContext[composition] = datamap[composition];
                    }
                }
                return compositions;
            };

            function fetchCompositions(requestDTO, datamap, showLoading) {
                var log = $log.getInstance('compositionservice#fetchCompositions');
                var urlToUse = url("/api/generic/ExtendedData/GetCompositionData");
                return $http.post(urlToUse, requestDTO, { avoidspin: !showLoading })
                    .then(function (response) {
                        var data = response.data;
                        var parentModifiedFields = data.parentModifiedFields;
                        if (parentModifiedFields) {
                            //server has replied that some fields should change on parent datamap as well
                            for (var field in parentModifiedFields) {
                                if (parentModifiedFields.hasOwnProperty(field)) {
                                    datamap[field] = parentModifiedFields[field];
                                }
                            }
                        }
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
                            //setting this case the tabs have not yet been loaded so that they can fetch from here
                            contextService.insertIntoContext("compositionpagination_{0}".format(composition), compositionArray[composition].paginationData, true);
                            compositionContext[composition] = compositionArray[composition];
                            result[composition] = {
                                relationship: composition,
                                list: resultList,
                                paginationData: compositionArray[composition].paginationData
                            };
                        }
                        return result;
                    });
            };

            function doPopulateWithCompositionData(requestDTO, datamap) {

                return fetchCompositions(requestDTO, datamap)
                    .then(function (result) {

                        $timeout(function () {
                            $rootScope.$broadcast("sw_compositiondataresolved", result);
                        });
                        return result;
                    });
            };

            function buildFetchRequestDTO(schema, datamap, compositions, paginatedSearch) {
                var applicationName = schema.applicationName;
                // sanitizing data to submit
                var fieldsTosubmit = submitService.removeExtraFields(datamap, true, schema);
                var compositionNames = getLazyCompositions(schema, datamap);
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
                parameters.compositionList = compositionNames;

                if (compositions && compositions.length > 0) {
                    parameters.compositionList = compositions;
                }
                return {
                    application: applicationName,
                    request: parameters,
                    data: fieldsTosubmit
                };
            };


            // Public methods

            function isCompositionLodaded(relationship) {
                return compositionContext[relationship] != null;
            };

            function locatePrintSchema(baseSchema, compositionKey) {
                var schemas = nonInlineCompositionsDict(baseSchema);
                var thisSchema = schemas[compositionKey];

                if (thisSchema.schema.schemas.print != null) {
                    return thisSchema.schema.schemas.print;
                } else if (thisSchema.schema.schemas.list != null) {
                    return thisSchema.schema.schemas.list;
                } else {
                    return thisSchema.schema.schemas.detail;
                }
            };

            function getTitle(baseSchema, compositionKey) {
                var schemas = nonInlineCompositionsDict(baseSchema);
                var thisSchema = schemas[compositionKey];
                return thisSchema.label;
            };

            function getListCommandsToKeep(compositionSchema) {
                var listSchema = compositionSchema.schemas.list;
                if (listSchema == null) {
                    return null;
                }
                var toKeepProperty = listSchema.properties["composition.mainbuttonstoshow"];
                if (!nullOrEmpty(toKeepProperty)) {
                    return toKeepProperty.split(';');
                }
                return [];
            };

            /**
             * @deprecated use schemaService#hasEditableProperty instead
             */
            function hasEditableProperty(listSchema) {
                return schemaService.hasEditableProperty(listSchema);
            };

            /**
             * @deprecated use compositionCommons#buildMergedDatamap instead 
             */
            function buildMergedDatamap(datamap, parentdata) {
                return compositionCommons.buildMergedDatamap(datamap, parentdata);
            };

            /*
            * this method will hit the server to fetch associated composition data on a second request making the detail screens faster
            *
            */
            function populateWithCompositionData(schema, datamap) {
                var applicationName = schema.applicationName;
                var log = $log.getInstance('compositionservice#populateWithCompositionData');
                log.info('going to server fetching composition data of {0}, schema {1}.'.format(applicationName, schema.schemaId));
                compositionContext = {};
                // fetching all compositions in a single http request:
                // browser limits simultaneous client requests (usually 6).
                // doing in a single request so it doesn't impact static files fetching and page loading              
                var dto = buildFetchRequestDTO(schema, datamap);
                return doPopulateWithCompositionData(dto, datamap);
            };

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
            function getCompositionList(composition, schema, datamap, pageNumber) {
                var pageRequest = buildPaginatedSearchDTO(pageNumber);
                var dto = buildFetchRequestDTO(schema, datamap, [composition], pageRequest);
                return fetchCompositions(dto, datamap, true);
            }



        }]);

})(angular);