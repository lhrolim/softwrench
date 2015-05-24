softwrench.controller('CrudInputController', function ($log, $scope, $rootScope, crudContextService, fieldService, schemaService, offlineSchemaService, statuscolorService) {

    function init() {
        $scope.displayables = crudContextService.mainDisplayables();
        $scope.datamap = crudContextService.currentDetailItem();
    }



    $scope.title = function () {
        return crudContextService.currentTitle();
    }

    $scope.isFieldHidden = function (fieldMetadata) {
        return fieldService.isFieldHidden(crudContextService.currentDetailItem(), crudContextService.currentDetailSchema(), fieldMetadata);
    }

    $rootScope.$on('$stateChangeSuccess',
        function (event, toState, toParams, fromState, fromParams) {
            if (toState.name == "main.cruddetail") {
                init();
            }
        });

    init();


}
);