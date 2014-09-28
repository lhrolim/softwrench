function AboutController($scope, $http, $templateCache, i18NService) {

    var data = $scope.resultData;
    if (data != null) {
        $scope.aboutData = data;
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };
}