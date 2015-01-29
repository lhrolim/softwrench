function AceController($scope, $http, $templateCache, $window, i18NService) {

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