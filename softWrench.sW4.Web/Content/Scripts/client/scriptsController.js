(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("ScriptsController", ScriptsController);
    ScriptsController.$inject = ["$scope", "restService"];
    function ScriptsController($scope, restService) {
        "ngInject";

        var codeEditor = null;

        $scope.evaluate = function () {
            const json = angular.toJson({
                script: $scope.script
            });
            restService.invokePost("Scripts", "Evaluate", null, json, (response) => {
                $scope.result = response.resultObject;
                codeEditor.focus();
            });
        }

        $scope.editorLoaded = function (editor) {
            codeEditor = editor;
            codeEditor.focus();
        }
    };
})(angular);