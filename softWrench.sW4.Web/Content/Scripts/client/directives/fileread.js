﻿(function (app) {
    "use strict";

app.directive("fileread", function ($log, alertService, attachmentService) {
    "ngInject";

    return {
        scope: {
            fileread: "=",
            path: "="
        },

        link: function (scope, element, attrs) {

            var validExtensions = !attrs.acceptedExtensions
                                ? null
                                : attrs.acceptedExtensions.split(",").map(function (e) {
                                    return e.trim();
                                });

            var readFiles = function (changeEvent, fileRead, reader, current) {
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

                var log = $log.getInstance('fileread#change');
                log.debug('file change detected');
                if (!attachmentService.isValid(this.value, validExtensions)) {
                    alert("Invalid file. Please choose another one.");
                    return;
                }

                if (isIe9() || changeEvent.target.files.length == 0) {
                    // if we´re on ie9 we need to submit the files using a form since it doesnt have the FileReader object present (HTML5+)
                    scope.fileread = "xxx";
                    scope.$digest();
                    return;
                }

                var fileName = [];
                var reader = new FileReader();

                //Getting the File extension.
                for (var i = 0; i < changeEvent.target.files.length; i++) {
                    var temp = changeEvent.target.files[i].name.split(".").pop().toLowerCase();
                    if (!attachmentService.isValid(temp, validExtensions)) {
                        changeEvent.currentTarget.value = "";
                        alertService.alert("Invalid file type. Allowed file types are:" + validExtensions.join(", "));
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

                readFiles(changeEvent, fileRead, reader, 0);
                for (var j = 0; j < changeEvent.target.files.length; j++) {
                    file = changeEvent.target.files[j];
                    fileName.push(file.name);
                }

                scope.path = fileName;
//                $("[data-upload=files]")[0].value = fileName;
                scope.$emit("sw.attachment.file.changed", fileName);
            });;
        }
    };
});

})(app);