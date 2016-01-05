//took from https://raw.githubusercontent.com/callmehiphop/angular-namespacer/master/angular-namespacer.js
(function (angular) {
    'use strict';

    var angularModule = angular.bind(angular, angular.module);

    /**
     * Decorates angular module instance to allow for namespaces
     */
    angular.module = function (moduleName, requires, configFn) {
        var instance = angularModule(moduleName, requires, configFn);

        instance.clientfactory = function (serviceName, fn) {
            return app.factory(moduleName + "." + serviceName, fn);
        };

        return instance;
    };

}(angular));