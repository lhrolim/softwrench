(function (angular) {
    "use strict";



    function detailService($log, $q, $timeout, $rootScope, associationService, eventService, compositionService, fieldService, schemaService, contextService, crudContextHolderService) {

        function isEditDetail(schema, datamap) {
            return (fieldService.getId(datamap, schema) != undefined && "output" !== schema.mode);
        };

        function handleAssociations(datamap, schema, result) {
            const isAnonyomous = contextService.get("anonymous", false, true);
            const shouldFetchAssociations = !result.allAssociationsFetched && !isAnonyomous;

            //some associations might already been retrieved
            associationService.updateFromServerSchemaLoadResult(result.associationOptions,null, !shouldFetchAssociations);

            if (shouldFetchAssociations) {
                return $timeout(function () {
                    //why this timeout?
                    $log.get("#detailService#fetchRelationshipData").info('fetching eager associations of {0}'.format(schema.applicationName));
                    associationService.loadSchemaAssociations(datamap, schema, { avoidspin: true }).then((result) => {
                        eventService.onassociationsloaded(schema);
                        return result;
                    });

                });
            } else {
                //they are all resolved already
                contextService.insertIntoContext("associationsresolved", true, true);

                return $q.when();
            }
        }

        function handleCompositions(datamap, schema, result) {
            const isEdit = isEditDetail(schema, datamap);
            if (!isEdit) {
                return $q.when();
            }

            //fetch composition data only for edit mode
            var shouldFetchCompositions = !schemaService.isPropertyTrue(result.schema, "detail.prefetchcompositions");

            if (!shouldFetchCompositions) {
                return $q.when(result.compositions);
            }

            return $timeout(function () {
                if (shouldFetchCompositions) {
                    $log.get("#detailService#fetchRelationshipData").info('fetching compositions of {0}'.format(schema.applicationName));
                    return compositionService.populateWithCompositionData(schema, datamap);
                }
                return $q.when();
            });

        }

        function fetchRelationshipData(datamap, schema, result) {
            crudContextHolderService.clearDetailDataResolved();
            const associationPromise = handleAssociations(datamap, schema, result);
            const compositionPromise = handleCompositions(datamap, schema, result);
            return $q.all([associationPromise, compositionPromise])
                .then(function () {
                    //ready to listen for dirty watchers
                    $log.get("detailService#fetchRelationshipData", ["detail"]).info("associations and compositions fetched");
                })
                .finally(function () {
                    crudContextHolderService.setDetailDataResolved();
                });
        };

        //TODO: move this to the crudContextHolderService
        //holding structures to navigate to next and previous elements on the detail page
        function updateLegacyCrudContext(datamap, schema, panelId) {
            datamap = datamap || crudContextHolderService.rootDataMap(panelId);
            schema = schema || crudContextHolderService.currentSchema(panelId);

            const crudContext = contextService.fetchFromContext("crud_context", true);
            if (!crudContext) {
                //this might happen if we´re handling a direct link
                return;
            }

            if (crudContext.panelid != null || crudContext.applicationName !== schema.applicationName) {
                // the list was from a modal or dashboard panel
                // or is from another aplication
                // should not be considered when goint to detail
                crudContext.detail_previous = null;
                crudContext.detail_next = null;
                crudContext.list_elements = null;
                crudContext.panelid = null;
                contextService.insertIntoContext("crud_context", crudContext);
            }

            var id = datamap[schema.idFieldName];
            const list = crudContext.list_elements;
            if (!list || !id) {
                return;
            }

            var idAsString = String(id);
            const findById = function (item) {
                return item.id === id || item.id === idAsString;
            };
            if (list.find(findById)) {
                const current = list.findIndex(findById);
                const previous = current - 1;
                const next = current + 1;
                crudContext.detail_previous = list[previous];
                crudContext.detail_next = list[next];
            } else {
                // not on list, so new created
                crudContext.detail_previous = null;
                crudContext.detail_next = list.length > 0 ? list[0] : null;
                list.unshift({ id: id });
            }
            contextService.insertIntoContext("crud_context", crudContext);


        }

        function detailLoaded(datamap, schema, result) {
            updateLegacyCrudContext(datamap, schema);
            crudContextHolderService.detailLoaded();

            return fetchRelationshipData(datamap, schema, result);


        }

        return {
            detailLoaded
        };
    };

    angular.module("sw_layout")
        .service("detailService",
        ["$log", "$q", "$timeout", "$rootScope", "associationService", "eventService", "compositionService", "fieldService", "schemaService", "contextService", "crudContextHolderService", detailService]);

})(angular);