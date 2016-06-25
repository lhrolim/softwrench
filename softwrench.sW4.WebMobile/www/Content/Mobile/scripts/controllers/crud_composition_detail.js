(function (softwrench) {
    "use strict";

    softwrench.controller('CrudCompositionDetailController', ["$log", "$scope", "$rootScope", "crudContextService", "fieldService", "offlineCompositionService", "offlineAssociationService", "schemaService", "eventService",
    function ($log, $scope, $rootScope, crudContextService, fieldService, offlineCompositionService, offlineAssociationService, schemaService, eventService) {

        function init() {
            $scope.schema = crudContextService.getCompositionDetailSchema();
            $scope.displayables =  schemaService.allDisplayables($scope.schema);
            $scope.datamap = crudContextService.getCompositionDetailItem();
            $scope.allowsUpdate = offlineCompositionService.allowsUpdate(crudContextService.getCompositionDetailItem(), crudContextService.getCompositionListSchema());

            eventService.onload($scope, $scope.schema, $scope.datamap, {});
        }

        $rootScope.$on('$stateChangeSuccess',
        function (event, toState, toParams, fromState, fromParams) {
            if (toState.name.startsWith("main.cruddetail.compositiondetail")) {
                //needs to refresh the displayables and datamap everytime the detail page is loaded.
                init();
            }
        });

        init();

    }]);
})(softwrench);



