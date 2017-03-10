(function (angular) {
    "use strict";

    var modules = {
        webcommons: angular.module('webcommons_services', ["sw_rootcommons"]),
        maximoapplications: angular.module('maximo_applications', []),
        rootCommons: angular.module('sw_rootcommons', [])
    };
    window.modules = modules;

})(angular);
