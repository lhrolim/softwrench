function AceController($scope, $http, $templateCache, $window, i18NService, alertService, restService) {

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
       
        $http({
            method: "PUT",
            url: url(urlToUse),
            headers: { "Content-Type": "application/xml" },
            data: ace.edit("editor").getValue()
        })
            .success(function () {
                $window.location.href = url("/stub/reset");
            });

    };
    $scope.savechanges = function () {

        var urlToUse = "/api/generic/EntityMetadata/SaveMetadataEditor";
        var jsonString = JSON.stringify(ace.edit("editor").getValue());
        //$http.post(urlToUse,jsonString).success(function () {
        //     alertSdervice.alert("Metadata changes saved successfully");
        //});
        $http({
            method: "POST",
            dataType: "json",
            url: url(urlToUse),
            headers: { "Content-Type": "application/json; charset=utf-8" },
            data:  jsonString }).
            success(function () {
                alertService.alert("saved successfully");
            });
         
        };
       
        //restService.invokePost("EntityMetadata", "SaveMetadataEditor", jsonString, null, null,null);
    
    $scope.restore = function () {

        alertService.confirmMsg("Are you sure you want to restore to default settings ? ", function () {
            window.location.reload();
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