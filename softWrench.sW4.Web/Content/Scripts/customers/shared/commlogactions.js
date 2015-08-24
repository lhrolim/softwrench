function CommLogActionsController($rootScope, $scope,contextService,fieldService) {
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
        var origMessage = clonedItem['message'] == null ? "" : clonedItem['message'];
        clonedItem['sendto'] = clonedItem['cc'] = clonedItem['commloguid']= null;
        clonedItem['sendfrom'] = $rootScope.defaultEmail;
        clonedItem['subject'] = "Fw: " + clonedItem['subject'];
        clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, origCc, origSubject, origMessage);
        clonedItem['createdate'] = null;
        $scope.$emit("sw.composition.edit", clonedItem);
    };

    $scope.reply = function (commlogitem) {
        var clonedItem = {};
        angular.copy(commlogitem, clonedItem);
        var origSendFrom = clonedItem['sendfrom'] == null ? "" : clonedItem['sendfrom'];
        var origSendTo = clonedItem['sendto'] == null ? "" : clonedItem['sendto'];
        var origCc = clonedItem['cc'] == null ? "" : clonedItem['cc'];
        var origSubject = clonedItem['subject'] == null ? "" : clonedItem['subject'];
        var origMessage = clonedItem['message'] == null ? "" : clonedItem['message'];
        for (var attribute in clonedItem) {
            if (clonedItem.hasOwnProperty(attribute)) {
                clonedItem[attribute] = null;
            }
        }
        fieldService.fillDefaultValues($scope.compositiondetailschema.displayables, clonedItem, $scope);
        clonedItem['sendto'] = origSendFrom.split(',');
        clonedItem['sendfrom'] = clonedItem['sendfrom'] ? clonedItem['sendfrom'] : contextService.getUserData().email;
        clonedItem['cc'] = clonedItem['cc'] ? clonedItem['cc'].split(',') : "";
        clonedItem['commloguid'] = null;
        clonedItem['subject'] = "Re: " + origSubject;
        clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, origCc, origSubject, origMessage);
        clonedItem['createdate'] = null;
        $scope.$emit("sw.composition.edit", clonedItem);
    };

    $scope.replyAll = function (commlogitem) {
        var clonedItem = {};
        angular.copy(commlogitem, clonedItem);
        var origSendFrom = clonedItem['sendfrom'] == null ? "" : clonedItem['sendfrom'];
        var origSendTo = clonedItem['sendto'] == null ? "" : clonedItem['sendto'];
        var origCc = clonedItem['cc'] == null ? "" : clonedItem['cc'];
        var origSubject = clonedItem['subject'] == null ? "" : clonedItem['subject'];
        var origMessage = clonedItem['message'] == null ? "" : clonedItem['message'];
        for (var attribute in clonedItem) {
            if (clonedItem.hasOwnProperty(attribute)) {
                clonedItem[attribute] = null;
            }
        }
        fieldService.fillDefaultValues($scope.compositiondetailschema.displayables, clonedItem, $scope);
        clonedItem['commloguid'] = null;
        clonedItem['sendto'] = origSendFrom.split(',');
        //if (origCc != "") {
        //    clonedItem['sendto'] = clonedItem['sendfrom'] + "," + origCc;
        //} else {
        //    clonedItem['sendto'] = clonedItem['sendfrom'];
        //}

        clonedItem['sendfrom'] = clonedItem['sendfrom'] ? clonedItem['sendfrom'] : contextService.getUserData().email;
        clonedItem['subject'] = "Re: " + origSubject;
        clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, origCc, origSubject, origMessage);
        clonedItem['createdate'] = null;
        $scope.$emit("sw.composition.edit", clonedItem);
    };
}    
