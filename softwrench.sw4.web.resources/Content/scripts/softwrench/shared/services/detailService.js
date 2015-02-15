var app = angular.module('sw_layout');

app.factory('detailService', function ($log, $http,$timeout, associationService, compositionService, fieldService) {

    return {

        fetchAssociatedData: function (scope, result) {
            var datamap = scope.datamap.fields;
            var schema = scope.schema;
            var isEdit = this.isEditDetail(schema, datamap);
            var shouldFetchAssociations = result.schema.properties['prefetchassociations'] != "true";
            //fetch composition data only for edit mode
            var shouldFetchCompositions = result.schema.properties['prefetchcompositions'] != "true" && isEdit;
            if (!shouldFetchAssociations) {
                associationService.updateAssociationOptionsRetrievedFromServer(scope, result.associationOptions, datamap);
            }
            $timeout(function () {
                if (shouldFetchAssociations) {
                    associationService.getEagerAssociations(scope);
                }
                if (shouldFetchCompositions) {
                    compositionService.populateWithCompositionData(schema, datamap);
                }
            });
        },

        isEditDetail: function (schema, datamap) {
            return fieldService.getId(datamap, schema) != undefined;
        }


    };

});


