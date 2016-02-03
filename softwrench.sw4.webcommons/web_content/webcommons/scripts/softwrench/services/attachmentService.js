(function (angular, $) {
    "use strict";

    var staticvalidFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html", "png", "lic"];

    angular.module("sw_layout").factory("attachmentService", ["$rootScope", "$q", "$timeout", "contextService", "fieldService", "schemaService", "alertService", "i18NService", "searchService", "tabsService", "redirectService", attachmentService]);

    function attachmentService($rootScope, $q, $timeout, contextService, fieldService, schemaService, alertService, i18NService, searchService, tabsService, redirectService) {

        var service = {
            isValid: isValid,
            downloadFile: downloadFile,
            selectAttachment: selectAttachment,
            createAttachmentFromFile: createAttachmentFromFile
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

        /**
         * Resolves a file wrapper instance from a DataTransferItem.
         * If options.redirect is `true`: will redirect the view to a visible attachment tab
         * If options.event is `true`: will fire "sw.attachment.file.load" event with the file wrapper as argument 
         * 
         * @param DataTransferItem|Blob|File file
         * @param {} [schema] attachment's parent schema
         * @param {} options: { 'redirect': Boolean, 'event': Boolean } 
         * @returns Promise resolved with a file wrapper dto instance: 
         *          {
         *               file: String, // content
         *               name: String, // file's name
         *               type: String, // mime type
         *               size: Number // size in bytes
         *           } 
         */
        function createAttachmentFromFile(file, schema, options) {
            options = options || {};

            var promise = !!options.redirect ? redirectToAttachmentView(schema) : $q.when();

            // file data has to be querried before returning the promise
            // because it gets disposed (no content and empty properties) after the function returns
            var blob = file instanceof DataTransferItem ? file.getAsFile() : file;
            var extension = file.type.split("/")[1];
            var fileName = !!blob.name ? blob.name : "attachment{0}.{1}".format(Date.now().getTime(), extension);

            promise = promise.then(function () {
                return readFile(blob);
            })
            .then(function (content) {
                return {
                    file: content,
                    name: fileName,
                    type: file.type,
                    size: blob.size
                };
            });

            return !options.event
                ? promise
                : promise.then(function (fileWrapper) {

                    var timer = $timeout(function () {
                        $rootScope.$broadcast("sw.attachment.file.load", fileWrapper);
                    }, 0, false);

                    return timer.then(function () {
                        return fileWrapper;
                    });

                });
        }

    }

})(angular, jQuery);
