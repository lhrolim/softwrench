
(function () {
    'use strict';

    var staticvalidFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html", "png", "lic"]

    angular.module('sw_layout').factory('attachmentService', ['contextService', 'fieldService', 'schemaService', 'alertService', 'i18NService', attachmentService]);

    function attachmentService(contextService, fieldService, schemaService, alertService, i18NService) {

        var service = {
            isValid: isValid,
            downloadFile:downloadFile
        };

        return service;

        function downloadFile(item,column,schema) {

            var parameters = {};
            var id = item['docinfoid'];
            parameters.id = id;
            parameters.mode = "http";
//            parameters.parentId = fieldService.getId(this.parentdata.fields, this.parentschema);
//            parameters.parentApplication = this.parentschema.applicationName;
//            parameters.parentSchemaId = this.parentschema.schemaId;

            var rawUrl = url("/Attachment/Download" + "?" + $.param(parameters));

            $.fileDownload(rawUrl, {

                failCallback: function (html, url) {
                    alertService.alert(String.format(i18NService.get18nValue('download.error', 'Error downloading file with id {0}. Please, Contact your Administrator'), id));
                }
            });
            return false;
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

    }
})();
