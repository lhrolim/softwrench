
(function () {
    'use strict';

    angular.module('sw_layout').factory('multiassetlocciService', ["$rootScope", "$log", multiassetlocciService]);

    function multiassetlocciService($rootScope, $log) {

        var service = {
            afterChangeAsset: afterChangeAsset,
            afterChangeLocation: afterChangeLocation,
            afterChangeSequence: afterChangeSequence
        };

        return service;

        function afterChangeAsset(parameters) {
            parameters.fields['#isDirty'] = true;
            parameters.fields['location'] = parameters.fields['asset_.location'];
        };

        function afterChangeLocation(parameters) {
            parameters.fields['#isDirty'] = true;
        };

        function afterChangeSequence(parameters) {
            parameters.fields['#isDirty'] = true;
        };
    }
})();
