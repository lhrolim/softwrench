(function (angular) {
    "use strict";

var app = angular.module('sw_layout');

app.directive('fileUpload', function (contextService, $log, attachmentService) {
    "ngInject";

    var log = $log.getInstance('sw4.fileUpload');

    return {
        link: function (scope, element, attrs) {
            var fileInput = $('#uploadBtn', element);

            fileInput.on('change', function (e) {
                log.debug('upload');

                //display the file name
                var fileName = this.value.match(/[^\/\\]+$/);
                var isValid = attachmentService.isValid(this.value);

                if (!isValid) {
                    $('#uploadFile', element).attr('value', '');
                    $('#uploadFile', element).val('');
                    return;
                }

                //if multiple flies selected, update the label
                if (fileInput[0].files.length > 1) {
                    fileName = '{0} files selected'.format(fileInput[0].files.length);
                }

                $('#uploadFile', element).attr('value', fileName);

                //update the file input title
                var title = 'No file selected';
                if (fileInput != undefined && fileInput.val() != '') {
                    title = fileInput.val();
                }

                fileInput.attr('title', title);
            });
        }
    };
});

})(angular);