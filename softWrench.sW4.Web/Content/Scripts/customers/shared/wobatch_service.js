var app = angular.module('sw_layout');

app.factory('wobatchService', function (redirectService, restService, alertService, validationService) {

    function doSave(ids) {
        if (ids.length == 0) {
            alertService.alert("Please select at least one element");
            return;
        }

        if (ids.length > 500) {
            alertService.alert("Please restrict your batch to 500 elements");
            return;
        }

        var parameters = {
            application: "workorder",
            schema: "list",
            alias: "teste"
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
    }


    return {

        exit: function (event) {
            alertService.confirm(null, null, function (data) {
                redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
            }, "Any non saved work will be lost. Are you sure you want to cancel the Batch?");
        },

        newBatch: function (event) {

            //            var searchDTO = {};
            //            searchDTO['searchParams'] = 'schedstart&&schedfinish';
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
            //TODO: Alias
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


            //            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },

        cancelBatch: function (event) {
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        },

        clickeditbatch: function (datamap, column) {

            if (column["attribute"] == "#closed") {
                if (datamap["#closed"]) {
                    //lets validate required fields first
                    var valArray = validationService.getInvalidLabels(schema.displayables, datamap);
                    if (datamap["#ReconCd"] == "00" && !datamap["#fdbckcomment"]) {
                        valArray.push("Feedback Comment");
                    }

                    if (valArray.length != 0) {
                        var message = "";
                        for (var i = 0; i < valArray.length; i++) {
                            var item = valArray[i];
                            message += "<li>{0}</li>".format(item);
                        }

                        alertService.alert("This workorder cannot be closed because there are required fields not filled: <br></br><ul>{0}</ul>".format(message));
                        datamap["#closed"] = false;
                        return;
                    }

                }
            }

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
                    mainButton.callback = function (result) {
                        if (result) {
                            datamap['#fdbckcomment'] = $('#feedbackcommentname2').val();
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
                    mainButton.callback = function (result) {
                        if (result) {
                            datamap['#lognote'] = $('#summary2').val();
                            }
                    };
                    buttons = {
                        cancel: cancelButton,
                        main: mainButton
                    };
                    break;
                default:
                    return;
                        }

            bootbox.dialog({
                message: message,
                title: column['label'],
                buttons: buttons,
                    className: "smallmodal"
                });
        },

        savebatch: function (datamap) {
            //workaround: the batchid is inserted into every row
            var batchId = datamap[0].fields["#batchId"];
            var parameters = {
                application: "workorder",
                schema: "list",
                batchId: batchId
            }
            var json = {};
            json.datamap = datamap;
            restService.invokePost("Batch", "Update", parameters, json, function (data) {
                alertService.success("Batch successfully saved", true);
            });
        },

       



    };

});