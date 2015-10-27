
(function () {
    'use strict';



    function filterModelService(searchService, userService) {



        function findMainFilterOperations(filter) {

        }

        var getFilterText = function (filter,searchData,operator) {
            var attribute = filter.attribute;
            var searchText = searchData[attribute];
            var prefix = operator.title;

            if (operator.id === 'Blank') {
                return "Is Blank";
            }

            if (operator.id.equalsAny("",'NF')) {
                prefix = '';
            }

            if (operator.id === 'EQ' && filter.type === "MetadataOptionFilter") {
                prefix = 'Any';
            }

            if (!searchText || !operator) {
                return '' || 'All';
            }

            if (searchText.length > 20) {
                searchText = searchText.substring(0, 20) + '...';
            }

            return prefix + '(' + searchText + ')';
        }


        function lookupRecentlyUsed(application, schemaId, attribute) {
            var key = "filter:" + application + ":" + schemaId + ":" + attribute;
            var stored = localStorage[key];
            if (!stored) {
                return [];
            }
            return JSON.parse(stored);
        }

        var updateRecentlyUsed = function (schema, attribute, item, removeItem) {
            var application = schema.applicationName;
            var schemaId = schema.schemaId;
            var key = "filter:" + application + ":" + schemaId + ":" + attribute;
            var stored = localStorage[key];
            if (stored == null) {
                stored = [];
            } else {
                stored = JSON.parse(stored);
            }
            if (!removeItem) {
                if (stored.length === 10) {
                    //implement a LRU strategy, where it grows until it reaches 10, than least recent gets removed
                    stored.pop();
                }
                stored.unshift(item);
            } else {
                //removing it from lru cache
                var idx = -1;
                for (var i = 0; i < stored.length; i++) {
                    if (stored[i].value === item.value) {
                        idx = i;
                        break;
                    }
                }
                if (idx !== -1) {
                    stored.splice(idx, 1);
                }
            }

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
            buildSearchValueFromOptions: buildSearchValueFromOptions,
            getFilterText: getFilterText
        };

        return service;
    }

    angular.module('sw_layout').factory('filterModelService', ['searchService', 'userService', filterModelService]);
})();
