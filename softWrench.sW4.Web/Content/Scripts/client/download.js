(function (angular) {
    "use strict";


angular.module("sw_layout").controller("DownloadController", DownloadController);
function DownloadController($scope, i18NService, fieldService, alertService) {
    "ngInject";

    $scope.download = function (controller, action, id, mode) {
        var controllerToUse = controller == undefined ? "Attachment" : controller;
        var actionToUse = controller == undefined ? "Download" : action;
        
        var parameters = {};
        parameters.id = id;
        parameters.mode = mode == undefined ? "http" : mode;
        parameters.parentId = fieldService.getId(this.parentdata.fields, this.parentschema);
        parameters.parentApplication = this.parentschema.applicationName;
        parameters.parentSchemaId = this.parentschema.schemaId;
        
        var rawUrl = url("/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));

        $.fileDownload(rawUrl, {

            failCallback: function (html, url) {
                alertService.alert(String.format(i18NService.get18nValue('download.error', 'Error downloading file with id {0}. Please, Contact your Administrator'), id));
            }
        });
    };
}

window.DownloadController = DownloadController;

})(angular);