(function () {
    "use strict";


    detailService.$inject = ["$log", "$http", "$timeout", "associationService", "compositionService", "fieldService", "schemaService", "contextService"];

    angular.module("sw_layout").factory("detailService", detailService);

    function detailService($log, $http, $timeout, associationService, compositionService, fieldService, schemaService, contextService) {

        var api = {
            fetchRelationshipData: fetchRelationshipData,
            isEditDetail: isEditDetail
        };

        return api;

        function fetchRelationshipData(scope, result) {

            var datamap = scope.datamap.fields;
            var schema = scope.schema;
            var isEdit = this.isEditDetail(schema, datamap);

            handleAssociations(scope, result);
            

            //fetch composition data only for edit mode
            if (!isEdit) {
                return;
            }

            handleCompositions(scope, result);

        };

        function isEditDetail(schema, datamap) {
            return fieldService.getId(datamap, schema) != undefined;
        };

        function handleAssociations(scope,result) {
            var shouldFetchAssociations = schemaService.getProperty(result.schema, "prefetchassociations") != "#all";

            //some associations might already been retrieved
            associationService.updateAssociationOptionsRetrievedFromServer(scope, result.associationOptions, scope.datamap.fields);

            if (shouldFetchAssociations) {
                $timeout(function () {
                    //why this timeout?
                    $log.get("#detailService#fetchRelationshipData").info('fetching eager associations of {0}'.format(scope.schema.applicationName));
                    associationService.getEagerAssociations(scope);

                });
            } else {
                //they are all resolved already
                contextService.insertIntoContext("associationsresolved", true, true);
            }
        }

        function handleCompositions(scope,result) {
            var shouldFetchCompositions = !schemaService.isPropertyTrue(result.schema, "detail.prefetchcompositions");

            if (!shouldFetchCompositions) {
                scope.compositions = result.compositions;
            }

            $timeout(function () {
                if (shouldFetchCompositions) {
                    $log.get("#detailService#fetchRelationshipData").info('fetching compositions of {0}'.format(scope.schema.applicationName));
                    compositionService.populateWithCompositionData(scope.schema, scope.datamap.fields);
                }
            });
        }

    };





})();

ngservice




