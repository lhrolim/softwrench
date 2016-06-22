(function (angular) {
    "use strict";

    angular.module("softwrench").directive("richTextField", [function () {
        const directive = {
            restrict: "E",
            templateUrl: "Content/Mobile/templates/directives/richtextfield.html",
            replace: false,
            scope: {
                value: "=",
                readOnly: "=",
                maxLength: "=",
                isRequired: "="
            },

            controller: ["$scope", function ($scope) {
                $scope.config = {
                    options: {
                        toolbar: false,
                        statusbar: false,
                        debounce: true,
                        inline: false,
                        theme: "modern",
                        skin_url: "lightgray",
                        menubar: false,
                        readonly: $scope.readOnly,
                        // so it doesn't mess base64 images coming from the server
                        convert_urls: false,
                        urlconverter_callback: url => url 
                    }
                };
            }]

        };

        return directive;
    }]);

})(angular);