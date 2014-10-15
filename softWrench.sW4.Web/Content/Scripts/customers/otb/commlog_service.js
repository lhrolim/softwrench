var app = angular.module('sw_layout');

app.factory('commlogService', function ($http, contextService, restService) {

    return {
        updatereadflag: function (parameters) {

            if (parameters.compositionItem["read"]) {
                return;
            }

            var user = contextService.getUserData();

            var httpParameters = {
                application: parameters.application,
                applicationItemId: parameters.applicationItemId,
                userId: user.dbId,
                commlogId: parameters.compositionId
            };

            //Update icon on list -- this is performed before the DB call so that there is no lag between the "spinner image" dissapearing and the unread icon dissapearing
            //In order to update the object list, we need to find the index of the current item.
            //TODO: Make it easier to update the list data (I.e. remove the iteration)
            var index = 0;
            var parentCompositionData = parameters.parentCompositionData;
            while (parentCompositionData[index].commloguid != parameters.compositionId) {
                index++;
            }
            parentCompositionData[index].read = true;

            restService.invokePost("Commlog", "UpdateReadFlag", httpParameters, null, null, function() {
                parentCompositionData[index].read = false;
            });
        }
    };

});