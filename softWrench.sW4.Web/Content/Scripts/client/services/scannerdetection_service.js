var app = angular.module('sw_layout');

app.factory('scannerdetectionService', function ($http, $rootScope, restService, searchService, contextService) {

    return {
        initInventoryGridListener: function (schema, datamap, parameters) {
            var searchData = parameters.searchData;
            
            $(document).scannerDetection(function (data) {
                var scanOrderString = contextService.getFromContext(schema.properties['config.fullKey'], false, $rootScope);
                var scanOrder = scanOrderString.split(",");
                var extraparameters = { keepfilterparameters: true };
                for (var attribute in scanOrder) {
                    var currentAttribute = scanOrder[attribute];
                    // If the property is not already in th escan data, add it and its value
                    if (!searchData.hasOwnProperty(currentAttribute)) {
                        var localSearchData = {};
                        localSearchData[currentAttribute] = data;
                        searchService.refreshGrid(localSearchData, extraparameters);
                        return;
                    // Else if the property exists but is blank, set it to its new value
                    } else if (searchData[currentAttribute] === '') {
                        searchData[currentAttribute] = data;
                        searchService.refreshGrid(searchData, extraparameters);
                        return;
                    }
                }
            });
        },
        
    };

});


