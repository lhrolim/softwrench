(function (angular) {
    "use strict";


    angular.module('webcommons_services').config(['$provide', function ($provide) {

        $provide.decorator('dynamicScriptsCacheService',  ["$delegate", "$injector", function ($delegate, $rootScope) {

            const _register = $delegate.registerScript;
            const _usecustom = $delegate.useCustomServiceIfPresent;

            $delegate.registerScript = function () {
                const args = [$provide, ...arguments];
                return _register.apply(this, args );
            };


            return $delegate;


        }]);

    }
    ]);


})(angular);