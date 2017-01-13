(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('srService', ["alertService", "redirectService", function (alertService, redirectService) {

        return {

            //beforechange
        beforeChangeLocation: function (event) {
            if (event.fields['assetnum']== null) {
                //if no asset is selected we can proceed.
                return true;
            }
            if (event.oldValue == null && event.fields['asset_.location']== event.newValue) {
                //if the location was null, and now it´s changing to the asset location, proceed. It might be 
                //due to the AfterChangeAsset callback.
                return true;
            }


            alertService.confirm("The location you have entered does not contain the current asset. Would you like to remove the current asset from the ticket?").then(function () {
                event.fields['assetnum'] = null;
                event.continue();
            }, function () {
                event.interrupt();
            });
        },

        //beforechange
        beforeChangeStatus: function (event) {
            if (event.fields['owner'] == null || event.fields['status']!= "NEW") {
                return true;
            }

            alertService.confirm("Changing the status to new would imply in removing the owner of this Service Request. Proceeed?").then(function () {
                event.fields['owner']= null;
                event.continue();
            }, function () {
                event.interrupt();
            });
        },

        //afterchange
        afterChangeAsset: function (event) {
            const fields = event.fields;
            if (fields['location'] == null && fields.assetnum != null) {
                const location = fields['asset_.location'];
                fields['location'] = location;
            }
        },

        //afterchange
        afterchangeowner: function (event) {
            if (event.fields['owner']== null) {
                return;
            }
            if (event.fields['owner'] == ' ') {
                event.fields['owner']= null;
                return;
            }
            if (event.fields['status'] == 'NEW' || event.fields['#originalstatus']== "NEW") {
                alertService.alert("Owner Group Field will be disabled if the Owner is selected.");
                return;
            }
            
        },

        //afterchange
        afterchangeownergroup: function (event) {
            
            if (event.fields['ownergroup']== null) {
                return;
            }
            if (event.fields['ownergroup']== ' ') {
                event.fields['ownergroup']= null;
                return;
            }
            if (event.fields['status']== 'NEW') {
                alertService.alert("Owner Field will be disabled if the Owner Group is selected.");
                return;
            }
            
            
        },

        //beforechange
        beforechangeownergroup: function (event) {
            if (event.fields['owner']!= null) {
                alertService.alert("You may select an Owner or an Owner Group; not both");
            }
        },

        startRelatedSRCreation: function (schema, datamap) {
            var localDatamap = datamap;
            var relatedDatamap = {
                'status': "NEW",
                '#relatedrecord_recordkey': localDatamap[schema.userIdFieldName]
            };
            var clonedFields = [
                "affectedemail", "affectedperson", "affectedphone", "assetnum", "assetditeid",
                "cinum", "classstructureid", "commodity", "commoditygroup",
                "description", "longdescription_.ldtext", "ld_.ldtext",
                "glaccount",
                "location",
                "orgid",
                "reportedby", "reportedemail", "reportedphone", "reportedpriority",
                "siteid", "source",
                "virtualenv"
            ];
            clonedFields.forEach(function(field) {
                if (!localDatamap.hasOwnProperty(field)) return;
                relatedDatamap[field] = localDatamap[field];
            });

            var params = {
                postProcessFn: function(data) {
                    angular.forEach(relatedDatamap, function(value, key) {
                        data.resultObject[key] = value;
                    });
                }
            };

            return redirectService.goToApplication(schema.applicationName, "newdetail", params, relatedDatamap);
        }

    };

}]);

})(angular);