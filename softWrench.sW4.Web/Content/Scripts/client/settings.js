function AceController($scope, $http, $templateCache, $window, i18NService) {
    "ngInject";
    $scope.save = function () {
        var urlToUse = $scope.type == 'menu' ? "/api/generic/EntityMetadata/SaveMenu" : "/api/generic/EntityMetadata/SaveMetadata";
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

    loadScript("/Content/Scripts/ace/ace.js", init);

    $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
        if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("EntityMetadataEditor.html") != -1) {
            init();
        }
    });
}