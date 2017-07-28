(function (softwrench, _) {
    "use strict";

    softwrench.controller("CrudTabController", ["$scope", "$rootScope", "$log", "crudContextService", "crudContextHolderService", "schemaService", "inlineCompositionService",
        function ($scope, $rootScope, log, crudContextService, crudContextHolderService, schemaService, inlineCompositionService) {

            function init() {
                log.get("crud_tab#init").debug("crud tab init");
                const crudContext = crudContextHolderService.getCrudContext();
                $scope.tab = crudContext.composition.currentTab;
                $scope.tabAllDisplayables = schemaService.nonTabFieldsFromDisplayables($scope.tab.displayables);
                $scope.tabDisplayables = $scope.tabAllDisplayables;

                if (!$scope.tabInlineCompositionsLoaded) {
                    $scope.tabInlineCompositionsLoaded = true;
                    inlineCompositionService.loadInlineCompositions($scope.item, $scope.datamap, $scope.tabAllDisplayables);
                }
            }


            init();
        }]);
})(softwrench, _);



