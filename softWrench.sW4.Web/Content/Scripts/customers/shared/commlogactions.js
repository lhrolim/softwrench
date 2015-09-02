function CommLogActionsController($rootScope, $scope,contextService,fieldService) {
    var messageHeader = "<br/><br/>________________________________________________________________________________________________________" +
                                "<br/><b>From:</b> {0}"  +
                                "<br/><b>To:</b> {1}"  +
                                "<br/><b>Cc:</b> {2}" +
                                "<br/><b>Subject: </b> {3}" +
                                "<br/><br/>{4}";

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
        clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, emptyIfNull(origCc), origSubject, origMessage);
        clonedItem['createdate'] = null;
        $scope.$emit("sw.composition.edit", clonedItem);
    };

    $scope.reply = function (commlogitem) {
        var clonedItem = {};
        angular.copy(commlogitem, clonedItem);
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
        
        clonedItem['sendfrom'] = clonedItem['sendfrom'] ? clonedItem['sendfrom'] : contextService.getUserData().email;

        clonedItem['sendto'] = origSendFrom.indexOf(",") > -1 ? origSendFrom.split(',') : [origSendFrom];
        clonedItem['cc'] = null;

        clonedItem['commloguid'] = null;
        clonedItem['subject'] = "Re: " + origSubject;
        clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, emptyIfNull(origCc), origSubject, origMessage);
        clonedItem['createdate'] = null;
        $scope.$emit("sw.composition.edit", clonedItem);
    };

    

    $scope.replyAll = function (commlogitem) {

        var clonedItem = {};
        angular.copy(commlogitem, clonedItem);

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
        clonedItem['commloguid'] = null;

        clonedItem['sendto'] = nullOrCommaSplit(origSendFrom);
        clonedItem['cc'] = nullOrCommaSplit(origCc);

        clonedItem['sendfrom'] = origSendFrom ? origSendFrom : contextService.getUserData().email;
        clonedItem['subject'] = "Re: " + origSubject;
        clonedItem['message'] = messageHeader.format(origSendFrom, origSendTo, emptyIfNull(origCc), origSubject, origMessage);
        clonedItem['createdate'] = null;
        $scope.$emit("sw.composition.edit", clonedItem);
    };
}    
