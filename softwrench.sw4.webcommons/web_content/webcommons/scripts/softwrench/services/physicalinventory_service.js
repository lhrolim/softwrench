(function (angular) {
    "use strict";

angular.module('webcommons_services')
    .factory('physicalInventoryService', ["formatService", function (formatService) {

    var physicalCount = function (schema, datamap, type) {
        var lastcountdate = new Date(datamap['physcntdate']);
        datamap['#lastCountDate'] = formatService.formatDate(lastcountdate, "MM/dd/yyyy HH:mm");
        datamap['physcntdate'] = formatService.formatDate(new Date(), "MM/dd/yyyy HH:mm");
        if (type === "newCount") {
            datamap['physcnt'] = 0;
        }
    }
    return {
        editPhysicalCount: function (schema, datamap) {
            physicalCount(schema, datamap, "editCount");
        },

        newPhysicalCount: function (schema, datamap) {
            physicalCount(schema, datamap, "newCount");
        },
    };
}]);

})(angular);