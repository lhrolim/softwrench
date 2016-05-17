
(function (angular) {
    'use strict';

    angular.module('maximo_applications').factory('personService', ['$rootScope',"$log", 'alertService', 'redirectService', 'applicationService', 'contextService', 'crudContextHolderService', 'dispatcherService', personService]);

    function personService($rootScope,$log, alertService, redirectService, applicationService, contextService, crudContextHolderService, dispatcherService) {

        var service = {
            afterChangeUsername: afterChangeUsername,
            validatePerson: validatePerson,
            cancelEdition: cancelEdition,
            submitPerson: submitPerson,
            loadPhone: loadPhone,
            loadEmail:loadEmail
        };

        return service;

        function loadPhone() {
            var dm = crudContextHolderService.rootDataMap("#modal");
            if (dm.phoneid) {
                //edition
                dm["#originalphonenum"] = dm.phonenum;
                dm["#originaltype"] = dm.type;
                dm["originalprimary"] = dm.isprimary;

                $log.get("personService#loadPhone", ["phone"]).debug("setting original phone data: {0}{1}{2}".format(dm.phonenum, dm.type, dm.isprimary));
            }
        };

        function loadEmail() {
            var dm = crudContextHolderService.rootDataMap("#modal");
            if (dm.emailid) {
                //edition
                dm["#originalemailaddress"] = dm.emailaddress;
                dm["#originaltype"] = dm.type;
                dm["originalprimary"] = dm.isprimary;
                $log.get("personService#loadEmail", ["email"]).debug("setting original email data: {0}{1}{2}".format(dm.emailaddress, dm.type, dm.isprimary));
            }
        }


        function cancelEdition() {
            var schema = crudContextHolderService.currentSchema();
            if (schema.schemaId === 'myprofiledetail') {
                return redirectService.redirectToHome();
            }
            return redirectService.goToApplication("Person", "list");
        };

        function afterChangeUsername(datamap) {
            if (!nullOrEmpty(datamap.fields["#personid"])) {
                datamap.fields["personid"] = datamap.fields["#personid"];
            }
        };

        function validatePerson(schema, datamap) {
            if (!datamap["#password"]) {
                return true;
            }
            var errors = [];
            if (datamap["#password"] != datamap["#retypepassword"]) {
                errors.push("Passwords do not match");
            }

            return errors;
        }

        function submitPerson(schema, datamap) {
            var currentSchema = crudContextHolderService.currentSchema();
            applicationService.save().then(function (response) {
                var ro = response.resultObject;
                if (currentSchema.schemaId === "myprofiledetail" && ro) {
                    contextService.loadUserContext(ro);
                }
            });
        }

    }
})(angular);
