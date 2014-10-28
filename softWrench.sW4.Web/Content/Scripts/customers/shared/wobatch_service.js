var app = angular.module('sw_layout');

app.factory('wobatchService', function (redirectService, $rootScope, restService, alertService, validationService) {

    function doSave(ids) {
        if (ids.length == 0) {
            alertService.alert("Please select at least one element");
            return;
        }

        if (ids.length > 500) {
            alertService.alert("Please restrict your batch to 500 elements");
            return;
        }

        saveOrUpdateAliasCode(function (alias) {
            var parameters = {
                application: "workorder",
                schema: "list",
                alias: alias
            }
            var json = {};
            json.ids = ids.join();
            restService.invokePost("Batch", "Create", parameters, json, function (data) {
                alertService.success("Batch successfully created", true);
                var batchId = data.resultObject.id;
                var searchDTO = {
                    searchParams: "id",
                    searchValues: batchId
                }
                redirectService.goToApplication("workorder", "editbatch", { searchDTO: searchDTO }, null);
            }, null);
        });
    }


    function saveOrUpdateAliasCode(cbckFn) {
        var saveFormSt = $("#savebatchesform").prop('outerHTML');
        //remove display:none
        saveFormSt = saveFormSt.replace('none', '');
        //change id of the filter sdao that it becomes reacheable via jquery
        saveFormSt = saveFormSt.replace('batchalias', 'batchalias2');
        bootbox.dialog({
            message: saveFormSt,
            title: "Save Batch",
            buttons: {
                cancel: {
                    label: 'Cancel',
                    className: "btn btn-default",
                    callback: function () {
                        return null;
                    }
                },
                main: {
                    label: 'Save',
                    className: "btn-primary",
                    callback: function () {
                        var alias = $('#batchalias2').val();
                        cbckFn(alias);
                    }

                }
            },
            className: "smallmodal"
        });
    }

    return {

        exit: function (event) {
            alertService.confirm(null, null, function (data) {
                redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
            }, "Any unsaved work will be lost. Are you sure you want to cancel this Batch?");
        },

        newBatch: function (event) {

            //            var searchDTO = {};
            //            searchDTO['searchParams'] = 'schedstart&&schedfinish';fille
            //            searchDTO['searchValues'] = '>={0}, , ,<={1}'.format(twoweeksAgo, now);
            //            //
            //            var parameters = {
            //                SearchDTO: searchDTO
            //            }
            redirectService.goToApplication("workorder", "createbatchlist", {}, null);
        },

        edit: function (datamap, column) {
            var batchId = datamap['id'];
            var searchDTO = {
                searchParams: "id",
                searchValues: batchId
            }
            redirectService.goToApplication("workorder", "editbatch", { searchDTO: searchDTO }, null);
        },

        generatebatch: function (datamap) {
            var ids = [];
            var alreadyused = [];
            $.each(datamap, function (key, dm) {
                var fields = dm.fields;
                if (fields["_#selected"] == true) {
                    if (fields["#alreadyused"] != true) {
                        ids.push(fields["wonum"]);
                    } else {
                        alreadyused.push(fields["wonum"]);
                    }
                }
            });
            if (alreadyused.length != 0) {
                alertService.confirm(null, null, function () {
                    doSave(ids);
                }, "The items {0} are already used on other batches and won´t be included. Proceed?".format(alreadyused.join()));
                return;
            }
            doSave(ids);
        },

        cancelBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },

        clickeditbatch: function (datamap, column, schema) {

            var message = '';
            var buttons = {};
            var cancelButton = {
                label: 'Cancel',
                className: "btn btn-default",
                callback: function () {
                    return null;
                }
            };
            var mainButton = {
                label: 'OK',
                className: "btn-primary"
            };

            switch (column['attribute']) {
                case 'description':
                    message = datamap['description'];
                    mainButton.callback = function (result) {
                        return null;
                    };
                    buttons = {
                        main: mainButton
                    };
                    break;
                case '#fdbckcomment':
                    var message = $("#feedbackcommentform").prop('outerHTML');

                    //remove display:none
                    message = message.replace('none', '');

                    //change id of the filter so that it becomes reacheable via jquery
                    message = message.replace('feedbackcommentname', 'feedbackcommentname2');

                    //set the initial value
                    message = message.replace('#feedbackcomment', nullOrEmpty(datamap['#fdbckcomment']) ? '' : datamap['#fdbckcomment']);
                    mainButton.callback = function (result) {
                        if (result) {
                            datamap['#fdbckcomment'] = $('#feedbackcommentname2').val();
                            $rootScope.$digest();
                        }
                    };
                    buttons = {
                        cancel: cancelButton,
                        main: mainButton
                    };
                    break;
                case '#lognote':
                    var message = $("#lognoteform").prop('outerHTML');

                    //remove display:none
                    message = message.replace('none', '');

                    //change id of the filter so that it becomes reacheable via jquery
                    message = message.replace('summary', 'summary2');
                    message = message.replace('details', 'details2');

                    //set the initial value
                    var worklogs = datamap['worklog_'];
                    if (worklogs != null && worklogs.length > 0) {
                        var worklog = worklogs[0]; // supports only 1 worklog entry
                        message = message.replace('#lognotesummary', nullOrEmpty(worklog['description']) ? '' : worklog['description']);
                        message = message.replace('#lognotedetails', nullOrEmpty(worklog['longdescription_.ldtext']) ? '' : worklog['longdescription_.ldtext']);
                    } else {
                        message = message.replace('#lognotesummary', '');
                        message = message.replace('#lognotedetails', '');
                    }

                    mainButton.callback = function (result) {
                        if (result) {
                            var worklog = {};
                            worklog['description'] = $('#summary2').val();
                            worklog['longdescription_.ldtext'] = $('#details2').val();
                            var hasData = worklog['description'] != "" || worklog['longdescription_.ldtext'] != "";
                            if (!hasData) {
                                return;
                            }
                            datamap['worklog_'] = [];
                            datamap['worklog_'].push(worklog);


                            datamap['#lognote'] = 'Y';
                            $rootScope.$digest();
                        }
                    };
                    buttons = {
                        cancel: cancelButton,
                        main: mainButton
                    };
                    break;
                case "#closed":
                    if (datamap["#closed"]) {
                        //lets validate required fields first
                        var valArray = validationService.getInvalidLabels(schema.displayables, datamap);
                        if (datamap["#ReconCd"] != "00" && !datamap["#fdbckcomment"]) {
                            valArray.push("Feedback Comment");
                        }

                        if (valArray.length != 0) {
                            var message = "";
                            for (var i = 0; i < valArray.length; i++) {
                                var item = valArray[i];
                                message += "<li>{0}</li>".format(item);
                            }

                            alertService.alert("This workorder cannot be closed because there are required fields not completed: <br></br><ul>{0}</ul>".format(message));
                            datamap["#closed"] = false;
                            $rootScope.$digest();
                            return;
                        }
                    }
                    return;
                default:
                    return;
            }

            bootbox.dialog({
                message: message,
                title: column['label'],
                buttons: buttons,
                className: "mediummodal"
            });
        },

        savebatch: function (datamap) {
            //            saveOrUpdateAliasCode(function (alias) {

            //workaround: the batchid is inserted into every row
            var batchId = datamap[0].fields["#batchId"];
            var batchAlias = datamap[0].fields["#batchalias"];
            var parameters = {
                application: "workorder",
                schema: "list",
                batchId: batchId,
            }
            var json = {};
            json.datamap = datamap;
            restService.invokePost("Batch", "Update", parameters, json, function (data) {
                alertService.success("Batch {0} successfully saved".format(batchAlias), true);
            });
            //            });
        },





    };

});