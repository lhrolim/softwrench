app.directive("fileread", function (alertService) {
    return {
        scope: {
            fileread: "=",
            path: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var validFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html" , "png"];
                var fileName = [];
                var flag = -1;
                if (!isIe9()) {
                    var reader = new FileReader();
                    if (changeEvent.target.files.length > 0) {
                        //Getting the File extension.
                        for (i = 0; i < changeEvent.target.files.length; i++) {
                            var temp = changeEvent.target.files[i].name.split(".").pop().toLowerCase();
                            if (validFileTypes.indexOf(temp) == -1) {
                                changeEvent.currentTarget.value = "";
                                alertService.alert("Invalid file type. Allowed file types are:" + validFileTypes.join(', '));
                                //Updating the model
                                scope.$apply(function() {
                                    scope.fileread = undefined;
                                    scope.path = undefined;
                                });
                                return;
                            }
                        }
                        flag = 1;
                        reader.onload = function (loadEvent) {
                            scope.$apply(function () {
                                if (flag == 1) {
                                    scope.fileread = scope.fileread + "#$%^"+(loadEvent.target.result);
                                }
                            });
                        };
                        var file;
                        for (i = 0; i < changeEvent.target.files.length; i++) {
                            file = changeEvent.target.files[i];
                            fileName.push(file.name);
                            
                        }
                        reader.readAsDataURL(file);
                    }
                }
                if (flag == 1) {
                    for (i = 0; i < changeEvent.target.files.length; i++) {
                        changeEvent.currentTarget.parentNode.parentNode.children[i].value = fileName[i];
                    }
                    scope.path = fileName;
                }
            });
        }
    };
});