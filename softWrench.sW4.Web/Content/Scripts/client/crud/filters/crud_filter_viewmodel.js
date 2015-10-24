
(function () {
    'use strict';



    function filterModelService(searchService, userService) {



        function findMainFilterOperations(filter) {

        }

        function lookupRecentlyUsed(application, schemaId, attribute) {
            var key = "filter:" + application + ":" + schemaId + ":" + attribute;
            var stored = localStorage[key];
            if (!stored) {
                return [];
            }
            return JSON.parse(stored);
        }

        var updateRecentlyUsed = function (schema, attribute, item) {
            var application = schema.applicationName;
            var schemaId = schema.schemaId;
            var key = "filter:" + application + ":" + schemaId + ":" + attribute;
            var stored = localStorage[key];
            if (stored == null) {
                stored = [];
            } else {
                stored = JSON.parse(stored);
            }
            if (stored.length === 10) {
                //implement a LRU strategy, where it grows until it reaches 10, than least recent gets removed
                stored.pop();
            }
            stored.unshift(item);
            localStorage[key] = JSON.stringify(stored);
        }

        var buildSearchValueFromOptions = function (selectedOptions) {
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
            updateRecentlyUsed: updateRecentlyUsed,
            buildSearchValueFromOptions: buildSearchValueFromOptions
        };

        return service;
    }

    angular.module('sw_layout').factory('filterModelService', ['searchService', 'userService', filterModelService]);
})();
