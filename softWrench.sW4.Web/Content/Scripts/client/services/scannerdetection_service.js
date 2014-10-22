var app = angular.module('sw_layout');

app.factory('scannerdetectionService', function ($http, restService, searchService) {

    return {
        initInventoryGridListener: function (schema, datamap, parameters) {
            var scanOrder = [];
            var searchData = parameters.searchData;
            var getUrl = restService.getActionUrl("Configuration", "GetConfiguration", { fullkey: schema.properties['config.fullKey'] });
            $http.get(getUrl).success(function (data) {
                var scanOrderString = data.substring(1, data.length - 1);
                scanOrder = scanOrderString.split(',');
            });

            $(document).scannerDetection(function (data) {
                for (var attribute in scanOrder) {
                    var currentAttribute = scanOrder[attribute];
                    // If the property is not already in th escan data, add it and its value
                    if (!searchData.hasOwnProperty(currentAttribute)) {
                        var localSearchData = {};
                        localSearchData[currentAttribute] = data;
                        searchService.refreshGrid(localSearchData);
                        return;
                    // Else if the property exists but is blank, set it to its new value
                    } else if (searchData[currentAttribute] === '') {
                        searchData[currentAttribute] = data;
                        searchService.refreshGrid(searchData);
                        return;
                    }
                }
            });
        },
        
    };

});


