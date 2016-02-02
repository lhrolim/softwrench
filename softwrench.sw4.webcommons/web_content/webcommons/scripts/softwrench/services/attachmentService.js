(function (angular, $) {
    "use strict";

    var staticvalidFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html", "png", "lic"];

    angular.module("sw_layout").factory("attachmentService", ["$rootScope", "$q", "contextService", "fieldService", "schemaService", "alertService", "i18NService", "searchService", "tabsService", "redirectService", attachmentService]);

    function attachmentService($rootScope, $q, contextService, fieldService, schemaService, alertService, i18NService, searchService, tabsService, redirectService) {

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

        function createAttachmentFromFile(schema, file) {
            // find the attachment view and redirect to it
            var tabs = tabsService.tabsDisplayables(schema);
            if (!tabs) return;
            var attachmentTab = tabs.filter(function (tab) {
                return tab.tabId.startsWith("attachment");
            });
            if (!attachmentTab) return;
            redirectService.redirectToTab(attachmentTab[0].tabId)
                .then(function () {
                    var deferred = $q.defer();
                    var blob = file.getAsFile();
                    var reader = new FileReader();
                    reader.addEventListener("load", function (event) {
                        var url = event.target.result;
                        deferred.resolve({
                            file: url,
                            fileName: "attachment<timestamp>.<extension>",
                            size: "<size>"
                        });
                    });
                    reader.readAsDataURL(blob);
                    // TODO: set file into the creation form  
                    return deferred.promise;
                })
                .then(function(fileWrapper) {
                    $rootScope.$broadcast("sw.attachment.<descent_name>", fileWrapper);
                });
            
        }

    }

})(angular, jQuery);
