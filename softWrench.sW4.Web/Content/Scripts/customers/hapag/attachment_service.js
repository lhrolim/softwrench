var app = angular.module('sw_layout');

app.factory('attachmentservice', function () {

    return {

        validateNewForm: function (schema, datamap) {
            var arr = [];
            if (isIe9()) {
                return arr;
            }
            var newAttachment = datamap['newattachment'];
            var newScreenshot = datamap['newscreenshot'];
            if (newScreenshot == "" && !newAttachment) {
                arr.push('Either an attachment or a screenshot should be selected');
            }
            return arr;

        }
    };


});