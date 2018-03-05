(function (angular) {
    'use strict';

    let doGetLookupOptions;

    class lookupService {

        constructor($rootScope, $timeout, $http, $log, $q, associationService, schemaService, searchService, crudContextHolderService, datamapSanitizeService, i18NService) {
            this.$rootScope = $rootScope;
            this.$timeout = $timeout;
            this.$http = $http;
            this.$log = $log;
            this.$q = $q;
            this.associationService = associationService;
            this.schemaService = schemaService;
            this.searchService = searchService;
            this.crudContextHolderService = crudContextHolderService;
            this.datamapSanitizeService = datamapSanitizeService;
            this.i18NService = i18NService;

            //#region private fns

            //This takes the lookupObj, pageNumber, and searchObj (dictionary of attribute (key) 
            //to its value that will filter the lookup), build a searchDTO, and return the post call to the
            //UpdateAssociations function in the ExtendedData controller.
            doGetLookupOptions = (schema, datamap, lookupDTO, searchDTO, searchDatamap) => {
                let fields = datamap;
                if (!!searchDatamap) {
                    fields = searchDatamap;
                }
                const eagerOptions = this.getEagerLookupOptions(lookupDTO, searchDTO);
                if (!!eagerOptions) {
                    return this.$q.when(eagerOptions);
                }


                //this should reflect LookupOptionsFetchRequestDTO.cs
                const parameters = {
                    parentKey: this.schemaService.buildApplicationMetadataSchemaKey(schema),
                    associationFieldName: lookupDTO.fieldMetadata.associationKey
                };

                parameters.searchDTO = searchDTO;
                const urlToUse = url("/api/generic/Association/GetLookupOptions?" + $.param(parameters));
                const jsonString = angular.toJson(this.datamapSanitizeService.sanitizeDataMapToSendOnAssociationFetching(fields));
                return this.$http.post(urlToUse, jsonString, {
                    headers: {
                        panelid: "#modal"
                    }
                }).then(response => {
                    return response.data;
                });
            }


            //#endregion

        }



        clearAutoCompleteCache(associationKey = RequiredParam) {
            return this.$rootScope.$broadcast(JavascriptEventConstants.ClearAutoCompleteCache, associationKey);
        }

        getEagerLookupOptions(lookupObj, quickSearchDTO) {
            const isShowingModal = this.crudContextHolderService.isShowingModal();
            let contextData = null;
            if (isShowingModal) {
                contextData = {
                    schemaId: "#modal"
                };
            }
            let eagerOptions = this.crudContextHolderService.fetchEagerAssociationOptions(lookupObj.fieldMetadata.associationKey, contextData);
            if (!eagerOptions) {
                return null;
            }
            if (quickSearchDTO) {
                const quickSearchData = quickSearchDTO.quickSearchData || "";
                eagerOptions = eagerOptions.filter(a => {
                    return a.value.containsIgnoreCase(quickSearchData) || a.label.containsIgnoreCase(quickSearchData);
                });
            }
            //adapting response
            return {
                resultObject: {
                    associationData: eagerOptions,
                    totalCount: eagerOptions.length,
                    pageSize: eagerOptions.length,
                    pageCount: 1,
                    pageNumber: 1,
                    paginationOptions: [eagerOptions.length]
                }
            }
        }

        /**
         * 
         * @param {} fieldMetadata 
         * @param {} datamap 
         * @param {} datamapId used to diferentiate compositions entries
         * @param {} newValue 
         * @returns {} 
         */
        refreshFromAttribute(fieldMetadata, datamap, datamapId, newValue) {
            var log = this.$log.getInstance("lookupService#refreshFromAttribute", ["association", "lookup"]);

            var associationKey = fieldMetadata.associationKey;

            this.associationService.getLabelText(associationKey, newValue, {
                hideDescription: fieldMetadata.hideDescription,
                allowTransientValue: fieldMetadata.rendererParameters["allowcustomvalue"] === "true",
                hideValue: fieldMetadata.rendererParameters["hidevalue"] === "true",
                isEager: !!fieldMetadata.providerAttribute
            }).then(function (label) {
                var key = fieldMetadata.applicationPath;
                if (datamapId) {
                    key += datamapId;
                }
                key = replaceAll(key, "\\.", "_");

                log.debug("setting lookup {0} to {1}".format(key, label));
                const el = $("input[data-displayablepath=" + key + "]");
                if (el.length === 0) {
                    log.warn("lookup {0} not found".format(key));
                }
                el.typeahead("val", label);
                log.debug("setting", associationKey, "to", label);
            });
        }

        /**
         * This method populates the lookupObj that is shared with both the lookupinput and lookupmodal directives and hits the server for fetching the data to show on screen
         * 
         * @param {} lookupDTO instance of LookupDTO
         * @param {} mainDatamap  the datamap on the screen to be updated
         * @param {} searchDatamap an optional datamap to be enforced for the search. Usually, it�s needed on compositions or modals, 
         * where we need to set extra values for searching while keeping the screen-state intact
         * @param {} overrideschema the schema to be used forcibly
         * @returns {Promise} for an update lookupObj 
         */
        initLookupModal(lookupDTO, mainDatamap, searchDatamap, overrideschema) {
            if (!(lookupDTO instanceof LookupDTO)) {
                throw new Error("expected paramter to be a lookupDTO instance");
            }

            const searchDTO = new SearchDTO({ addPreSelectedFilters: true });
            lookupDTO.searchData = {};
            lookupDTO.searchOperator = {};

            return this.getLookupOptions(lookupDTO, searchDTO, mainDatamap, searchDatamap, overrideschema);

        }

        /**
       * 
       * @param {} lookupObj 
       * @param {} postFetchHook 
       * @param {} searchObj 
       * @param {} datamap if passed will consider this object instead of the root datamap of the crudcontext
       * @param {} overrideschema the schema to be used forcibly
       * @returns {} 
       */
        getLookupOptions(lookupDTO, searchDTO, datamap, searchDatamap, overrideschema) {

            const log = this.$log.get("lookupService#getLookupOptions", ["modal"]);
            log.debug("get lookup options init");

            const panelId = this.crudContextHolderService.isShowingModal() ? "#modal" : null;
            datamap = datamap || this.crudContextHolderService.rootDataMap(panelId);
            const currentSchema = overrideschema ? overrideschema : this.crudContextHolderService.currentSchema(panelId);

            if (lookupDTO.modalPaginationData != null) {
                //scenario where a first paginated search had already been performed
                searchDTO.totalCount = lookupDTO.modalPaginationData.totalCount || 0;
                searchDTO.pageSize = lookupDTO.modalPaginationData.pageSize || 30;
            }

            return doGetLookupOptions(currentSchema, datamap, lookupDTO, searchDTO, searchDatamap)
                .then(data => {
                    const result = data.resultObject;
                    return this.updateLookupObjFromServerResult(result, lookupDTO);
                });

        }


        updateLookupObjFromServerResult(result, lookupObj) {

            const associationResult = result;
            lookupObj.schema = associationResult.associationSchemaDefinition;
            if (!lookupObj.schema && lookupObj.fieldMetadata.type !== "OptionField" && !angular.mock) {
                lookupObj.schema = {
                    schemaid: "#defaultmodal",
                    displayables: [
                        {
                            label: lookupObj.fieldMetadata.label,
                            attribute: lookupObj.fieldMetadata.attribute,
                            qualifier: "value",
                            rendererParameters: {},
                            type: "ApplicationFieldDefinition"
                        },
                        {
                            label: this.i18NService.getLookUpDescriptionLabel(lookupObj.fieldMetadata),
                            attribute: lookupObj.fieldMetadata.labelFields[0],
                            rendererParameters: {},
                            qualifier: "label",
                            type: "ApplicationFieldDefinition"
                        }
                    ],
                    properties: {},
                    schemaFilters: {
                        filters: [
                            {
                                label: lookupObj.fieldMetadata.label,
                                attribute: lookupObj.fieldMetadata.attribute,
                                rendererParameters: {},
                                type: "ApplicationFieldDefinition"
                            },
                            {
                                label: this.i18NService.getLookUpDescriptionLabel(lookupObj.fieldMetadata),
                                attribute: lookupObj.fieldMetadata.labelFields[0],
                                rendererParameters: {},
                                type: "ApplicationFieldDefinition"
                            }
                        ]
                    }
            }
            }

            lookupObj.options = associationResult.associationData;


            lookupObj.modalPaginationData = {
                pageCount: associationResult.pageCount,
                pageNumber: associationResult.pageNumber,
                pageSize: associationResult.pageSize,
                totalCount: associationResult.totalCount,
                selectedPage: associationResult.pageNumber,
                paginationOptions: associationResult.paginationOptions || [10, 30, 100]
            };


            if (associationResult.searchDTO && associationResult.searchDTO.searchParams && associationResult.searchDTO.searchValues) {
                const searchDataAndOperator = this.searchService.buildSearchDataAndOperations(associationResult.searchDTO.searchParams, associationResult.searchDTO.searchValues);
                lookupObj.searchData = searchDataAndOperator.searchData;
                lookupObj.searchOperator = searchDataAndOperator.searchOperator;
            }

            if (!!associationResult.affectedProfiles && associationResult.affectedProfiles.length > 1) {
                this.crudContextHolderService.lookupDataLoaded(associationResult);
            }

            return lookupObj;
        }



    }


    lookupService.$inject = ['$rootScope', '$timeout', "$http", '$log', '$q', 'associationService', 'schemaService', 'searchService', "crudContextHolderService", "datamapSanitizeService", "i18NService"];

    angular.module("sw_lookup").service('lookupService', lookupService);

})(angular);