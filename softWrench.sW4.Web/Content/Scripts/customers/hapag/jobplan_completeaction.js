function JobPlanCompleteActionController($scope, $http, i18NService, contextService, schemaService, alertService) {

    "ngInject";

    var imacOptions = [
        { label: 'Select One', value: 'Select One' },
        { label: 'Completed', value: 'COMP' },
        { label: 'Failed', value: 'FAIL' },
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

    $scope.handleReasonReject = function (applicationName, compositionitem, schema, datamap, compositionschema) {
        //implementing both HAP-1170 and HAP-1169
        var label = applicationName === "imac" ? "Please enter the reason to set the task to FAIL." : "Please enter a valid Reason for rejecting the Change.";


        bootbox.prompt({
            inputType: 'textarea',
            title: label,
            buttons: {
                confirm: {
                    label: i18NService.get18nValue('general.save', 'Submit'),
                    className: "commandButton"
                },

                cancel: {
                    label: i18NService.get18nValue('_exportotoexcel.cancel', 'Cancel'),
                    className: "btn btn-default",
                    callback: function () {
                        return null;
                    }
                }

            },
            callback: function (result) {
                if (result === null || result === undefined) {
                    //true closes the modal
                    return true;
                }

                if (result === "") {
                    alertService.alert("Please inform your reason");
                    //false keeps it open
                    return false;
                }

                //validating according to the rules of HAP-1170
                var isValid = true;

                if (result.length < 7) {
                    isValid = false;
                }

                if (result.indexOf(' ') < 0) {
                    isValid = false;
                } else {
                    var words = result.split(" ");
                    for (var i=0; i< words.length; i++) {
                        if (words[i].length < 2) {
                            isValid = false;
                        }
                    }
                }

                if (!isValid) {
                    alertService.alert("The reason must contains at least 7 characters, one space, and each word should have at least 2 characters");
                    return false;
                }

                datamap.fields["#reasonreject"] = result;
                $scope.doSubmit(applicationName, compositionitem, schema, datamap, compositionschema);


            },
            className: "rejectionmodal"
        });
    }

    $scope.submitAction = function (compositionitem, schema, datamap, compositionschema) {

        var applicationName = schema.applicationName;
        datamap.fields['#selectedAction'] = $scope.actiontoexecute;
        if ((applicationName === "imac" && $scope.actiontoexecute === "FAIL") || (applicationName === "change" && "REJECTED".equalsIc($scope.actiontoexecute))) {
            return $scope.handleReasonReject(applicationName, compositionitem, schema, datamap, compositionschema);
        } else {
            return $scope.doSubmit(applicationName, compositionitem, schema, datamap, compositionschema);
        }



    };

    $scope.doSubmit = function (applicationName, compositionitem, schema, datamap, compositionschema) {
        var schemaId = compositionschema.applicationName;
        var isJobPlan = schemaId === "woactivity";
        datamap.fields['WoActivityId'] = compositionitem.wonum;
        datamap.fields['activityownergroup'] = compositionitem.ownergroup;
        datamap.fields['activitysequence'] = compositionitem.wosequence;
        datamap.fields['#tasksummary'] = compositionitem.description;

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

        contextService.insertIntoContext("skipmetadataonbrowser", true);

        $http.post(urlToUse, json)
            .success(function (result) {
                var relName = isJobPlan ? "woactivity_" : "approvals_";
                var eventData = {};
                eventData[relName] = {
                    list: result.resultObject.fields[relName],
                    relationship: relName
                };
                $scope.$emit("sw_compositiondataresolved", eventData);
                contextService.deleteFromContext("skipmetadataonbrowser");
            }).error(function (result) {
                contextService.deleteFromContext("skipmetadataonbrowser");
            });
    }

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