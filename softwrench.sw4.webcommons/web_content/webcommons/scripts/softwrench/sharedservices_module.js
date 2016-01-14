(function (angular) {
    "use strict";

    var modules = {
        webcommons: angular.module('webcommons_services', []),
        maximoapplications: angular.module('maximo_applications', [])
    };
    window.modules = modules;

})(angular);
