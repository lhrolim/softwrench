var app = angular.module('sw_layout');

app.factory('wobatchService', function (redirectService) {

    return {

        newBatch: function (event) {

//            var searchDTO = {};
//            searchDTO['searchParams'] = 'schedstart&&schedfinish';
//            searchDTO['searchValues'] = '>={0}, , ,<={1}'.format(twoweeksAgo, now);
//            //
//            var parameters = {
//                SearchDTO: searchDTO
//            }
            redirectService.goToApplicationView("workorder", "createbatchlist", null, null, {}, null);
        },

        saveBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },



    };

});