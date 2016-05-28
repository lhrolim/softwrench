(function (angular, $) {
    "use strict";

    var app = angular.module('sw_layout');

    app.factory('compositionService',
        ["$log", "$http", "$rootScope", "$timeout", "contextService", "submitService", "schemaService", "searchService", "$q", "fieldService",
            "compositionCommons", "crudContextHolderService", "tabsService", "userPreferencesService",
            function($log,
                $http,
                $rootScope,
                $timeout,
                contextService,
                submitService,
                schemaService,
                searchService,
                $q,
                fieldService,
                compositionCommons,
                crudContextHolderService,
                tabsService,
                userPreferencesService) {

                var config = {
                    defaultPageSize: 10,
                    defaultOptions: [10, 30, 100],
                    defaultRequestOptions: [0, 10, 30]
                };

                //stores the context of the current detail loaded compositions
                var compositionContext = {};


                


        //#region private methods

        function buildPaginatedSearchDTO(pageNumber, pageSize) {
            var dto = searchService.buildSearchDTO();
            dto.pageNumber = pageNumber || 1;
            dto.pageSize = pageSize === "all" ? 0 : pageSize || config.defaultPageSize;
            dto.totalCount = 0;
            dto.paginationOptions = config.defaultRequestOptions;
            return dto;
        };



        function fetchCompositions(requestDTO, datamap, showLoading) {
            var log = $log.getInstance('compositionservice#fetchCompositions');
            var urlToUse = url("/api/generic/Composition/GetCompositionData");
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

                        var paginationData = compositionArray[composition].paginationData;
                        // enforce composition pagination options
                        paginationData.paginationOptions = paginationData.paginationOptions ? paginationData.paginationOptions : config.defaultOptions;
                        //setting this case the tabs have not yet been loaded so that they can fetch from here
                        contextService.insertIntoContext("compositionpagination_{0}".format(composition), paginationData, true);
                        compositionContext[composition] = compositionArray[composition];
                        result[composition] = {
                            relationship: composition,
                            list: resultList,
                            paginationData: paginationData
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
                        crudContextHolderService.compositionsLoaded(result);
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

            var pageSize = userPreferencesService.getSchemaPreference("compositionPageSize", schema.applicationName, schema.schemaId);
            if (pageSize && paginatedSearch && paginatedSearch.pageSize !== 0) {
                paginatedSearch.pageSize = pageSize;
            }

            var parameters = {
                key: {
                    schemaId: schema.schemaId,
                    mode: schema.mode,
                    platform: platform()
                },
                id: schemaService.getId(datamap, schema),
                paginatedSearch: paginatedSearch || buildPaginatedSearchDTO(null, pageSize)
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

        //#endregion

        //#region Public 

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

        function isCompositionLodaded(relationship) {
            return compositionContext[relationship] != null;
        };

        function locatePrintSchema(baseSchema, compositionKey) {
            var schemas = tabsService.nonInlineCompositionsDict(baseSchema);
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
            var schemas = tabsService.nonInlineCompositionsDict(baseSchema);
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
         * @param Integer pageSize number of items per page
         * @returns Promise 
         *              resolved with parent's datamap populated with the fetched composition list 
         *              and pagination data (datamap[composition] = { list: [Object], paginationData: Object });
         *              rejected with HTTP error 
         */
        function getCompositionList(composition, schema, datamap, pageNumber, pageSize) {
            var pageRequest = buildPaginatedSearchDTO(pageNumber, pageSize);
            var dto = buildFetchRequestDTO(schema, datamap, [composition], pageRequest);
            return fetchCompositions(dto, datamap, true);
        }

        function searchCompositionList(composition, schema, datamap, searchDTO) {
            var dto = buildFetchRequestDTO(schema, datamap, [composition], searchDTO);
            return fetchCompositions(dto, datamap, true);
        }

        /**
         * Fetches the composition item with the specified id
         * 
         * @param String|Number compositionId item's id
         * @param {} compositiondetailschema composition's detail schema 
         * @returns Promise resolved with response's body 
         */
        function getCompositionDetailItem(compositionId, compositiondetailschema) {
            var parameters = {};
            var request = {};
            var key = {};
            var applicationName = compositiondetailschema.applicationName;
            parameters.request = request;
            request.id = compositionId;
            request.key = key;
            key.schemaId = compositiondetailschema.schemaId;
            key.mode = compositiondetailschema.mode;
            key.platform = "web";
            var urlToCall = url("/api/data/" + applicationName + "?" + $.param(parameters));
            return $http.get(urlToCall).then(function (result) {
                return result.data;
            });
        }

        function isCompositionListItem(datamap) {
            return datamap && datamap["#datamaptype"] === "compositionitem";
        }

        function buildCompositionListItemContext(contextData, datamap, schema) {
            contextData = contextData || {};
            if (!contextData["schemaId"]) {
                contextData = { schemaId: schema.schemaId }
            }
            contextData["entryId"] = "compositionitem_" + datamap[schema.idFieldName];
            return contextData;
        }

        function resolveCompositions(compositionData) {

            Object.keys(compositionData).forEach((relationship) => {
                crudContextHolderService.compositionQueue()[relationship] = { [relationship]: compositionData[relationship] };
            });
            
            $rootScope.$broadcast("sw_compositiondataresolved", compositionData);
        }

        function pollCompositionEvent(relationship) {
            var compositionLoadEventQueue = crudContextHolderService.compositionQueue();
            if (compositionLoadEventQueue.hasOwnProperty(relationship)) {
                var compositionData = compositionLoadEventQueue[relationship];
                delete compositionLoadEventQueue[relationship];
                return compositionData;
            }
            return null;
        }


        //#endregion

        const api = {
            locatePrintSchema,
            getTitle,
            getListCommandsToKeep,
            hasEditableProperty,
            buildMergedDatamap,
            populateWithCompositionData,
            getCompositionList,
            searchCompositionList,
            isCompositionLodaded,
            getLazyCompositions,
            getCompositionDetailItem,
            isCompositionListItem,
            buildCompositionListItemContext,
            resolveCompositions,
            pollCompositionEvent
        };

        return api;


    }]);

})(angular, jQuery);