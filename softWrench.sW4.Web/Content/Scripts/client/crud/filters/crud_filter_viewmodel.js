
(function () {
    'use strict';



    function filterModelService(searchService,userService) {

     

        function findMainFilterOperations(filter) {

        }

        function lookupRecentlyUsed(application,schemaId, attribute) {
            var key = "filter:" + application + ":" + schemaId + ":" + attribute;
            var stored = localStorage[key];
            return stored || [];
        }

        var buildSearchValueFromOptions = function(selectedOptions) {
            var any = false;
            var buffer = "";
            for (var option in selectedOptions) {
                if (!selectedOptions.hasOwnProperty(option)) {
                    continue;
                }
                if (selectedOptions[option] === 1) {
                    any = true;
                    buffer += (userService.readProperty(option) + ",");
                }
            }
            if (any) {
                return buffer.substring(0, buffer.length - 1);
            }
            return null;
        };

        var service = {
            findMainFilterOperations: findMainFilterOperations,
            lookupRecentlyUsed: lookupRecentlyUsed,
            buildSearchValueFromOptions: buildSearchValueFromOptions
        };

        return service;
    }

    angular.module('sw_layout').factory('filterModelService', ['searchService', 'userService', filterModelService]);
})();
