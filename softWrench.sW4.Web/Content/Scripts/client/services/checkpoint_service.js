var app = angular.module('sw_layout');

app.factory('checkpointService', function (contextService,searchService) {

    return {
        
        createListCheckpoint: function (schema, searchData,paginationData) {
            var searchDTO = searchService.buildSearchDTO(searchData, {}, {}, null);
            searchDTO.pageNumber = extraParameters.pageNumber ? extraParameters.pageNumber : 1;
            searchDTO.totalCount = 0;
            searchDTO.pageSize = extraParameters.pageSize ? extraParameters.pageSize : 30;
        }

    }

});


