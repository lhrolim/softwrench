(function (angular) {
    "use strict";

angular.module("sw_layout").controller("AceController", AceController);
function AceController($scope, $http, $templateCache, $window, i18NService, alertService, restService, contextService) {
    "ngInject";

    $scope.save = function () {

        switch ($scope.type) {
            case 'menu':
                var urlToUse = "/api/generic/EntityMetadata/SaveMenu";
                break;
            case 'statuscolors':
                var urlToUse = "/api/generic/EntityMetadata/SaveStatuscolor";
                break;
            case 'metadata':
                var urlToUse = "/api/generic/EntityMetadata/SaveMetadata";
                break;
            default:
                var urlToUse = $scope.type;
                break;

        }
        alertService.confirmMsg("You will be logged out in order to implement this change.Do you want to continue? ", function () {
            $http({
                method: "PUT",
                url: url(urlToUse),
                headers: { "Content-Type": "application/xml" },
                data: ace.edit("editor").getValue()
            })
                .success(function () {
                    contextService.deleteFromContext("swGlobalRedirectURL");
                    $window.location.href = url("/stub/reset");
                });

        });
    };
    $scope.savechanges = function () {
        if ($scope.comments != undefined) {
            
            alertService.confirmMsg("Are you sure you want to Save your changes to the Metadata ? ", function () {
                
                    var httpParameters = {
                        Comments: $scope.comments,
                        Metadata: ace.edit("editor").getValue()
                    };

                    var urlToUse = "/api/generic/EntityMetadata/SaveMetadataEditor";

                    var json = angular.toJson(httpParameters);
                    $http({
                        method: "POST",
                        dataType: "json",
                        url: url(urlToUse),
                        headers: { "Content-Type": "application/json; charset=utf-8" },
                        data: json
                    }).
                        success(function () {
                            
                            $scope.save();
                            alertService.alert("Metadata saved successfully");
                           
                            
                        });
                
            });
            }
        else
            alertService.alert("Please describe your changes in the Comments section");
        };
       
            
    $scope.restore = function () {
        alertService.confirmMsg("This will restore your XML to the default XML file, and none of your changes will be saved. Is this what you want to do? ", function () {
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
            //alertService.confirmMsg("Are you sure you want to restore to default settings ? ", function () {
            //    var urlToUse = url("/api/generic/EntityMetadata/RestoreMetadataEditor");
            //    $http.get(urlToUse)
            //    window.location.reload();
            //});

        });
    };
       
    $scope.restorexml = function () {
        alertService.confirmMsg("Select a Restore File from the table to restore your xml to selected file. None of your current changes will be saved. Is this what you want to do? ", function () {
            var urlToCall = url("/api/generic/EntityMetadata/RestoreSavedMetadata");
            $http.get(urlToCall).success(function (result) {
                                $scope.results = result;
                
        
            }).error(function (result) {
                alertService.alert("Failed to Load your xml file.Please try again later");
            });
          

        });
    };
    $scope.edit = function (Metadata) {
        alertService.confirmMsg("The selected xml file will overwrite the existing xml file.None of your current changes will be saved. Is this what you want to do? ", function () {

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

    function init() {
        var editor = ace.edit("editor");
        editor.getSession().setMode("ace/mode/xml");
        var data = $scope.resultData;
        $scope.type = data.type;
       
        editor.setValue(data.content);
        editor.gotoLine(0);
    }

    loadScript("/Content/Scripts/vendor/ace/ace.js", init);

    $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
        if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("EntityMetadataEditor.html") != -1) {
            init();
        }
    });
}

window.AceController = AceController;

})(angular);