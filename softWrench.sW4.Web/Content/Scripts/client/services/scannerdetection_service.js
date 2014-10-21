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
                    if (!searchData.hasOwnProperty(scanOrder[attribute])) {
                        var localSearchData = {};
                        localSearchData[scanOrder[attribute]] = data;
                        searchService.refreshGrid(localSearchData);
                        return;
                    } else if(searchData[scanOrder[attribute]] === '') {
                        searchData[scanOrder[attribute]] = data;
                        searchService.refreshGrid(searchData);
                        return;
                    }
                }
            });
        },
        
    };

});


