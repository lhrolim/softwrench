(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("pdfController", pdfController);
    pdfController.$inject = ["$scope", "$attrs", "$timeout"];
    function pdfController($scope, $attrs, $timeout) {
        $scope.pdfUrl = $attrs["pdfurl"];
        $scope.pdfName = $attrs["pdfname"];

        $scope.download = function() {
            window.location = $scope.pdfUrl;
        }

        $timeout(function () {
            $(".pdf-toolbar [rel=tooltip]").tooltip({ container: "body", trigger: "hover" });
        }, 1000);
    }
})(angular);
