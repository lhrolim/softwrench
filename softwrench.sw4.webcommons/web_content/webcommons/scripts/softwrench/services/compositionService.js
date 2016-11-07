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
            var log = $log.getInstance('compositionservice#fetchCompositions', ['composition']);
            var urlToUse = url("/api/generic/Composition/GetCompositionData");
            return $http.post(urlToUse, requestDTO, { avoidspin: !showLoading })
                .then(response => {
                    var data = response.data;
                    var parentModifiedFields = data.parentModifiedFields;
                    if (parentModifiedFields) {
                        //server has replied that some fields should change on parent datamap as well
                        angular.forEach(parentModifiedFields, (fieldValue, field) => {
                            if (parentModifiedFields.hasOwnProperty(field)) {
                                datamap[field] = fieldValue;
                            }
                        });
                    }
                    var compositionArray = data.resultObject;
                    var result = {};
                    angular.forEach(compositionArray, (compositionValue, composition) => {
                        if (!compositionArray.hasOwnProperty(composition)) {
                            return;
                        }
                        var resultList = compositionValue.resultList;
                        log.info('composition {0} returned with {1} entries'.format(composition, resultList.length));
                        //this datamap entry is bound to the whole screen, so we need to set it here as well
                        datamap[composition] = resultList;

                        var paginationData = compositionValue.paginationData;
                        // enforce composition pagination options
                        paginationData.paginationOptions = paginationData.paginationOptions ? paginationData.paginationOptions : config.defaultOptions;
                        //setting this case the tabs have not yet been loaded so that they can fetch from here
                        contextService.insertIntoContext("compositionpagination_{0}".format(composition), paginationData, true);
                        compositionContext[composition] = compositionValue;
                        result[composition] = {
                            relationship: composition,
                            list: resultList,
                            paginationData: paginationData
                        };
                    });
                    return result;
                //DO not remove this catch... for some chaotic reason the response is being swallowed without it.
                }).catch(err => console.log(err));
        };

        function doPopulateWithCompositionData(requestDTO, datamap) {

            return fetchCompositions(requestDTO, datamap)
                .then(result => {
                    $timeout(() => {
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

            angular.forEach(cachedCompositions, (compositionValue, composition) => {
                if (!cachedCompositions.hasOwnProperty(composition)) {
                    return;
                }
                if ("lazy".equalsIc(compositionValue.fetchType)) {
                    compositions.push(composition);
                } else if ("eager".equalsIc(compositionValue.fetchType)) {
                    compositionContext[composition] = datamap[composition];
                }
            });
            return compositions;
        };

        function isCompositionLodaded(relationship) {
            const value = compositionContext[relationship];
            return typeof value !== "undefined" && value !== null;
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
            const listSchema = compositionSchema.schemas.list;
            if (typeof listSchema === "undefined" || listSchema === null) {
                return null;
            }
            const toKeepProperty = listSchema.properties["composition.mainbuttonstoshow"];
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
         * @param {Object} compositiondetailschema composition's detail schema 
         * @returns Promise resolved with response's body 
         */
        function getCompositionDetailItem(compositionId, compositiondetailschema, customParams) {
            const applicationName = compositiondetailschema.applicationName;
            const parameters = {
                request: {
                    id: compositionId,
                    key: {
                        schemaId: compositiondetailschema.schemaId,
                        mode: compositiondetailschema.mode,
                        platform: "web"
                    }
                }
            };
            if (!!customParams) {
                parameters.request["customParameters"] = customParams;
            }

            const urlToCall = url(`/api/data/${applicationName}?${$.param(parameters)}`);
            return $http.get(urlToCall).then(result => result.data);
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

        function updateCompositionDataAfterSave(schema, datamap, responseDataMap) {
            const compositions = this.getLazyCompositions(schema, datamap) || [];
            compositions.forEach(function (composition) {
                const currentValue = datamap[composition];
                const updateFields = responseDataMap;

                const updatedValue = updateFields[composition];
                // has previous data but has no updated data: not safe to update -> hydrate with previous value
                if (!!currentValue && (!updateFields.hasOwnProperty(composition) ||
                    !angular.isArray(updatedValue))) {
                    updateFields[composition] = currentValue
                        .filter(function (c) { // filter out just created items
                            return !c["_iscreation"];
                        })
                        .map(function (c) { // remove `#isDirty` flag from the items
                            delete c["#isDirty"];
                            return c;
                        });
                }
            });
        }


        function generateBatchItemDatamap(idx,compositionlistSchema) {
            const newItem = {
                //used to make a differentiation between a compositionitem datamap and a regular datamap
                '#datamaptype': "compositionitem",
                '#datamapidx': idx
            };

            //this id will be placed on the entity so that angular can use it to track. 
            //It has to be negative to indicate its not a maximo Id, and also a unique value to avoid collisions
            const fakeNegativeId = -Date.now().getTime();
            newItem[compositionlistSchema.idFieldName] = fakeNegativeId;
            compositionlistSchema.displayables.forEach(d => {
                if (!d.isHidden) {
                    //in order to override parentdata on default lookupsearch
                    //check compositioncommons#buildMergedDatamap
                    if (d.target) {
                        newItem[d.target] = null;
                    } else {
                        newItem[d.attribute] = null;
                    }
                }
            });

            return newItem;
        }
       


        //#endregion

        const api = {
            generateBatchItemDatamap,
            locatePrintSchema,
            getTitle,
            getListCommandsToKeep,
            hasEditableProperty,
            buildMergedDatamap,
            populateWithCompositionData,
            getCompositionList,
            updateCompositionDataAfterSave,
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