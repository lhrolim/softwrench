(function (angular) {
    "use strict";

    if (!window.modulesset) {
        //this is a workaround for the karma runner setup where these would be registered twice leading to all sort of issues.
        //TODO: Ideally, though we´d need to exclude this from the list of files using a ! pattern

        var modules = {
            webcommons: angular.module('webcommons_services', ["sw_rootcommons"]),
            maximoapplications: angular.module('maximo_applications', []),
            rootCommons: angular.module('sw_rootcommons', [])
        };
        window.modules = modules;
        window.modulesset = true;

    }

})(angular);
