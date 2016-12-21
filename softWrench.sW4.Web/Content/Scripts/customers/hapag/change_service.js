(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('changeservice', ["$http", "redirectService", function ($http, redirectService) {

    return {

        /// <summary>
        ///  Method used for opening a detail of a change request, from the grid.it s declared on the metadata, using list.click.service property. 
        /// 
        /// Since change grids contains both SR and Changes itself, there´s a custom logic determining which detail should be opened based both:
        /// 
        /// 1)which column was clicked (SR ID ==> always SR),
        /// 2)The data itself==> if it has no wonum (sr created by hapag), or if it has the magic number -666 on it
        /// 
        /// </summary>
        /// <param name="datamap"></param>
        /// <param name="column"></param>
        opendetail: function (datamap, column) {

            var wonum = datamap['wonum'];
            //this "wild" number (-666) is used because we have a custom join of 2 tables inside of a grid, and we need a way to sort it properly
            var isSr = wonum == null || wonum == "-666" || column.attribute == 'sr_.ticketid';
            var application = isSr ? 'servicerequest' : 'change';
            var id = isSr ? datamap['sr_.ticketid'] : wonum;
            var parameters = { id: id, popupmode: 'browser' };
            redirectService.goToApplicationView(application, 'detail', 'output', null, parameters);
        }
    };

}]);

})(angular);