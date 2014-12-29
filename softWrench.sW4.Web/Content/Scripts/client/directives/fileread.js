﻿app.directive("fileread", function (alertService) {
    return {
        scope: {
            fileread: "=",
            path: "="
        },


        link: function(scope, element, attributes) {
            element.bind("change", function(changeEvent) {
                var validFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html", "png"];
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
                        var file;
                        var fileRead = [];
                        var current = 0;
                        var readLoad = function(flag, loadEvent) {
                            scope.$apply(function () {
                                if (flag == 1) {
                                    fileRead.push(loadEvent.target.result);
                                    readFiles();
                                }
                            });
                        };
                        var readFiles = function() {
                            if (isIe9() || changeEvent.target.files.length == 0) { // Using an opposite if to return when there aren't any files 
                                return;
                            }
                            var fileNew = changeEvent.target.files[current]; // get the first from queue and store in file
                            current++;
                            if (current == changeEvent.target.files.length + 1) {
                                scope.fileread = fileRead;
                                return;
                            }
                            reader.onloadend = function(loadEvent) { // when finished reading file, call recursive readFiles function
                                readLoad(flag, loadEvent);
                            };
                            reader.readAsDataURL(fileNew);
                        };
                        readFiles();
                        for (i = 0; i < changeEvent.target.files.length; i++) {
                            file = changeEvent.target.files[i];
                            fileName.push(file.name);
                        }

                    }
                };
                if (flag == 1) {
                    scope.path = fileName;
                    $('[data-upload=files]')[0].value = fileName;
                }
            });;
        }
    }; });
