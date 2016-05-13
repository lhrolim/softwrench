(function (angular) {
    "use strict";

angular.module("sw_layout").controller("MakeSWAdminController", MakeSWAdminController);
function MakeSWAdminController($scope, $http, $timeout, redirectService) {
    "ngInject";

    $scope.submit = function () {
        var parameters = {
            password: $scope.password
        };
        var urlToInvoke = redirectService.getActionUrl('MakeSWAdmin', 'Submit', parameters);
        $http.get(urlToInvoke).
        success(function (data, status, headers, config) {
            if (data.resultObject == true) {
                $scope.msg = "";
                $scope.msgsuccess = "Successfully Authorized";
                $timeout(function () {
                    location.reload();
                }, 1500);

                redirectService.redirectToHome();
            } else {
                $scope.msg = "Unauthorized";
            }
        }).
        error(function (data, status, headers, config) {
            $scope.msg = "Error";
        });
    };

    function init() {
        $scope.password = "";
        $scope.msg = "";
        $scope.msgsuccess = "";
        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("MakeSWAdmin.html") != -1) {
                init($scope.resultData);
            }
        });
    };

    init();
}

window.MakeSWAdminController = MakeSWAdminController;

})(angular);