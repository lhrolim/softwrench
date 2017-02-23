var app = angular.module('sw_layout');

app.factory('attachmentservice', function (submitService) {
    "ngInject";
    return {

        validateNewForm: function (schema, datamap) {
            var arr = [];
            if (isIe9()) {
                var formId = submitService.getFormToSubmitIfHasAttachement(schema.displayables, datamap);
                if (formId == null) {
                    //this means we have nothing filled in the form (no attach, no screenshot)
                    arr.push('Either an attachment or a screenshot should be selected');
                }
                return arr;
            }
            var newAttachment = datamap['newattachment'];
            var newScreenshot = datamap['newscreenshot'];
            if ((!newScreenshot || newScreenshot == "") && !newAttachment) {
                arr.push('Either an attachment or a screenshot should be selected');
            }
            return arr;

        }
    };


});