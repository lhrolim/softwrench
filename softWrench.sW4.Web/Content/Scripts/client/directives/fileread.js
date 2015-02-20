app.directive("fileread", function (alertService,$log,contextService) {
    return {
        scope: {
            fileread: "=",
            path: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var validFileTypes = contextService.fetchFromContext('allowedfiles', true);
                var log = $log.getInstance('fileread#change');
                log.debug('file change detected');
                var extensionIdx = this.value.lastIndexOf(".");
                var extension = this.value.substring(extensionIdx+1);
                if ($.inArray(extension, validFileTypes) == -1) {
                    alertService.alert("Invalid file. Please choose another one.");
                    return;
                }

                if (isIe9()) {
                    
                    //to bypass required validation --> real file data will be set using form submission
                    //ie9 does not have the FileReaderObject
                    scope.fileread = "xxx";
                } else {
                    var fileName;
                    var hasFiles = changeEvent.target.files.length > 0;
                    if (!hasFiles) {
                        return;
                    }
                    var file = changeEvent.target.files[0];
                    fileName = file.name;
                    changeEvent.currentTarget.parentNode.parentNode.children[0].value = fileName;
                    scope.path = fileName;

                    var reader = new FileReader();

                    //Getting the File extension.
                    var temp = changeEvent.target.files[0].name.split(".").pop().toLowerCase();
                    if (validFileTypes.indexOf(temp) == -1) {
                        changeEvent.currentTarget.value = "";
                        alert("Invalid file. Choose another please");
                        //Updating the model
                        scope.$apply(function () {
                            scope.fileread = undefined;
                            scope.path = undefined;
                        });
                        return;
                    }
                    reader.onload = function (loadEvent) {
                        scope.$apply(function () {
                            scope.fileread = loadEvent.target.result;
                        });
                    };
                    reader.readAsDataURL(file);
                }
                scope.$digest();

            });
        }
    };
});