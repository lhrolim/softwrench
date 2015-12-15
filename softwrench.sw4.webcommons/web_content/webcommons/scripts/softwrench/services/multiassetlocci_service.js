
(function (angular) {
    'use strict';

    function multiassetlocciService() {


        function afterChangeAsset(parameters) {

            if (parameters.fields['assetnum'] != null) {
                parameters.fields['location'] = parameters.fields['asset_.location'];
            }
        };

        function afterChangeLocation(parameters) {

            if (parameters.fields['location'] != parameters.fields['asset_.location']) {
                parameters.fields['assetnum'] = null;
            }
        };


        var service = {
            afterChangeAsset: afterChangeAsset,
            afterChangeLocation: afterChangeLocation,
            afterChangeSequence: afterChangeSequence,
            afterChangeProgress: afterChangeProgress
        };

        return service;


    }

    angular.module('sw_layout').factory('multiassetlocciService', ["$rootScope", "$log", multiassetlocciService]);

})(angular);
