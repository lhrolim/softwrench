app.directive("fileread", function (alertService) {
    return {
        scope: {
            fileread: "=",
            path: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var validFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html"];
                var fileName;
                var flag = -1;
                if (isIe9()) {
                    //to bypass required validation --> real file data will be set using form submission
                    scope.fileread = "xxx";
                    return;
                }
                var reader = new FileReader();
                if (changeEvent.target.files.length > 0) {
                    //Getting the File extension.
                    var temp = changeEvent.target.files[0].name.split(".").pop().toLowerCase();
                    if (validFileTypes.indexOf(temp) == -1) {
                        changeEvent.currentTarget.value = "";
                        alertService.alert("Invalid file. Chose another please");
                        //Updating the model
                        scope.$apply(function () {
                            scope.fileread = undefined;
                            scope.path = undefined;
                        });
                        return;
                    }
                    flag = 1;
                    reader.onload = function (loadEvent) {
                        scope.$apply(function () {
                            if (flag == 1) {
                                scope.fileread = loadEvent.target.result;
                            }
                        });
                    };
                    var file = changeEvent.target.files[0];
                    fileName = file.name;
                    reader.readAsDataURL(file);
                }

                if (flag == 1) {
                    changeEvent.currentTarget.parentNode.parentNode.children[0].value = fileName;
                    scope.path = fileName;
                }
            });
        }
    };
});