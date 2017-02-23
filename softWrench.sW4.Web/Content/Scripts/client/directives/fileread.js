app.directive("fileread", function (alertService, $log, contextService, submitService) {

    "ngInject";

    return {
        scope: {
            fileread: "=",
            path: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var log = $log.getInstance('fileread#change');
                log.debug('file change detected');
                if (!submitService.isValidAttachment(this.value)) {
                    if (!isIe9()) {
                        changeEvent.currentTarget.value = "";
                        scope.$apply(function () {
                            scope.fileread = undefined;
                            scope.path = undefined;
                        });
                    }

                    alert("Invalid file. Please choose another one.");
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