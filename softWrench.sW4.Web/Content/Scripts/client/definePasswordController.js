(function (angular) {
    'use strict';

    function DefinePasswordController($scope, $http, spinService) {

        
        $scope.timezone = new Date().getTimezoneOffset();

        $scope.confirm = function () {
            spinService.start();
            var formToSubmit = $("#definepasswordform");
            formToSubmit.attr("action", url("/UserSetup/DoSetPassword"));
            formToSubmit.submit();
        }
    }

    DefinePasswordController.$inject = ['$scope', '$http', 'spinService'];

    angular.module('sw_layout').controller('DefinePasswordController', DefinePasswordController);



})(angular);
