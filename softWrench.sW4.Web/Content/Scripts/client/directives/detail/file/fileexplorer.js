(function (angular) {
    "use strict";

    angular.module("sw_components").directive("fileexplorer", ["contextService", "dispatcherService", "attachmentService", "fileService", "schemaService", "crudContextHolderService", "alertService","$timeout",

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

                        return dispatcherService.invokeServiceByString(addFunction, [newFile, rel, scope.datamap]).then(r=> {
                        }).catch(() => {
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

                    scope.deleteFile = function ($event, item) {
                        $event.stopImmediatePropagation();
                        //TODO: this was copied from compositionlist.js --> create some sort of inheritance between them
                        return alertService.confirm("Are you sure you want to delete this file").then(() => {

                            const deletefunction = this.fieldMetadata.schema.rendererParameters["deletefunction"];
                            if (!!deletefunction) {
                                return dispatcherService.invokeServiceByString(deletefunction, [item]).then(shouldRefresh => {
                                    if (shouldRefresh) {
                                        const idx = scope.files.indexOf(item);
                                        scope.files.splice(idx, 1);
                                    }
                                });
                            }
                            throw new Error("not yet implemented");
                        });

                    }


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


                    scope.refresh = function (newValue){
                        scope.files = [];
                        if (!newValue) {
                            const datamap = crudContextHolderService.rootDataMap();
                            newValue = datamap[scope.fieldMetadata.relationship];
                        }

                        angular.forEach(newValue, (currentFile) => {
                            if (currentFile["#newFile"]) {
                                scope.files.push(currentFile);
                                return;
                            }
                            if (currentFile.deleted) {
                                return;
                            }

                            scope.files.push({
                                persisted: true,
                                label: currentFile["docinfo_.description"],
                                extension: getExt(currentFile["docinfo_.description"]),
                                "docinfo_.urltype": currentFile["docinfo_.urltype"],
                                docinfoid: currentFile["docinfoid"],
                                doclinksid: currentFile["doclinksid"]
                            });
                        });
                    }


                    scope.$watch("datamap[fieldMetadata.relationship]", (newValue) => {
                        if (!newValue) {
                            return;
                        }
                        scope.refresh(newValue);
                    });
                }
            };

            return directive;
        }]);

})(angular);