(function (angular) {
    "use strict";

    angular.module("sw_components").directive("fileexplorer", ["contextService", "dispatcherService", "attachmentService", function (contextService, dispatcherService, attachmentService) {
        const directive = {

            restrict: "E",
            scope: {
                fieldMetadata: '=',
                datamap: '='
            },
            templateUrl: contextService.getResourceUrl("/Content/Templates/directives/file/fileexplorer.html"),

            controller: ["$scope", function ($scope) {
                // workaround
                if ($scope.fieldMetadata && $scope.fieldMetadata.schema && !$scope.fieldMetadata.rendererParameters) {
                    $scope.fieldMetadata.rendererParameters = $scope.fieldMetadata.schema.rendererParameters;
                }
            }],

            link: function (scope, element, attrs) {
                const addFunction = scope.fieldMetadata.rendererParameters && scope.fieldMetadata.rendererParameters["fileexplorer.addfunction"];

                scope.browseFile = function ($event) {
                    $event.preventDefault();
                    element.find(".upload-input").trigger("click");
                };

                function getExt(fileName) {
                    const idx = fileName.indexOf(".");
                    return idx !== -1 ? fileName.substring(idx + 1) : undefined;
                }

                function processFile(fileName, value) {
                    const exists = scope.files.some((file) => {
                        return fileName === file.label;
                    });
                    if (exists) {
                        return;
                    }

                    const newFile = {
                        persisted: false,
                        value: value,
                        label: fileName,
                        extension: getExt(fileName)
                    }

                    scope.files.push(newFile);
                    if (addFunction) {
                        dispatcherService.invokeServiceByString(addFunction, [newFile, scope.fieldMetadata.relationship]).catch(() => {
                            const index = scope.files.indexOf(newFile);
                            if (index !== -1) {
                                scope.files.splice(index, 1);
                            }
                            scope.fileData = null;
                            element.find(".upload-input").val(null);
                        });
                    }
                }

                scope.fileClick = function (file) {
                    if (file["docinfo_.urltype"]) {
                        attachmentService.selectAttachment(file);
                    }
                }

                //scope.removeClick = function(file) {
                //}

                scope.$watch("fileData", (newValue) => {
                    if (!newValue) {
                        return;
                    }
                    if (Array.isArray(newValue)) {
                        angular.forEach(newValue, (value, index) => {
                            processFile(scope.fileNames[index], value);
                        });
                    } else {
                        processFile(scope.fileName, newValue);
                    }
                });

                scope.$watch("datamap[fieldMetadata.relationship]", (newValue) => {
                    scope.files = [];
                    if (!newValue) {
                        return;
                    }
                    angular.forEach(newValue, (currentFile) => {
                        scope.files.push({
                            persisted: true,
                            label: currentFile["docinfo_.description"],
                            extension: getExt(currentFile["docinfo_.description"]),
                            "docinfo_.urltype": currentFile["docinfo_.urltype"],
                            docinfoid: currentFile["docinfoid"]
                        });
                    });
                });
            }
        };

        return directive;
    }]);

})(angular);