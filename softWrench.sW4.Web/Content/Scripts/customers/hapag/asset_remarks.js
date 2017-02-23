function AssetRemarksController($scope, $http, i18NService, fieldService) {
    "ngInject";

    function init() {

    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.saveRemark = function () {
        var datamap = $scope.datamap;
        var id = fieldService.getId(datamap, $scope.schema);
        var parameters = {
            id: id,
            Remark: datamap.remarks
        }
        var urlToUse = url("api/data/operation/asset/SaveRemarks?platform=web&id=" + id);
        parameters = addSchemaDataToJson(parameters, $scope.schema, null);
        var json = angular.toJson(parameters);
        $http.post(urlToUse, json).success(function(result) {
            var a;
        });
    }

    init();

}