(function (angular, $) {
    "use strict";

    function toshibaAttachmentService(alertService, i18NService) {
        //#region Public methods

        function selectAttachment(item) {
            // not from rest: call 'super'
            if (!item["#from_rest"]) {
                return this.__super__.selectAttachment(item);
            }
            // from rest: custom download controller
            const doclinksid = item["doclinksid"];
            const params = {
                doclinksid: doclinksid,
                document: item["#parsedurl"] || item["document"],
                weburl: item["weburl"]
            }
            const message = String.format(i18NService.get18nValue("download.error", "Error downloading file with doclinksid {0}. Please, Contact your Administrator"), doclinksid);
            return this.executeDownloadRequest("ToshibaRestAttachment", params, message);
        }

        //#endregion

        //#region Service Instance
        const service = {
            selectAttachment,
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("tgcs.attachmentService", ["alertService", "i18NService", toshibaAttachmentService]);

    //#endregion

})(angular, jQuery);