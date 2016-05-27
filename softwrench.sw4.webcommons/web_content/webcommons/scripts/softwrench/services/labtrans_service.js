(function (angular) {
    "use strict";

    angular.module('sw_layout').factory('labtransService', ["$q", "alertService", "redirectService", "crudContextHolderService", "restService", "searchService", "applicationService", "contextService",
        function ($q, alertService, redirectService, crudContextHolderService, restService, searchService, applicationService, contextService) {
            var calcLineCost = function (regularHours, regularRate, premiumHours, premiumRate) {
                var regularPay = regularHours * regularRate;
                var premiumPay = 0;
                if (premiumHours && premiumRate) {
                    premiumPay = premiumHours * (premiumRate * regularRate);
                }
                var lineCost = regularPay + premiumPay;
                if (lineCost == undefined) {
                    lineCost = "";
                }
                return lineCost;
            };

            var updateLineCost = function (event) {
                if (event.parentdata) {
                    var parentdatamap = event.parentdata.fields || event.parentdata;
                    // Update from one of the labor lines
                    var regularHours = parentdatamap['regularhrs'];
                    var regularRate = event.fields['payrate'];
                    var premiumHours = parentdatamap['premiumpayhours'];
                    var premiumRate = event.fields['ppcraftrate_.rate'];
                    event.fields['premiumpayrate'] = premiumRate;
                    event.fields['linecost'] = calcLineCost(regularHours, regularRate, premiumHours, premiumRate);
                } else if (event.fields['#laborlist_'] && (event.fields['_iscreation'] || event.fields['mode'] == 'batch')) {
                    // Update from the body of the batch labor detail
                    var labors = event.fields['#laborlist_'];
                    for (var laborIndex in labors) {
                        if (!labors.hasOwnProperty(laborIndex)) {
                            continue;
                        }
                        var currentLabor = labors[laborIndex];
                        var regularHours = event.fields['regularhrs'];
                        var regularRate = currentLabor['payrate'];
                        var premiumHours = event.fields['premiumpayhours'];
                        var premiumRate = currentLabor['ppcraftrate_.rate'];
                        currentLabor['premiumpayrate'] = premiumRate;
                        currentLabor['linecost'] = calcLineCost(regularHours, regularRate, premiumHours, premiumRate);
                    }
                } else {
                    // Update from the single labor detail
                    var regularHours = event.fields['regularhrs'];
                    var regularRate = event.fields['laborcraftrate_.rate'];
                    var premiumHours = event.fields['premiumpayhours'];
                    var premiumRate = event.fields['ppcraftrate_.rate'];
                    event.fields['premiumpayrate'] = premiumRate;
                    event.fields['linecost'] = calcLineCost(regularHours, regularRate, premiumHours, premiumRate);
                }
            };

            var deleteLabtrans = function(labtransIds) {
                return alertService.confirm2("Are you sure you wish to delete the selected labor transaction(s)? This operation cannot be undone.").then(function() {
                    return restService.postPromise("Labtrans", "DeleteLabtrans", {}, labtransIds).then(function(result) {
                        crudContextHolderService.clearSelectionBuffer(null);
                        searchService.refreshGrid();
                    });
                });
            };

            var approveLabtrans = function(labtransIds) {
                return alertService.confirm2("Are you sure you wish to approve the selected labor transaction(s)?").then(function () {
                    return restService.postPromise("Labtrans", "ApproveLabtrans", {}, labtransIds).then(function (result) {
                        crudContextHolderService.clearSelectionBuffer(null);
                        searchService.refreshGrid();
                    });
                });
            }

            return {
                afterlaborchange: function (event) {
                    if (event.fields['laborcode'] == ' ') {
                        event.fields['craft'] = null;
                        event.fields['payrate'] = 0.0; // Reset payrate
                        alertService.alert("Task field will be disabled if labor is not selected");
                    }
                    // TODO: Need to reset the display fields on craft after laborcode has been changed.
                    return;
                },
                aftercraftchange: function (event) {
                    if (event.fields['laborcraftrate_.rate'] != null) {
                        event.fields['payrate'] = event.fields['laborcraftrate_.rate'];
                        updateLineCost(event);
                    }
                    else {
                        event.fields['payrate'] = 0.0;
                        event.fields['laborcraftrate_.rate'] = 0.0;
                        event.fields['linecost'] = 0.0;
                    }
                },
                afterDateTimeChange: function(event) {
                    // If all of the datetime fields are filed
                    if ((event.fields['startdate'] && !event.fields['startdate'].nullOrEmpty()) &&
                    (event.fields['starttime'] && !event.fields['starttime'].nullOrEmpty()) &&
                    (event.fields['finishdate'] && !event.fields['finishdate'].nullOrEmpty()) &&
                    (event.fields['finishtime'] && !event.fields['finishtime'].nullOrEmpty())) {

                        var startDate = new Date(event.fields['startdate']);
                        var startTime = Date.parse(event.fields['starttime']);
                        startDate.setHours(startTime.getHours());
                        startDate.setMinutes(startTime.getMinutes());

                        var finishDate = new Date(event.fields['finishdate']);
                        var finishTime = Date.parse(event.fields['finishtime']);
                        finishDate.setHours(finishTime.getHours());
                        finishDate.setMinutes(finishTime.getMinutes());

                        // time diff
                        var difference = finishDate - startDate;
                        // convert ms to hours
                        var hours = difference / 3600000;
                        hours = hours.toPrecision(6);
                        // set the labor hours
                        event.fields['regularhrs'] = hours;
                    }
                },
                openNewDetailModal: function (modalschemaId) {
                    var schema = modalschemaId ? modalschemaId : "newdetail";
                    return redirectService.goToApplication("labtrans", schema, {}, {});
                },
                updateLineCost: updateLineCost,
                approveSingleLabtrans: function() {
                    var datamap = crudContextHolderService.rootDataMap();
                    var labtransIds = [];
                    labtransIds.push(datamap.fields.labtransid);
                    approveLabtrans(labtransIds);
                },
                approveMultipleLabtrans: function() {
                    var selectedLabtrans = crudContextHolderService.getSelectionModel(null).selectionBuffer;
                    var labtransIds = Object.keys(selectedLabtrans);
                    if (labtransIds.length < 1) {
                        return alertService.alert("There are no Labor Transaction selected");
                    }
                    approveLabtrans(labtransIds);
                },
                deleteSingleLabtrans: function() {
                    var datamap = crudContextHolderService.rootDataMap();
                    var labtransIds = [];
                    if (datamap.fields.genapprservreceipt == 1) {
                        return alertService.alert("Approved Labor Transactions cannot be deleted.");
                    }
                    labtransIds.push(datamap.fields.labtransid);
                    deleteLabtrans(labtransIds);
                },
                deleteMultipleLabtrans: function () {
                    var selectedLabtrans = crudContextHolderService.getSelectionModel(null).selectionBuffer;
                    var keys = Object.keys(selectedLabtrans);
                    // If not records are selected do nothing
                    if (keys.length == 0) {
                        return alertService.alert("There are no Labor Transaction selected");
                    }
                    var labtransIds = [];
                    var approvedLaborIds = [];
                    // Sort the labors checking for approved records
                    keys.forEach(function(key) {
                        if (selectedLabtrans[key].fields.genapprservreceipt == 1) {
                            approvedLaborIds.push(key);
                        } else {
                            labtransIds.push(key);
                        }
                    });
                    // If the set includes approved labors, they cannot be deleted
                    if (approvedLaborIds.length > 0) {
                        return alertService.alert("Approved Labor Transactions cannot be deleted.");
                    }
                    deleteLabtrans(labtransIds);
                },
                validateEdit: function(datamap, schema) {
                    if (datamap["genapprservreceipt"] === 1) {
                        alertService.alert("Cannot modify already approved labor transactions");
                        return $q.reject();
                    }
                    return $q.when();
                },
                editLabtrans: function() {
                    var datamap = crudContextHolderService.rootDataMap();
                    if (datamap.fields.genapprservreceipt == 1) {
                        alertService.alert("Cannot edit already approved labor transactions");
                        return false;
                    }
                    return true;
                },
                listClick: function(datamap, field, schema) {
                    var history = datamap["workorder_.historyflag"];
                    var approved = datamap["genapprservreceipt"];
                    var parameters = {
                        "id": datamap["labtransid"]
                    }
                    if (history || approved) {
                        redirectService.goToApplicationView("labtrans", "editdetail", "output", null, parameters);
                    } else {
                        redirectService.goToApplicationView("labtrans", "editdetail", "input", null, parameters);
                    }
                    return false;
                },
                defaultLaborExpression: function (datamap, schema, displayable) {
                    var username = '';
                    var rootdatamap = crudContextHolderService.rootDataMap();
                    if (rootdatamap.fields['#laborlist_'].length < 2) {
                        var user = contextService.getUserData();
                        username = user.login.toUpperCase();
                    }
                    return username;
                },
                save: function (datamap) {
                    applicationService.save().then(function (data) {
                        datamap.fields['labtransid'] = data.id;
                    });
                }
    };
}]);

})(angular);