(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("crudTree", ["crudContextHolderService", "contextService", function (crudContextHolderService, contextService) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/crud_tree.html"),
            scope: {
                fieldMetadata: "=",
                datamap: "=",
                schema: "="
            },

            link: function (scope, element, attrs) {

            },

            controller: ["$scope", "$injector", "i18NService", "fieldService", "formatService", "layoutservice", "expressionService",
                function ($scope, $injector, i18NService, fieldService, formatService, layoutservice, expressionService) {
                    $injector.invoke(BaseController, this, {
                        $scope: $scope,
                        i18NService: i18NService,
                        fieldService: fieldService,
                        formatService: formatService,
                        layoutservice: layoutservice,
                        expressionService: expressionService
                    });

                    $scope.fieldsDatamap = $scope.safe($scope.safe($scope.datamap, $scope.fieldMetadata.attribute), "fields");

                    $scope.nodeDatamap = function (nodeMetadata) {
                        const treeDm = $scope.safe($scope.datamap, $scope.fieldMetadata.attribute);
                        const nodes = $scope.safe(treeDm, "nodes");
                        return $scope.safe(nodes, nodeMetadata.attribute);
                    }

                    const init = function () {
                        if (!$scope.fieldMetadata.fields || $scope.fieldMetadata.fields.length === 0) return;
                        fieldService.fillDefaultValues($scope.fieldMetadata.fields, $scope.fieldsDatamap, $scope);
                    }

                    init();
                }]

        };
        return directive;
    }]);
})(angular);