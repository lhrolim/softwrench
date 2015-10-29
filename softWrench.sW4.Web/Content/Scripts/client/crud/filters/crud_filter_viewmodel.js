
(function () {
    'use strict';



    function filterModelService(searchService, userService) {



        var getFilterText = function (filter, searchData, operator) {
            var attribute = filter.attribute;
            var searchText = searchData[attribute];
            var prefix = operator.title;

            if (operator.id === 'Blank') {
                return "Is Blank";
            }

            if (operator.id.equalsAny("", 'NF')) {
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

        function doUpdateStore(application, schemaId, attribute, newValue) {
            var key = "filter:" + application + ":" + schemaId + ":" + attribute;
            localStorage[key] = JSON.stringify(newValue);
            return newValue;
        }


        function lookupRecentlyUsed(application, schemaId, attribute) {
            var key = "filter:" + application + ":" + schemaId + ":" + attribute;
            var stored = localStorage[key];
            if (!stored) {
                return [];
            }
            return JSON.parse(stored);
        }

        var deleteFromRecentlyUsed = function (schema, attribute, item) {
            var application = schema.applicationName;
            var schemaId = schema.schemaId;
            var stored = this.lookupRecentlyUsed(application, schemaId, attribute);
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
            return doUpdateStore(application, schemaId, attribute, stored);

        }

        var updateRecentlyUsed = function (schema, attribute, itemArray) {
            var application = schema.applicationName;
            var schemaId = schema.schemaId;
            var stored = this.lookupRecentlyUsed(application, schemaId, attribute);
            if (itemArray.length > 10) {
                stored = itemArray.slice(0, 9);
            } else {
                itemArray.forEach(function (item) {
                    if (stored.some(function (el) {
                        //avoid duplications
                       return el.value === item.value;
                    })) {
                        return;
                    }

                    stored.unshift(item);
                    if (stored.length > 10) {
                        stored.pop();
                    }
                });
            }
            return doUpdateStore(application, schemaId, attribute, stored);
        }

        var buildSelectedItemsArray = function (availableOptions, selectedOptions) {
            var result = [];
            for (var i = 0; i < availableOptions.length; i++) {
                var item = availableOptions[i];
                if (selectedOptions.hasOwnProperty(item.value) && selectedOptions[item.value] === 1) {
                    result.push(item);
                }
            }
            return result;
        }

        var buildSearchValueFromOptions = function (selectedItems) {
            if (selectedItems.length === 0) {
                return null;
            }
            var buffer = "";
            for (var i = 0; i < selectedItems.length; i++) {
                var item = selectedItems[i];
                buffer += (userService.readProperty(item.value) + ",");
            }
            return buffer.substring(0, buffer.length - 1);

        };

        var service = {
            lookupRecentlyUsed: lookupRecentlyUsed,
            updateRecentlyUsed: updateRecentlyUsed,
            deleteFromRecentlyUsed: deleteFromRecentlyUsed,
            buildSearchValueFromOptions: buildSearchValueFromOptions,
            getFilterText: getFilterText,
            buildSelectedItemsArray: buildSelectedItemsArray
        };

        return service;
    }

    angular.module('sw_layout').factory('filterModelService', ['searchService', 'userService', filterModelService]);
})();
