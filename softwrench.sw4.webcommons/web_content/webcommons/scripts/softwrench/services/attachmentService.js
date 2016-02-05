(function (angular, $) {
    "use strict";

    var staticvalidFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html", "png", "lic"];

    angular.module("sw_layout").factory("attachmentService", ["$rootScope", "$q", "$timeout", "contextService", "fieldService", "schemaService", "alertService", "i18NService", "searchService", "tabsService", "redirectService", attachmentService]);

    function attachmentService($rootScope, $q, $timeout, contextService, fieldService, schemaService, alertService, i18NService, searchService, tabsService, redirectService) {

        var service = {
            isValid: isValid,
            downloadFile: downloadFile,
            selectAttachment: selectAttachment,
            redirectToAttachmentView: redirectToAttachmentView,
            createAttachmentFromFile: createAttachmentFromFile,
            createAttachmentFromElement: createAttachmentFromElement
        };

        return service;

        function downloadFile(item, column, schema) {

            var parameters = {};
            var id = item['docinfoid'];
            parameters.id = id;
            parameters.mode = "http";

            var rawUrl = url("/Attachment/Download" + "?" + $.param(parameters));

            $.fileDownload(rawUrl, {

                failCallback: function (html, url) {
                    alertService.alert(String.format(i18NService.get18nValue("download.error", "Error downloading file with id {0}. Please, Contact your Administrator"), id));
                }
            });
            return false;
        }

        function getUrl(item, column, schema) {
            var searchData = {};
            searchData.docinfoid = item.docinfoid.toString();
            searchService.searchWithData("attachment", searchData).success(function (data) {
                var resultObject = data.resultObject;
                var resultFields = resultObject[0].fields;
                var resultUrl = resultFields['docinfo_.urlname'];
                window.open(resultUrl);
            }).error(function () {
                return null;
            });
        }

        function isValid(value) {
            if (value == null) {
                return false;
            }

            var fileName = value.match(/[^\/\\]+$/);
            var validFileTypes = contextService.fetchFromContext("allowedfiles", true);
            if (!validFileTypes) {
                validFileTypes = staticvalidFileTypes;
            }
            var extensionIdx = value.lastIndexOf(".");
            var extension = value.substring(extensionIdx + 1).toLowerCase();
            return $.inArray(extension, validFileTypes) !== -1;
        }

        function selectAttachment(item, column, schema) {
            if (item["docinfo_.urltype"] === "URL") {
                getUrl(item, column, schema);
            } else {
                downloadFile(item, column, schema);
            }
            return false;
        }

        function redirectToAttachmentView(schema) {
            // find the attachment view and redirect to i
            var tabs = tabsService.tabsDisplayables(schema);
            if (!tabs) {
                return $q.reject(new Error("no displayable tabs for schema {0}.{1}".format(schema.applicationName, schema.schemaId)));
            }
            var attachmentTab = tabs.filter(function (tab) {
                return tab.tabId.startsWith("attachment");
            });
            if (!attachmentTab) {
                return $q.reject(new Error("no attachment tab for schema {0}.{1}".format(schema.applicationName, schema.schemaId)));
            }
            // redirect to view
            return redirectService.redirectToTab(attachmentTab[0].tabId);
        }

        function readFile(file) {
            var deferred = $q.defer();
            var reader = new FileReader();

            reader.addEventListener("loadend", function (event) {
                deferred.resolve(reader.result);
            });
            reader.addEventListener("error", function (error) {
                deferred.reject(error);
            });

            reader.readAsDataURL(file);
            return deferred.promise;
        }

        function base64ToBlob(b64Data, contentType, sliceSize) {
            contentType = contentType || "";
            sliceSize = sliceSize || 512;

            var byteCharacters = atob(b64Data);
            var byteArrays = [];

            for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
                var slice = byteCharacters.slice(offset, offset + sliceSize);

                var byteNumbers = new Array(slice.length);
                for (var i = 0; i < slice.length; i++) {
                    byteNumbers[i] = slice.charCodeAt(i);
                }

                var byteArray = new Uint8Array(byteNumbers);

                byteArrays.push(byteArray);
            }

            var blob = new Blob(byteArrays, { type: contentType });
            return blob;
        }

        function newAttachmentFileName(extension) {
            return "attachment{0}.{1}".format(Date.now().getTime(), extension);
        }

        function broadCastAttachmentLoaded(file) {
            var timer = $timeout(function () {
                $rootScope.$broadcast("sw.attachment.file.load", file);
            }, 0, false);

            return timer.then(function () {
                return file;
            });
        }

        /**
         * Resolves a file wrapper instance from a DataTransferItem|File|Blob.
         * - will redirect the view to a visible attachment tab
         * - will fire "sw.attachment.file.load" event with the file wrapper as argument 
         * 
         * @param DataTransferItem|Blob|File file
         * @param {} schema attachment's parent schema
         * @returns Promise resolved with a file wrapper dto instance: 
         *          {
         *               file: String, // content
         *               name: String, // file's name
         *               type: String, // mime type
         *               size: Number // size in bytes
         *           } 
         */
        function createAttachmentFromFile(file, schema) {
            // file data has to be querried before returning the promise (can't be done inside the callbacks)
            // because the file gets disposed (no content and empty properties) after the function returns
            var blob = (file instanceof Blob || file instanceof File) ? file : file.getAsFile();
            var extension = file.type.split("/")[1];
            var fileName = !!blob.name ? blob.name : newAttachmentFileName(extension);

            return redirectToAttachmentView(schema)
                .then(function () {
                    return readFile(blob);
                })
                .then(function (content) {
                    return {
                        file: content,
                        name: fileName,
                        type: file.type,
                        size: blob.size
                    };
                })
                .then(broadCastAttachmentLoaded);
        }

        /**
         * Resolves a file wrapper instance from an HTMLNode that will receive a pasted image (FF trick).
         * - will redirect the view to a visible attachment tab
         * - will fire "sw.attachment.file.load" event with the file wrapper as argument
         * 
         * @param HTMLNode contentHolder DOM node with `contenteditable='true'` that will receive the pasted content as an image Node
         * @param {} schema attachment's parent schema
         * @returns Promise resolved with a file wrapper dto instance: 
         *          {
         *               file: String, // content
         *               name: String, // file's name
         *               type: String, // mime type
         *           }  
         */
        function createAttachmentFromElement(contentHolder, schema) {

            var promise = null;

            var waitForImage = function () {
                // polling the Node to see if the image node was appended
                if (!contentHolder.childNodes || contentHolder.childNodes.length <= 0) {
                    promise = $timeout(waitForImage, 20, false);
                    return promise;
                }
                // get the image as base64 encoded from child image Node
                var child = contentHolder.childNodes[0];
                contentHolder.innerHTML = "";
                if (!child) return $q.reject(new Error("image was not pasted"));
                if (child.tagName !== "IMG") return $q.reject(new Error("can only support images"));
                return child.src;
            };
            // begin polling for image
            promise = $timeout(waitForImage, 0, false);

            // use timer promise prior to the redirect so the image validation can happen before trying to redirect
            return promise.then(function (content) {
                // redirect then chain and resolve with the file
                return redirectToAttachmentView(schema).then(function () {
                    return {
                        file: content,
                        name: newAttachmentFileName("png"),
                        type: "image/png"
                    }
                });
            })
            .then(broadCastAttachmentLoaded);
        }

    }

})(angular, jQuery);
