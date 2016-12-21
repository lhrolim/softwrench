
(function () {
    'use strict';



    function filterModelService(searchService, userService) {

        /**
         * Returns the text to display on the filters given the choosen operator 
         * @param {} filter 
         * @param {} searchData 
         * @param {} operator 
         * @returns {} 
         */
        const getFilterText = function (filter, searchData, operator) {
            const attribute = filter.attribute;
            var searchText = searchData ? searchData[attribute] : '';
            var prefix = operator.title;

            if (operator.id === 'CUSTOM') {
                return "Custom";
            }

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
        };

        function doUpdateStore(application, schemaId, attribute, newValue) {
            const key = "filter:" + application + ":" + schemaId + ":" + attribute;
            localStorage[key] = JSON.stringify(newValue);
            return newValue;
        }


        function lookupRecentlyUsed(application, schemaId, attribute) {
            const key = "filter:" + application + ":" + schemaId + ":" + attribute;
            const stored = localStorage[key];
            if (!stored) {
                return [];
            }
            return JSON.parse(stored);
        }

        const deleteFromRecentlyUsed = function (schema, attribute, item) {
            const application = schema.applicationName;
            const schemaId = schema.schemaId;
            const stored = this.lookupRecentlyUsed(application, schemaId, attribute);
            var idx = -1;
            for (let i = 0; i < stored.length; i++) {
                if (stored[i].value === item.value) {
                    idx = i;
                    break;
                }
            }
            if (idx !== -1) {
                stored.splice(idx, 1);
            }
            return doUpdateStore(application, schemaId, attribute, stored);

        };
        const updateRecentlyUsed = function (schema, attribute, itemArray) {
            const application = schema.applicationName;
            const schemaId = schema.schemaId;
            var stored = this.lookupRecentlyUsed(application, schemaId, attribute);
            if (itemArray.length > 10) {
                stored = itemArray.slice(0, 9);
            } else {
                itemArray.forEach(function (item) {
                    if (item.nonstoreable) {
                        //avoid pushing nonstoreable items
                        return;
                    }

                    if (stored.some(function (el) {
                        //avoid duplications, 
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
        };
        const buildSelectedItemsArray = function (availableOptions, selectedOptions) {
            const result = [];
            for (let i = 0; i < availableOptions.length; i++) {
                const item = availableOptions[i];
                if (selectedOptions.hasOwnProperty(item.value) && selectedOptions[item.value] === 1) {
                    result.push(item);
                }
            }
            return result;
        };
        const buildSearchValueFromOptions = function (selectedItems) {
            if (selectedItems.length === 0) {
                return null;
            }
            var buffer = "";
            var hasBlank = false;
            var hasAnyNonBlank = false;
            for (let i = 0; i < selectedItems.length; i++) {
                const item = selectedItems[i];
                if (item.value === "nullor:") {
                    hasBlank = true;
                    continue;
                }
                hasAnyNonBlank = true;
                buffer += (userService.readProperty(item.value) + ",");
            }
            if (hasBlank) {
                //this will get evaluated on server side
                buffer = "nullor:" + buffer;
            }
            return hasAnyNonBlank ? buffer.substring(0, buffer.length - 1) : buffer;

        };
        const parseOptions = function(searchData) {
            const options = [];
            if (searchData.startsWith("nullor:")) {
                options.push("nullor:");
                searchData = searchData.substring(7);
            }
            return options.concat(searchData.split(","));
        };
        const service = {
            lookupRecentlyUsed,
            updateRecentlyUsed,
            deleteFromRecentlyUsed,
            buildSearchValueFromOptions,
            getFilterText,
            buildSelectedItemsArray,
            parseOptions
        };
        return service;
    }

    angular.module('sw_layout').service('filterModelService', ['searchService', 'userService', filterModelService]);
})();
