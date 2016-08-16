(function (angular) {
    "use strict";

    

    function detailService($log, $q, $timeout, $rootScope, associationService,eventService, compositionService, fieldService, schemaService, contextService, crudContextHolderService) {

        function isEditDetail(schema, datamap) {
            return fieldService.getId(datamap, schema) != undefined;
        };

        function handleAssociations(scope, result) {
            var shouldFetchAssociations = !result.allAssociationsFetched;

            //some associations might already been retrieved
            associationService.updateFromServerSchemaLoadResult(result.associationOptions, !shouldFetchAssociations);

            if (shouldFetchAssociations) {
                return $timeout(function () {
                    //why this timeout?
                    $log.get("#detailService#fetchRelationshipData").info('fetching eager associations of {0}'.format(scope.schema.applicationName));
                    associationService.loadSchemaAssociations(scope.datamap, scope.schema, { avoidspin: true }).then(function (result) {
                        var eventInvocation = eventService.dispatchEvent(scope.schema, "onassociationsloaded");
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
            var datamap = scope.datamap;
            var schema = scope.schema;
            var isEdit = isEditDetail(schema, datamap);

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
            var associationPromise = handleAssociations(scope, result);
            var compositionPromise = handleCompositions(scope, result);
            return $q.all([associationPromise, compositionPromise])
                .then(function (results) {
                    //ready to listen for dirty watchers
                    $log.get("detailService#fetchRelationshipData").info("associations and compositions fetched");
                })
                .finally(function () {
                    crudContextHolderService.setDetailDataResolved();
                });
        };

        var api = {
            fetchRelationshipData: fetchRelationshipData,
            isEditDetail: isEditDetail
        };

        return api;
    };

    angular.module("sw_layout")
        .factory("detailService",
            ["$log", "$q", "$timeout", "$rootScope", "associationService", "eventService", "compositionService", "fieldService", "schemaService", "contextService", "crudContextHolderService", detailService]);

})(angular);