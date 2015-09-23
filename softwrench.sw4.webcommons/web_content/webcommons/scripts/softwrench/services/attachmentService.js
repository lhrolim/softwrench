
(function () {
    'use strict';

    var staticvalidFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html", "png", "lic"]

    angular.module('sw_layout').factory('attachmentService', ['contextService', 'fieldService', 'schemaService', 'alertService', 'i18NService', 'searchService', attachmentService]);

    function attachmentService(contextService, fieldService, schemaService, alertService, i18NService, searchService) {

        var service = {
            isValid: isValid,
            downloadFile: downloadFile,
            selectAttachment: selectAttachment
        };

        return service;

        function downloadFile(item,column,schema) {

            var parameters = {};
            var id = item['docinfoid'];
            parameters.id = id;
            parameters.mode = "http";

            var rawUrl = url("/Attachment/Download" + "?" + $.param(parameters));

            $.fileDownload(rawUrl, {

                failCallback: function (html, url) {
                    alertService.alert(String.format(i18NService.get18nValue('download.error', 'Error downloading file with id {0}. Please, Contact your Administrator'), id));
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
            var validFileTypes = contextService.fetchFromContext('allowedfiles', true);
            if (!validFileTypes) {
                validFileTypes = staticvalidFileTypes;
            }
            var extensionIdx = value.lastIndexOf(".");
            var extension = value.substring(extensionIdx + 1).toLowerCase();
            return $.inArray(extension, validFileTypes) !== -1;
        }

        function selectAttachment(item, column, schema) {
            if (item.doctype === "Websites") {
                getUrl(item, column, schema);
            } else {
                downloadFile(item, column, schema);
            }
            return false;
        }

    }
})();
