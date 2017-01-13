
(function (angular) {
    'use strict';

    function multiassetlocciService() {

        //afterchange
        function afterChangeAsset(parameters) {

            if (parameters.fields['assetnum'] != null) {
                parameters.fields['location'] = parameters.fields['asset_.location'];
            }
        };

        //afterchange
        function afterChangeLocation(parameters) {

            if (parameters.fields['location'] != parameters.fields['asset_.location']) {
                parameters.fields['assetnum'] = null;
            }
        };


        const service = {
            afterChangeAsset,
            afterChangeLocation
        };

        return service;


    }

    angular.module('sw_layout').service('multiassetlocciService', ["$rootScope", "$log", multiassetlocciService]);

})(angular);
