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

                scope.files = [];

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
                        "#newFile": true,
                        persisted: false,
                        value: value,
                        label: fileName,
                        extension: getExt(fileName)
                    }

                    scope.files.push(newFile);

                    const rel = scope.fieldMetadata.relationship;

                    if (!addFunction) {
                        scope.datamap[rel] = scope.datamap[rel] || [];
                        scope.datamap[rel].push(newFile);
                        scope.ignoreWatch = true;
                        return;
                    }

                    dispatcherService.invokeServiceByString(addFunction, [newFile, rel, scope.datamap]).catch(() => {
                        const index = scope.files.indexOf(newFile);
                        if (index !== -1) {
                            scope.files.splice(index, 1);
                        }
                        scope.fileData = null;
                        element.find(".upload-input").val(null);
                    });
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
                    if (!newValue || scope.ignoreWatch) {
                        scope.ignoreWatch = false;
                        return;
                    }
                    scope.files = [];
                    angular.forEach(newValue, (currentFile) => {
                        if (currentFile["#newFile"]) {
                            scope.files.push(currentFile);
                            return;
                        }
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