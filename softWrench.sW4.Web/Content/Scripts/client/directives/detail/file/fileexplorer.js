(function (angular) {
    "use strict";

    angular.module("sw_components").directive("fileexplorer", ["contextService", "dispatcherService", "attachmentService", "fileService", "schemaService", "crudContextHolderService","alertService",
        function (contextService, dispatcherService, attachmentService, fileService, schemaService, crudContextHolderService, alertService) {
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

                scope.downloadAll = function ($event) {
                    $event.preventDefault();

                    //TODO: make it workpackage agnostic 
                    const ownerId = scope.datamap["#workorder_.workorderid"];//schemaService.getId(scope.datamap, crudContextHolderService.currentSchema());
                    const userId = scope.datamap["#workorder_.wonum"];//schemaService.getId(scope.datamap, crudContextHolderService.currentSchema());
                    const ownerTable = "workorder";// TODO: make it generic scope.datamap["class"];
                    const relationship = scope.fieldMetadata.relationship;

                    const parameters = {
                        ownerId,
                        userId,
                        ownerTable,
                        relationship
                    }

                    const controllerUrl = url(`/FileExplorer/DownloadAll?${$.param(parameters)}`);
                    return fileService.downloadPromise(controllerUrl).catch((errorMessage) => alertService.alert("error downloading file"));

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


                scope.clearFileExplorer = function (compositiondata) {
                    scope.files = [];
                }

                scope.$on(JavascriptEventConstants.NavigateRequestCrawlOcurred, scope.clearFileExplorer);


             


                scope.$watch("datamap[fieldMetadata.relationship]", (newValue) => {
                    if (!newValue) {
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