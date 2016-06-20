(function (angular, $) {
    "use strict";

    angular.module("softwrench").directive("singleFileInput", [function () {
        const directive = {
            restrict: "E",
            templateUrl: "Content/Mobile/templates/directives/singlefileinput.html",
            replace: false,
            scope: {
                datamap: "=",
                schema: "=",
                fieldName: "@"
                //field: "="
            },

            controller: ["$element", "$scope", "$timeout", "offlineSchemaService", "loadingService", function ($element, $scope, $timeout, offlineSchemaService, loadingService) {
                // can't get the field from the scope because it is a different cloned instance
                // have to get it through the schema
                $scope.model = { field: offlineSchemaService.getFieldByAttribute($scope.schema, $scope.fieldName) };

                const originalImagePreview = $scope.model.field.rendererParameters["showImagePreview"];

                const fileInput = $element[0].querySelector("input[type='file']");

                const changeListener = event => {
                    loadingService.showDefault();
                    const file = event.target.files[0];
                    const type = file.type;
                    const name = file.name;
                    const reader = new FileReader();
                    reader.onloadend = loadEvent =>
                        $timeout(() => {
                            $scope.datamap[$scope.fieldName] = loadEvent.target.result;
                            $scope.model.field.rendererParameters["showImagePreview"] = !!type && type.contains("image");
                            $scope.datamap["document"] = name;
                        }).finally(() => loadingService.hide());

                    reader.readAsDataURL(file);
                };

                $scope.uploadFile = () => {
                    $(fileInput).trigger("click");
                };

                angular.element(fileInput).on("change", changeListener);
                $scope.$on("$destroy", function () {
                    angular.element(fileInput).off("change", changeListener);
                    $scope.model.field.rendererParameters["showImagePreview"] = originalImagePreview;
                });
            }]

        };

        return directive;
    }]);

})(angular, jQuery);