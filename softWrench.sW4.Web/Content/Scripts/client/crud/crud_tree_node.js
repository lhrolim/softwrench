(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("crudTreeNode", ["crudContextHolderService", "contextService", function (crudContextHolderService, contextService) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/crud/crud_tree_node.html"),
            scope: {
                fieldMetadata: "=",
                datamap: "=",
                schema: "=",
                listtype: "=",
                nodeindex: "=",
                startindex: "=",
                level: "@"
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

                    const calcIndex = function () {
                        if (!$scope.listtype) return "";
                        const index = 1 + $scope.nodeindex + ($scope.startindex ? Number.parseInt($scope.startindex) : 0);
                        if ($scope.listtype === "number") return index;
                        if ($scope.listtype === "letterdot") return String.fromCharCode(index + 96) + ".";
                        return "";
                    }

                    $scope.hasIndex = !!$scope.listtype;
                    $scope.index = calcIndex();
                    $scope.childrenLevel = Number.parseInt($scope.level) + 1;
                    $scope.indentationStyle = {
                        display: "inline-block",
                        "min-width": (Number.parseInt($scope.level * 20)) + "px"
                    };

                    $scope.fieldsDatamap = $scope.safe($scope.datamap, "fields");

                    $scope.nodeDatamap = function (nodeMetadata) {
                        const nodes = $scope.safe($scope.datamap, "nodes");
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