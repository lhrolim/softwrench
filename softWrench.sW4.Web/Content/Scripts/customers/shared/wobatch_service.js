var app = angular.module('sw_layout');

app.factory('wobatchService', function (redirectService,restService,alertService) {

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
            application : "workorder",
            schema: "list",
            alias: "teste"
        }
        var json = {};
        json.ids = ids.join();
        restService.invokePost("Batch", "Create", parameters, json, function (data) {
            alertService.success("Batch successfully created",true);
            redirectService.goToApplicationView("_wobatch", "list", null, null, {}, null);
        }, null);
    }


    return {

        newBatch: function (event) {

            //            var searchDTO = {};
            //            searchDTO['searchParams'] = 'schedstart&&schedfinish';
            //            searchDTO['searchValues'] = '>={0}, , ,<={1}'.format(twoweeksAgo, now);
            //            //
            //            var parameters = {
            //                SearchDTO: searchDTO
            //            }
            redirectService.goToApplicationView("workorder", "createbatchlist", null, null, {}, null);
        },

        saveBatch: function (datamap) {
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
                alertService.confirm(null, null, function() {
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



    };

});