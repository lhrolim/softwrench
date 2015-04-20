

function AuditController($scope, $http, $templateCache, pwdenforceService, i18NService) {

    init($scope.resultData);

    function init(data) {
        if (data != null) {
            $scope.auditEntries = data.auditEntries;
            
        }
    }

}


