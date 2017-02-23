function JobPlanCompleteActionController($scope, $http, i18NService, contextService, schemaService) {

    "ngInject";

    var imacOptions = [
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
        if (contextService.InModule(["xitc"])) {
            //HAP-1007
            //for xitc, not only the persongroups of the user, but also the indirect locations brought due to R0017 should also be considered.
            //this property is set on HapagImacDataSet
            var xitcLocations = user.genericproperties["hlaglocationxitc"];
            for (var j = 0; j < xitcLocations.length; j++) {
                var xitcLocation = xitcLocations[j];
                if (xitcLocation === ownerGroup) {
                    return true;
                }
            }
        }

        return false;
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.getAvailableOptions = function (schema, compositionschema) {
        if (schema == null) {
            //this would happen only for printing
            return [];
        }
        var applicationName = schema.applicationName;
        var schemaId = compositionschema.schemaId;
        if (applicationName === "imac") {
            return imacOptions;
        }
        if (compositionschema.applicationName === "woactivity") {
            return woactivityOptions;
        }
        if (compositionschema.applicationName === "approvals") {
            return approvalsOptions;
        }
    }

    $scope.submitAction = function (compositionitem, schema, datamap, compositionschema) {
        var schemaId = compositionschema.applicationName;
        var applicationName = schema.applicationName;
        var isJobPlan = schemaId == "woactivity";
        datamap.fields['WoActivityId'] = compositionitem.wonum;
        datamap.fields['activityownergroup'] = compositionitem.ownergroup;
        datamap.fields['activitysequence'] = compositionitem.wosequence;
        datamap.fields['#tasksummary'] = compositionitem.description;
        datamap.fields['#selectedAction'] = $scope.actiontoexecute;
        datamap.fields['#groupAction'] = getgroup(compositionitem, isJobPlan);
        var numberOfActions = datamap.fields["#numberofapprovalactions"];
        if (!isJobPlan) {
            datamap.fields['#lastaction'] = numberOfActions == 1;
        }

        var parameters = datamap.fields;
        var actionname = isJobPlan ? "completeaction" : "approvalaction";
        var urlToUse = url("api/data/operation/{0}/{1}?platform=web&id=".format(applicationName, actionname) + schemaService.getId(datamap, $scope.parentschema));
        parameters = addCurrentSchemaDataToJson(parameters, schema);
        var json = angular.toJson(parameters);
        $http.post(urlToUse, json).success(function (result) {
            var relName = isJobPlan ? "woactivity_" : "approvals_";
            var eventData = {};
            eventData[relName] = {
                list: result.resultObject.fields[relName],
                relationship: relName
            };
            $scope.$emit("sw_compositiondataresolved", eventData);
        });
    };

    $scope.submitEnabled = function () {
        return $scope.actiontoexecute == null || $scope.actiontoexecute == 'Select One';
    }


    $scope.shouldshowaction = function (compositionitem, schema, compositionschema) {
        if (!compositionschema) {
            //print mode
            return false;
        }
        var schemaId = compositionschema.applicationName;
        var isJobPlan = schemaId == "woactivity";
        if (isJobPlan) {
            var group = getgroup(compositionitem, true);
            return compositionitem.status === "INPRG" && isMemberOfOwnerGroup(group);
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