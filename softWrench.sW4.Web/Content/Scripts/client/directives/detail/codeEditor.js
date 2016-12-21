(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("codeEditor", ["contextService", function (contextService) {
        return {
            restrict: "E",
            replace: true,
            templateUrl: contextService.getResourceUrl("/Content/Templates/directives/codeEditor.html"),
            scope: {
                mode: "@",
                readonly: "@",
                value: "=",
                onload: "&"
            },
            link: function (scope, element) {
                const resized = window.debounce(() => scope.editor.resize(), 500);
                addResizeListener(element[0], () => resized());
            },
            controller: ["$scope", function ($scope) {
                $scope.editor = null;
                $scope.readonly = $scope.readonly === "true";

                $scope.aceLoaded = function (aceEditor) {
                    $scope.editor = aceEditor;
                    $scope.onload({ editor: aceEditor });
                };

                $scope.config = function() {
                    const config = {
                        readOnly: $scope.readonly,
                        showPrintMargin: false,
                        onLoad: $scope.aceLoaded
                    }
                    if ($scope.mode) {
                        config.mode = $scope.mode;
                    }
                    return config;
                }
            }]
        };
    }]);
})(angular);