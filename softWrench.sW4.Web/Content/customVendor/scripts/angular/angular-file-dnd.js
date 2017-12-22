(function (angular) {
    "use strict";

    angular.module("omr.angularFileDnD", []).directive("fileDropzone", ["attachmentService", "$rootScope", "$q", function (attachmentService, $rootScope, $q) {
        return {
            require: "^?form",
            restrict: "A",
            scope: {
                field: "=",
                file: "=",
                fileName: "=",
                multiple: "@",
                dropzoneHoverClass: "@"
            },
            link: function (scope, element, attrs, form) {
                var checkSize, getDataTransfer, isTypeValid, processDragOverOrEnter, validMimeTypes;
                getDataTransfer = function (event) {
                    var dataTransfer;
                    return dataTransfer = event.dataTransfer || event.originalEvent.dataTransfer;
                };
                processDragOverOrEnter = function (event) {
                    if (event) {
                        element.addClass(scope.dropzoneHoverClass);
                        if (event.preventDefault) {
                            event.preventDefault();
                        }
                        if (event.stopPropagation) {
                            return false;
                        }
                    }
                    getDataTransfer(event).effectAllowed = "copy";
                    return false;
                };

                validMimeTypes = (!attrs.fileDropzone || attrs.fileDropzone.trim() === "")
                    ? null
                    : attrs.fileDropzone.split(",").map(function (e) {
                        return e.trim();
                    });

                checkSize = function (size) {
                    var _ref;
                    if (((_ref = attrs.maxFileSize) === (void 0) || _ref === "") || (size / 1024) / 1024 < attrs.maxFileSize) {
                        return true;
                    } else {
                        alert("File must be smaller than " + attrs.maxFileSize + " MB");
                        return false;
                    }
                };
                isTypeValid = function (type) {
                    if (attachmentService.isValid(type, validMimeTypes)) {
                        return true;
                    } else {
                        alert("Invalid file type.  File must be one of following types " + validMimeTypes.join(","));
                        return false;
                    }
                };
                element.bind("dragover", processDragOverOrEnter);
                element.bind("dragenter", processDragOverOrEnter);
                element.bind("dragleave", function () {
                    return element.removeClass(scope.dropzoneHoverClass);
                });

                const processSingle = function (event) {
                    var file, name, reader, size, type;
                    scope.field.rendererParameters["showImagePreview"] = false;
                    reader = new FileReader();
                    reader.onload = function (evt) {
                        if (!checkSize(size) || !isTypeValid(name.split(".").pop())) return;
                        scope.$apply(function () {
                            scope.file = evt.target.result;
                            scope.fileName = name;
                            scope.field.rendererParameters["showImagePreview"] = !!type && type.startsWith("image");
                        });
                        if (form) {
                            form.$setDirty();
                        }
                        $rootScope.$broadcast("file-dropzone-drop-event", {
                            file: scope.file,
                            type: type,
                            name: name,
                            size: size,
                            field: scope.field.attribute
                        });
                    };
                    file = getDataTransfer(event).files[0];
                    name = file.name;
                    type = file.type;
                    size = file.size;
                    reader.readAsDataURL(file);
                    return false;
                }

                const innerProcessMultiple = function (fileMetadata) {
                    const deferred = $q.defer();
                    ((innerDeferred) => {
                        const reader = new FileReader();
                        reader.onload = (evt) => {
                            if (!checkSize(fileMetadata.size) || !isTypeValid(fileMetadata.name.split(".").pop())) innerDeferred.reject();
                            fileMetadata.data = evt.target.result;
                            innerDeferred.resolve(fileMetadata);
                        };
                        reader.readAsDataURL(fileMetadata);
                    })(deferred);
                    return deferred.promise;
                }

                const processMultiple = function (event) {
                    const fileMetadatas = getDataTransfer(event).files;
                    $q.all(Array.from(fileMetadatas).map(innerProcessMultiple)).then(files => {
                        scope.file = files.map(file => file.data);
                        scope.fileName = files.map(file => file.name);
                        if (form) {
                            form.$setDirty();
                        }
                    });
                    return false;
                }

                return element.bind("drop", function (event) {
                    if (event != null) {
                        event.preventDefault();
                    }
                    element.removeClass(scope.dropzoneHoverClass);
                    scope.multiple === "true" ? processMultiple(event) : processSingle(event);
                });
            }
        };
    }]);

})(angular);