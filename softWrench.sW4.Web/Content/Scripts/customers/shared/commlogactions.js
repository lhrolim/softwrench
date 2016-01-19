
(function (angular) {
    'use strict';

    var module = angular.module('sw_layout');

    module.constant("commlog_messagheader",
        //above this limit framework shall no longer produce the full rowstamp map, but rather just pass the maxrowstamp to the server
        "<br/><br/>{0}" +
        "________________________________________________________________________________________________________" +
        "<br/><b>From:</b> {1}" +
        "<br/><b>To:</b> {2}" +
        "<br/><b>Cc:</b> {3}" +
        "<br/><b>Subject: </b> {4}" +
        "<br/><br/>{5}"
    );

    function commLogActionsController($rootScope, $scope, contextService, fieldService, applicationService, commlog_messagheader) {


        function buildReplyAllSendTo(origTo, origFrom, newFrom) {
            var transFrom = nullOrCommaSplit(origFrom);
            var transTo = nullOrCommaSplit(origTo);
            var newTo = transFrom.concat(transTo);
            var userAddressIndex = newTo.indexOf(newFrom);
            if (userAddressIndex > -1) {
                newTo.splice(userAddressIndex, 1);
            }
            return newTo;
        }


        function getServerData(commloglistitem) {
            return applicationService.getApplicationDataPromise("commlog", "detail", { id: commloglistitem["commloguid"] })
                .then(function (result) {
                    return result.data.resultObject.fields;
                });
        }

        function dispatchEvent(clonedItem) {
            $scope.$emit("sw.composition.edit", clonedItem);
            return clonedItem;
        }

        function normalizeOriginal(originalItem, subject) {
            originalItem["sendfrom"] = emptyIfNull(originalItem["sendfrom"]);
            originalItem["sendto"]= emptyIfNull(originalItem["sendto"]);
            originalItem["cc"] =emptyIfNull(originalItem["cc"]);
            originalItem["subject"]= emptyIfNull(originalItem['subject']);
            originalItem["message"] = emptyIfNull(originalItem['message']);
            return originalItem;
        }

        function buildMessage(originalItem) {
            var preferences = contextService.getUserData().userPreferences;
            var signature = preferences == null ? "" : preferences.signature;
            return commlog_messagheader.format(signature, originalItem.sendfrom, originalItem.sendto, emptyIfNull(originalItem.cc), originalItem.subject, originalItem.message);
        }


        function commonstransform(originalItem,replyMode) {
            normalizeOriginal(originalItem);
            var clonedItem = fieldService.fillDefaultValues($scope.compositiondetailschema.displayables, { commloguid :null}, $scope);

            // If KOGT, set subject to null so that the default subject from metadata will populate
            var client = contextService.client();
            if (client != null && client.equalIc("kongsberg") && originalItem['ownertable'].equalIc("SR")) {
                clonedItem['subject'] = null;
            } else {
                var subjectPrefix = replyMode ? "Re: " : "Fw: ";
                clonedItem['subject'] = subjectPrefix + originalItem.subject;
            }

            // if there was a default value marked for the sendfrom it shall be used, otherwise fallinback to user default email
            clonedItem['sendfrom'] = clonedItem['sendfrom'] ? clonedItem['sendfrom'] : contextService.getUserData().email;
            clonedItem['cc'] = nullOrCommaSplit(originalItem.cc);
            clonedItem['message'] = buildMessage(originalItem);
            clonedItem['createdate'] = fieldService.currentDate();
            return clonedItem;
        }

        var transformReplyAll = function (originalItem) {
            var detailItem = commonstransform(originalItem,true);
            detailItem['sendto'] = buildReplyAllSendTo(originalItem.sendto, originalItem.sendfrom, detailItem['sendfrom']);
            return detailItem;
        }

        var transformReply = function (originalItem) {
            var detailItem = commonstransform(originalItem, true);
            detailItem['sendto'] = originalItem.sendfrom.indexOf(",") > -1 ? originalItem.sendfrom.split(',') : [originalItem.sendfrom];
            return detailItem;
        }

        var transformForward = function (originalItem) {
            var detailItem = commonstransform(originalItem, false);
            detailItem['sendto'] = detailItem['cc'] = null;
            return detailItem;
        }

        // Forward message
        // Send to: User entered
        // Send from: Default address, if none then current user email
        // CC: User entered
        // Subject: "Fw:" + Original subject
        $scope.forward = function (commlogitem) {

            //TODO: open new message in modal
            return getServerData(commlogitem)
                .then(transformForward)
                .then(dispatchEvent);
        };

        // Reply to Original sender
        // Send to: Original sendfrom
        // Send from: Default address, if none then current user email
        // CC: Same CC as the original communication
        // Subject: "Re:" + Original subject
        $scope.reply = function (commlogitem) {

            //TODO: open new message in modal
            return getServerData(commlogitem)
                .then(transformReply)
               .then(dispatchEvent);
        };

        // Reply to all
        // Send to: Original sendfrom, all send to's, removing the new sendfrom from the list if it is present
        // Send from: Default address, if none then current user email
        // CC: Same CC as the original communication
        // Subject: "Re:" + Original subject
        $scope.replyAll = function (commlogitem) {

            //TODO: open new message in modal
            return getServerData(commlogitem)
               .then(transformReplyAll)
               .then(dispatchEvent);
        };
    }

    module.controller('CommLogActionsController', ['$rootScope', '$scope', 'contextService', 'fieldService', 'applicationService', 'commlog_messagheader', commLogActionsController]);

})(angular);