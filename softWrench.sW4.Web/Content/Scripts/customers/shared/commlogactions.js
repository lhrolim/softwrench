
(function (angular) {
    'use strict';

    var module = angular.module('sw_layout');

    module.constant("commlog_messagheader", {
        //above this limit framework shall no longer produce the full rowstamp map, but rather just pass the maxrowstamp to the server
        messageHeader: "<br/><br/>________________________________________________________________________________________________________" +
            "<br/><b>From:</b> {0}" +
            "<br/><b>To:</b> {1}" +
            "<br/><b>Cc:</b> {2}" +
            "<br/><b>Subject: </b> {3}" +
            "<br/><br/>{4}"
    });



    module
      .controller('CommLogActionsController', ['$rootScope', '$scope', 'contextService', 'fieldService', 'applicationService', CommLogActionsController]);




    function CommLogActionsController($rootScope, $scope, contextService, fieldService, applicationService, commlog_messagheader) {


        function nullOrCommaSplit(value) {
            if (value == null) {
                return null;
            }
            return value.split(",");
        }

        function emptyIfNull(value) {
            if (value == null) {
                return "";
            }
            return value;
        }

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

        // Forward message
        // Send to: User entered
        // Send from: Default address, if none then current user email
        // CC: User entered
        // Subject: "Fw:" + Original subject
        $scope.forward = function (commlogitem) {
            applicationService.getApplicationDataPromise("commlog", "detail", { id: commlogitem["commloguid"] }).then(function (result) {
                var clonedItem = {};
                angular.copy(result.data.resultObject.fields, clonedItem);
                var origSendFrom = clonedItem['sendfrom'] == null ? "" : clonedItem['sendfrom'];
                var origSendTo = clonedItem['sendto'] == null ? "" : clonedItem['sendto'];
                var origCc = clonedItem['cc'] == null ? "" : clonedItem['cc'];
                var origSubject = clonedItem['subject'] == null ? "" : clonedItem['subject'];
                var origMessage = clonedItem['message'] == null ? "" : clonedItem['message'];
                clonedItem['sendto'] = clonedItem['cc'] = clonedItem['commloguid'] = null;
                clonedItem['sendfrom'] = $rootScope.defaultEmail;
                clonedItem['subject'] = "Fw: " + clonedItem['subject'];
                clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, emptyIfNull(origCc), origSubject, origMessage);
                clonedItem['createdate'] = null;
                fieldService.fillDefaultValues($scope.compositiondetailschema.displayables, clonedItem, $scope);
                $scope.$emit("sw.composition.edit", clonedItem);
            });

        };

        // Reply to Original sender
        // Send to: Original sendfrom
        // Send from: Default address, if none then current user email
        // CC: Same CC as the original communication
        // Subject: "Re:" + Original subject
        $scope.reply = function (commlogitem) {
            applicationService.getApplicationDataPromise("commlog", "detail", { id: commlogitem["commloguid"] })
                .then(function (result) {
                    var clonedItem = {};
                    angular.copy(result.data.resultObject.fields, clonedItem);
                    var origSendFrom = clonedItem['sendfrom'] == null ? "" : clonedItem['sendfrom'];
                    var origSendTo = clonedItem['sendto'] == null ? "" : clonedItem['sendto'];
                    var origCc = clonedItem['cc'];

                    var origSubject = emptyIfNull(clonedItem['subject']);
                    var origMessage = emptyIfNull(clonedItem['message']);

                    for (var attribute in clonedItem) {
                        if (clonedItem.hasOwnProperty(attribute)) {
                            clonedItem[attribute] = null;
                        }
                    }
                    fieldService.fillDefaultValues($scope.compositiondetailschema.displayables, clonedItem, $scope);
                    // The clonedItem['sendfrom'] should now have the default value filled or be null, in which case it should be set to the users email address
                    clonedItem['sendfrom'] = clonedItem['sendfrom'] ? clonedItem['sendfrom'] : contextService.getUserData().email;

                    clonedItem['sendto'] = origSendFrom.indexOf(",") > -1 ? origSendFrom.split(',') : [origSendFrom];
                    clonedItem['cc'] = null;

                    clonedItem['commloguid'] = null;
                    clonedItem['subject'] = "Re: " + origSubject;
                    clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, emptyIfNull(origCc), origSubject, origMessage);
                    $scope.$emit("sw.composition.edit", clonedItem);
                });
        };


        // Reply to all
        // Send to: Original sendfrom, all send to's, removing the new sendfrom from the list if it is present
        // Send from: Default address, if none then current user email
        // CC: Same CC as the original communication
        // Subject: "Re:" + Original subject
        $scope.replyAll = function (commlogitem) {
            applicationService.getApplicationDataPromise("commlog", "detail", { id: commlogitem["commloguid"] })
                .then(function (result) {
                    var clonedItem = {};
                    angular.copy(result.data.resultObject.fields, clonedItem);

                    var origSendFrom = clonedItem['sendfrom'];
                    var origSendTo = clonedItem['sendto'];
                    var origCc = clonedItem['cc'];

                    var origSubject = emptyIfNull(clonedItem['subject']);
                    var origMessage = emptyIfNull(clonedItem['message']);

                    for (var attribute in clonedItem) {
                        if (clonedItem.hasOwnProperty(attribute)) {
                            clonedItem[attribute] = null;
                        }
                    }
                    fieldService.fillDefaultValues($scope.compositiondetailschema.displayables, clonedItem, $scope);
                    // The clonedItem['sendfrom'] should now have the default value filled or be null, in which case it should be set to the users email address
                    clonedItem['sendfrom'] = clonedItem['sendfrom'] ? clonedItem['sendfrom'] : contextService.getUserData().email;
                    clonedItem['commloguid'] = null;
                    clonedItem['sendto'] = buildReplyAllSendTo(origSendTo, origSendFrom, clonedItem['sendfrom']);
                    clonedItem['cc'] = nullOrCommaSplit(origCc);
                    clonedItem['subject'] = "Re: " + origSubject;
                    clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, emptyIfNull(origCc), origSubject, origMessage);
                    $scope.$emit("sw.composition.edit", clonedItem);
                });
        };



    }
})(angular);