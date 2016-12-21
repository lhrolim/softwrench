(function (angular) {
    "use strict";



    function detailService($log, $q, $timeout, $rootScope, associationService, eventService, compositionService, fieldService, schemaService, contextService, crudContextHolderService) {

        function isEditDetail(schema, datamap) {
            return fieldService.getId(datamap, schema) != undefined;
        };

        function handleAssociations(scope, result) {
            const shouldFetchAssociations = !result.allAssociationsFetched;

            //some associations might already been retrieved
            associationService.updateFromServerSchemaLoadResult(result.associationOptions, !shouldFetchAssociations);

            if (shouldFetchAssociations) {
                return $timeout(function () {
                    //why this timeout?
                    $log.get("#detailService#fetchRelationshipData").info('fetching eager associations of {0}'.format(scope.schema.applicationName));
                    associationService.loadSchemaAssociations(scope.datamap, scope.schema, { avoidspin: true }).then(function (result) {
                        eventService.onassociationsloaded(scope.schema);
                        return result;
                    });

                });
            } else {
                //they are all resolved already
                contextService.insertIntoContext("associationsresolved", true, true);

                return $q.when();
            }
        }

        function handleCompositions(scope, result) {
            const datamap = scope.datamap;
            const schema = scope.schema;
            const isEdit = isEditDetail(schema, datamap);
            if (!isEdit) {
                return $q.when();
            }

            //fetch composition data only for edit mode
            var shouldFetchCompositions = !schemaService.isPropertyTrue(result.schema, "detail.prefetchcompositions");

            if (!shouldFetchCompositions) {
                scope.compositions = result.compositions;
            }

            return $timeout(function () {
                if (shouldFetchCompositions) {
                    $log.get("#detailService#fetchRelationshipData").info('fetching compositions of {0}'.format(scope.schema.applicationName));
                    return compositionService.populateWithCompositionData(scope.schema, scope.datamap);
                }
                return $q.when();
            });

        }

        function fetchRelationshipData(scope, result) {
            crudContextHolderService.clearDetailDataResolved();
            const associationPromise = handleAssociations(scope, result);
            const compositionPromise = handleCompositions(scope, result);
            return $q.all([associationPromise, compositionPromise])
                .then(function () {
                    //ready to listen for dirty watchers
                    $log.get("detailService#fetchRelationshipData").info("associations and compositions fetched");
                })
                .finally(function () {
                    crudContextHolderService.setDetailDataResolved();
                });
        };

        function updateCrudContext(datamap,schema,panelId) {
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

        const api = {
            fetchRelationshipData,
            isEditDetail,
            updateCrudContext
        };
        return api;
    };

    angular.module("sw_layout")
        .service("detailService",
            ["$log", "$q", "$timeout", "$rootScope", "associationService", "eventService", "compositionService", "fieldService", "schemaService", "contextService", "crudContextHolderService", detailService]);

})(angular);