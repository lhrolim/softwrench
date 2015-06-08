softwrench.controller('CrudInputController', function ($log, $scope, $rootScope, schemaService, crudContextService, fieldService, offlineAssociationService, $ionicPopover) {


    $ionicPopover.fromTemplateUrl('Content/Mobile/templates/compositionmenu.html', {
        scope: $scope,
    }).then(function (popover) {
        $scope.compositionpopover = popover;
    });

  

    $scope.associationSearch = function (queryparameters) {
        return offlineAssociationService.filterPromise(crudContextService.currentDetailSchema(), $scope.datamap, queryparameters.identifier, queryparameters.query);
    }

    $scope.getAssociationLabelField = function (fieldMetadata) {
        return 'datamap.' + fieldMetadata.labelFields[0];
    }

    $scope.getAssociationValueField = function (fieldMetadata) {
        return 'datamap.' + fieldMetadata.valueField;
    }

    $scope.isFieldHidden = function (fieldMetadata) {
        return fieldService.isFieldHidden($scope.datamap, $scope.schema, fieldMetadata);
    }
  


}
);