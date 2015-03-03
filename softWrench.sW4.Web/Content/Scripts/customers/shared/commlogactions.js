﻿function CommLogActionsController($rootScope, $scope,contextService) {
    var messageHeader = "<br/><br/>________________________________________________________________________________________________________" +
                                "<br/><b>From:</b> {0}"  +
                                "<br/><b>To:</b> {1}"  +
                                "<br/><b>Cc:</b> {2}" +
                                "<br/><b>Subject: </b> {3}" +
                                "<br/><br/>{4}";


    $scope.forward = function (commlogitem) {
        var clonedItem = {};
        angular.copy(commlogitem, clonedItem);
        var origSendFrom = clonedItem['sendfrom'] == null ? "" : clonedItem['sendfrom'];
        var origSendTo = clonedItem['sendto'] == null ? "" : clonedItem['sendto'];
        var origCc = clonedItem['cc'] == null ? "" : clonedItem['cc'];
        var origSubject = clonedItem['subject'] == null ? "" : clonedItem['subject'];
        var origMessage = clonedItem['message'];
        clonedItem['sendto'] = clonedItem['cc'] = clonedItem['commloguid']= null;
        clonedItem['sendfrom'] = $rootScope.defaultEmail;
        clonedItem['subject'] = "Fw: " + clonedItem['subject'];
        clonedItem['message'] = messageHeader.format(origSendFrom,origSendTo,origCc,origSubject,origMessage);
        $scope.$emit("sw.composition.edit", clonedItem);
    };

    $scope.reply = function (commlogitem) {
        var clonedItem = {};
        angular.copy(commlogitem, clonedItem);
        var origSendFrom = clonedItem['sendfrom'] == null ? "" : clonedItem['sendfrom'];
        var origSendTo = clonedItem['sendto'] == null ? "" : clonedItem['sendto'];
        var origCc = clonedItem['cc'] == null ? "" : clonedItem['cc'];
        var origSubject = clonedItem['subject'] == null ? "" : clonedItem['subject'];
        var origMessage = clonedItem['message'];
        clonedItem['sendto'] = clonedItem['sendfrom'];
        clonedItem['sendfrom'] = contextService.getUserData().email;
        clonedItem['cc'] = "";
        clonedItem['commloguid'] = null;
        clonedItem['subject'] = "Re: " + clonedItem['subject'];
        clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, origCc, origSubject, origMessage);
        $scope.$emit("sw.composition.edit", clonedItem);
    };

    $scope.replyAll = function (commlogitem) {
        var clonedItem = {};
        angular.copy(commlogitem, clonedItem);
        var origSendFrom = clonedItem['sendfrom'] == null ? "" : clonedItem['sendfrom'];
        var origSendTo = clonedItem['sendto'] == null ? "" : clonedItem['sendto'];
        var origCc = clonedItem['cc'] == null ? "" : clonedItem['cc'];
        var origSubject = clonedItem['subject'] == null ? "" : clonedItem['subject'];
        var origMessage = clonedItem['message'];
        clonedItem['commloguid'] = null;
        if (origCc != "") {
            clonedItem['sendto'] = origSendTo + "," +clonedItem['sendfrom'] + "," + origCc;
        } else {
            clonedItem['sendto'] = origSendTo + "," +clonedItem['sendfrom'];
        }

        clonedItem['sendfrom'] = contextService.getUserData().email;
        clonedItem['subject'] = "Re: " + clonedItem['subject'];
        clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, origCc, origSubject, origMessage);
        $scope.$emit("sw.composition.edit", clonedItem);
    };
}    