(function (angular) {
    "use strict";

angular.module("sw_layout").factory('kongsberg.srService', ['srService', 'crudContextHolderService', function (srService, crudContextHolderService) {

    return {

        afterchangeowner: function (event) {
            srService.afterchangeowner(event);
        },

        afterchangeownergroup: function (event) {

            if (event.fields['ownergroup'] == null) {
                return;
            }
            if (event.fields['ownergroup'] == ' ') {
                event.fields['ownergroup'] = null;
                return;
            }
            if (event.fields['status'] == 'NEW') {
                event.fields['status'] = 'QUEUED';
                return;
            }
        },

        getSubjectDefaultExpression: function (datamap, schema, displayable) {

            var parentData = crudContextHolderService.rootDataMap();
            if (datamap['ownertable'].equalIc("SR")) {
                return "##" + parentData['ticketid'] + '## ' + parentData['description'];
            }
            return "";
        },

    };

}]);

})(angular);