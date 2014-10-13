﻿function JobPlanCompleteActionController($scope, $http, i18NService, contextService) {

    var imacOption = [
                { label: 'Select One', value: 'Select One' },
                { label: 'Completed', value: 'COMP' },
    ];

    var woactivityOptions = [
                { label: '-- Select One --', value: 'Select One' },
                { label: 'Completed', value: 'COMP' },
                { label: 'Not Required', value: 'NOTREQ' },
                { label: 'Failed', value: 'FAIL' }
    ];

    var approvalsOptions = [
                { label: '-- Select One --', value: 'Select One' },
                { label: 'Approved', value: 'Approved' },
                { label: 'Rejected', value: 'Rejected' }
    ];

    function isMemberOfOwnerGroup(ownerGroup) {
        if (ownerGroup == null && contextService.isLocal()) {
            return true;
        }
        var user = contextService.getUserData();
        var personGroups = user.personGroups;
        for (var i = 0; i < personGroups.length; i++) {
            var userGroup = personGroups[i];
            if (userGroup.personGroup.name == ownerGroup) {
                return true;
            }
        }
        return false;
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };
    
    $scope.getAvailableOptions = function (schema, compositionschema) {
        var applicationName = schema.applicationName;
        var schemaId = compositionschema.schemaId;
        if (applicationName == "imac") {
            return imacOptions;
        }
        if (compositionschema.applicationName == "woactivity") {
            return woactivityOptions;
        }
        if (compositionschema.applicationName == "approvals") {
            return approvalsOptions;
        }
    }

    $scope.submitAction = function (compositionitem, schema, datamap, compositionschema) {
        var schemaId = compositionschema.applicationName;
        var isJobPlan = schemaId == "woactivity";
        datamap.fields['WoActivityId'] = compositionitem.wonum;
        datamap.fields['activityownergroup'] = compositionitem.ownergroup;
        datamap.fields['activitysequence'] = compositionitem.wosequence;
        datamap.fields['#tasksummary'] = compositionitem.description;
        datamap.fields['#selectedAction'] = $scope.actiontoexecute;
        datamap.fields['#groupAction'] = getgroup(compositionitem, isJobPlan);
        var parameters = datamap.fields;
        var applicationName = schema.applicationName;
        var actionname = isJobPlan ? "completeaction" : "approvalaction";
        var urlToUse = url("api/data/operation/{0}/{1}?platform=web&id=".format(applicationName, actionname) + parameters.ticketid);
        parameters = addCurrentSchemaDataToJson(parameters, schema);
        var json = angular.toJson(parameters);
        $http.post(urlToUse, json).success(function () {
            window.location.reload();
        });
    };

    $scope.submitEnabled = function () {
        return $scope.actiontoexecute == null || $scope.actiontoexecute == 'Select One';
    }


    $scope.shouldshowaction = function (compositionitem, schema, compositionschema) {
        var schemaId = compositionschema.applicationName;
        var isJobPlan = schemaId == "woactivity";
        if (isJobPlan) {
            var group = getgroup(compositionitem, true);
            return compositionitem.status == "INPRG" && isMemberOfOwnerGroup(group);
        }
        //calculated at server side ChangeDataSet
        return compositionitem["#shouldshowaction"] == true;
    };

    function getgroup(compositionitem, isJobPlan) {
        if (isJobPlan) {
            return compositionitem.ownergroup;
        }
        return compositionitem.approvergroup;
    }

}