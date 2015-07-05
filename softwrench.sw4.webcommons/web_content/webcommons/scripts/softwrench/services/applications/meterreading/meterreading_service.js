
(function () {
    'use strict';

    angular.module('maximo_applications').factory('meterReadingService', ['applicationService', meterReadingService]);

    function meterReadingService(applicationService) {

        var service = {
            read: read
        };

        return service;

        function read(schema, datamap) {
            var crudData = { crud: datamap };
            return applicationService.invokeOperation("workorder", "readings", "EnterMeter", crudData);
        }

    }
})();
