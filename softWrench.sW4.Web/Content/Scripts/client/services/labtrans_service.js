var app = angular.module('sw_layout');

app.factory('labtranService', function ($http, contextService, redirectService, modalService, restService, searchService, alertService, validationService) {
    return {
        displayModal: function (parentschema, parentdatamap) {
            var compositionschema = parentschema.cachedCompositions['labtrans_'].schemas['detail'];
            var user = contextService.getUserData();
            var dtm = new Date();
            var formatteddate = dtm.toLocaleDateString();
            var newdatamap = {};

            // quick reset of the modal data and also pre-populate with desire data
            newdatamap["refwo"] = parentdatamap.fields["wonum"];
            newdatamap["siteid"] = parentdatamap.fields["siteid"];
            newdatamap["transtype"] = "WORK";
            newdatamap["laborcode"] = user.login.toUpperCase();
            newdatamap["enterby"] = user.login.toUpperCase();
            newdatamap["enterdate"] = formatteddate;
            newdatamap["transdate"] = formatteddate;
            newdatamap["craft"] = null;
            newdatamap["regularhrs"] = 8;
            newdatamap["payrate"] = 0.0;
            newdatamap["startdate"] = formatteddate;
            newdatamap["starttime"] = null;
            newdatamap["finishdate"] = null;
            newdatamap["finishtime"] = null;
            newdatamap["gldebitacct"] = null;
            newdatamap["glcreditacct"] = null;
            newdatamap["linecost"] = null;
            modalService.show(compositionschema, newdatamap);
        },
        submitLaborTrans: function (schema, datamap) {
            var validationErrors = validationService.validate(schema, schema.displayables, datamap);
            if (validationErrors.length > 0) {
                //interrupting here, can´t be done inside service
                return;
            }

            var jsonString = angular.toJson(datamap);
            var httpParameters = {
                application: "labtrans",
                platform: "web",
                currentSchemaKey: "detail.input.web"
            }
            restService.invokePost("data", "post", httpParameters, jsonString, function () {
                var restParameters = {
                    key: {
                        schemaId: "detail",
                        mode: "input",
                        platform: "web"
                    },
                    SearchDTO: null
                };
                var urlToUse = url("/api/data/labtrans?" + $.param(restParameters));
                $http.get(urlToUse).success(function (data) {
                    modalService.hide();
                    window.location.reload();
                });
            });
        },
        cancelLaborTrans: function () {
            modalService.hide();
        },
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
                event.fields['linecost'] = event.fields['laborcraftrate_.rate'] * event.fields['regularhrs'];
            }
        }
    };
});