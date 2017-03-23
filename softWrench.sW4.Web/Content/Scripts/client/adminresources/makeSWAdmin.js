(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("MakeSWAdminController", MakeSWAdminController);
    function MakeSWAdminController($scope, $http, $timeout, redirectService, restService) {
        "ngInject";

        $scope.submit = function () {
            const parameters = {
                password: $scope.password,
                newmasterpassword: $scope.masterpassword
            };
            restService.postPromise("MakeSWAdmin","Submit",null,parameters).
            then(function (response) {
                const data = response.data;
                if (data.resultObject === true) {
                    $scope.msg = "";
                    $scope.msgsuccess = "Successfully Authorized";
                    $timeout(function () {
                        location.reload();
                    }, 1500);

                    redirectService.redirectToHome();
                } else {
                    $scope.msg = "Unauthorized";
                }
            }).catch(function (data) {
                $scope.msg = "Error";
            });
        };

        function init() {
            $scope.password = "";
            $scope.msg = "";
            $scope.msgsuccess = "";
            $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
                if (oldValue !== newValue && $scope.resultObject.redirectURL.indexOf("MakeSWAdmin.html") !== -1) {
                    init($scope.resultData);
                }
            });
        };

        init();
    }

    window.MakeSWAdminController = MakeSWAdminController;

})(angular);