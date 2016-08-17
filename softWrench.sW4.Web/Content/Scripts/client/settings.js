(function (angular) {
    "use strict";

angular.module("sw_layout").controller("AceController", AceController);
function AceController($scope, $http, $templateCache, $window, i18NService, alertService, restService, contextService) {
    "ngInject";

    const EditMenu = "menu";
    const EditStatucColor = "statuscolors";
    const EditClassificationColor = "classificationcolors";
    const EditMetaData = "metadata";

    $scope.templates = [];
    $scope.selectedTemplate = '';

    $scope.loadTemplate = function () {
        $http.get(url("/api/generic/EntityMetadata/GetMetadataContent?templatePath=" + $scope.selectedTemplate.path))
            .success(function (data) {
                $scope.resultData = data.resultObject;
                init();
            })
            .error(function (data) {
                alertService.alert("Failure!");
            }); 
    };

    $scope.getTemplates = function () {
        $http.get(url("/api/generic/EntityMetadata/GetTemplateFiles"))
            .success(function (data) {
                $scope.templates = data;
                if ($scope.templates.length > 0) {
                    $scope.selectedTemplate = $scope.templates[0];
                }
            })
            .error(function (data) {
                alertService.alert("Failure!");
            });
    };

    $scope.save = function () {
        alertService.confirm("You will be logged out in order to implement this change.Do you want to continue? ").then(function () {
            $http({
                method: "PUT",
                url: url(resolveApiUrl($scope.type)),
                headers: { "Content-Type": "application/xml" },
                data: ace.edit("editor").getValue()
            }).success(function () {
                contextService.deleteFromContext("swGlobalRedirectURL");
                $window.location.href = url("/stub/reset");
            });
        });
    };

    $scope.savechanges = function () {
        if ($scope.comments && $scope.fullname) {
            alertService.confirm("Are you sure you want to save your changes? You will be logged out in order to implement this change. Do you want to continue?").then(function () {
                var httpParameters = {
                    Comments: $scope.comments,
                    Metadata: ace.edit("editor").getValue(),
                    Path: $scope.selectedTemplate.path,
                    Name: $scope.selectedTemplate.name,
                    UserFullName: $scope.fullname
                };

                var json = angular.toJson(httpParameters);

                $http({
                    method: "PUT",
                    dataType: "json",
                    url: url(resolveApiUrl($scope.type)),
                    headers: { "Content-Type": "application/json; charset=utf-8" },
                    data: json
                }).success(function () {
                    alertService.notifymessage("Metadata saved successfully");
                    contextService.deleteFromContext("swGlobalRedirectURL");
                    $window.location.href = url("/stub/reset");
                });
            });
        } else {
            alertService.alert("Please describe your changes in the comments section and enter your full name.");
        } 
    };
                   
    $scope.restore = function () {
        alertService.confirm("This will restore your XML to the default XML file, and none of your changes will be saved. Is this what you want to do? ").then(function () {
            var urlToCall = url("/api/generic/EntityMetadata/RestoreDefaultMetadata");
            $http.get(urlToCall).success(function (result) {
                var editor = ace.edit("editor");
                editor.getSession().setMode("ace/mode/xml");
                var data = $scope.resultData;
                $scope.type = data.type;
                editor.setValue(result.resultObject.content);
                editor.gotoLine(0);
                $scope.save();
                alertService.alert("Default xml file has been successfully restored");
            }).error(function (result) {
                alertService.alert("Failed to Load your xml file.Please try again later");
            });
        });
    };
       
    $scope.restorexml = function () {
        alertService.confirm("Select a Restore File from the table to restore your xml to selected file. None of your current changes will be saved. Is this what you want to do?").then(function () {
            var urlToCall = url("/api/generic/EntityMetadata/RestoreSavedMetadata?metadataFileName=" + $scope.selectedTemplate.name);
            $http.get(urlToCall).success(function (result) {
                $scope.results = result;
            }).error(function (result) {
                alertService.alert("Failed to Load your xml file.Please try again later");
            });
        });
    };

    $scope.edit = function (Metadata) {
        alertService.confirm("The selected xml file will overwrite the existing xml file.None of your current changes will be saved. Is this what you want to do?").then(function () {
            var editor = ace.edit("editor");
            editor.getSession().setMode("ace/mode/xml");
            var data = $scope.resultData;
            $scope.type = data.type;
            editor.setValue(Metadata);
            editor.gotoLine(0);
            $scope.save();
            alertService.alert("Your xml file has been successfully restored");
        });
    };

    $scope.contextPath = function (path) {
        return url(path);
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    function resolveApiUrl(type) {
        var urlToUse = "";

        switch (type) {
            case EditMenu:
                urlToUse = "/api/generic/EntityMetadata/SaveMenu";
                break;
            case EditStatucColor:
                urlToUse = "/api/generic/EntityMetadata/SaveStatuscolor";
                break;
            case EditClassificationColor:
                urlToUse = "/api/generic/EntityMetadata/SaveClassificationcolor";
                break;
            case EditMetaData:
                urlToUse = "/api/generic/EntityMetadata/SaveMetadataEditor";
                break;
            default:
                urlToUse = $scope.type;
                break;
        }

        return urlToUse;
    };

    function init() {
        var editor = ace.edit("editor");
        editor.getSession().setMode("ace/mode/xml");
        var data = $scope.resultData;
        $scope.type = data.type;

        editor.setValue(data.content);
        editor.gotoLine(0);

        if (EditMetaData.equalIc($scope.type)) {
            //Fetch the templates
            $scope.getTemplates();
        }
    };

    loadScript("/Content/customVendor/scripts/msic/ace.js", init());

    $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
        if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("EntityMetadataEditor.html") != -1) {
            init();
        }
    });
}

window.AceController = AceController;

})(angular);