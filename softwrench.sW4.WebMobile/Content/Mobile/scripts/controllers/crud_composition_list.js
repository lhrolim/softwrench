softwrench.controller('CrudCompositionListController',
    function ($log, $scope, $rootScope, schemaService, crudContextService, fieldService, offlineAssociationService, $ionicPopover){


   
    $scope.isFieldHidden = function (fieldMetadata) {
        return fieldService.isFieldHidden(crudContextService.currentDetailItem(), crudContextService.currentDetailSchema(), fieldMetadata);
    }

    $scope.associationSearch = function (queryparameters) {
        return offlineAssociationService.filterPromise(crudContextService.currentDetailSchema(), $scope.datamap, queryparameters.identifier, queryparameters.query);
    }

    $scope.getAssociationLabelField = function (fieldMetadata) {
        return 'datamap.' + fieldMetadata.labelFields[0];
    }

    $scope.getAssociationValueField = function (fieldMetadata) {
        return 'datamap.' + fieldMetadata.valueField;
    }

  


}
);