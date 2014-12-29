app.directive("fileread", function (alertService) {
    return {
        scope: {
            fileread: "=",
            path: "="
        },




        link: function (scope, element, attributes) {

            var validFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html", "png"];

            var readFiles = function (changeEvent,fileRead,reader,current) {
                if (isIe9() || changeEvent.target.files.length == 0) {
                    return;
                }
                var fileNew = changeEvent.target.files[current]; // get the first from queue and store in file
                current++;
                if (current == changeEvent.target.files.length + 1) {
                    scope.fileread = fileRead;
                    return;
                }
                reader.onloadend = function (loadEvent) { // when finished reading file, call recursive readFiles function
                    scope.$apply(function () {
                        fileRead.push(loadEvent.target.result);
                        readFiles(changeEvent, fileRead, reader, current);
                    });
                };
                reader.readAsDataURL(fileNew);
            };

            element.bind("change", function (changeEvent) {

                if (isIe9() || changeEvent.target.files.length == 0) {
                    // if we´re on ie9 we need to submit the files using a form since it doesnt have the FileReader object present (HTML5+)
                    return;
                }

                var fileName = [];
                var reader = new FileReader();

                //Getting the File extension.
                for (var i = 0; i < changeEvent.target.files.length; i++) {
                    var temp = changeEvent.target.files[i].name.split(".").pop().toLowerCase();
                    if (validFileTypes.indexOf(temp) == -1) {
                        changeEvent.currentTarget.value = "";
                        alertService.alert("Invalid file type. Allowed file types are:" + validFileTypes.join(', '));
                        //Updating the model
                        scope.$apply(function () {
                            scope.fileread = undefined;
                            scope.path = undefined;
                        });
                        return;
                    }
                }
                var file;
                var fileRead = [];
             
                readFiles(changeEvent, fileRead, reader,0);
                for (i = 0; i < changeEvent.target.files.length; i++) {
                    file = changeEvent.target.files[i];
                    fileName.push(file.name);
                }

                scope.path = fileName;
                $('[data-upload=files]')[0].value = fileName;
            });;
        }
    };
});
