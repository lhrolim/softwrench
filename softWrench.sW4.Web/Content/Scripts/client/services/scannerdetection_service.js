var app = angular.module('sw_layout');

app.factory('scannerdetectionService', function (searchService) {

    return {
        initInventoryGridListener: function (schema, datamap) {
            $(document).scannerDetection(function(data){
                searchService.refreshGrid(data);
            })
        },
        
    };

});


