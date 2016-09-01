
(function (angular) {
    'use strict';

    angular.module('maximo_applications').factory('personService', ['$rootScope',"$log", 'alertService', 'redirectService', 'applicationService', 'contextService', 'crudContextHolderService', 'historyService','$q', personService]);

    function personService($rootScope, $log, alertService, redirectService, applicationService, contextService, crudContextHolderService, historyService,$q) {
     

        function handlePrimaryForCreation(compositionDatamap,type) {
            //phone creation
            const personData = crudContextHolderService.rootDataMap();
            if (!personData[type] || personData[type].length === 0) {
                compositionDatamap["isprimary"] = true;
                compositionDatamap["originalprimary"] = true;
            }
        }

        function loadPhone() {
            const dm = crudContextHolderService.rootDataMap("#modal");
            if (dm.phoneid) {
                //edition
                dm["#originalphonenum"] = dm.phonenum;
                dm["#originaltype"] = dm.type;
                dm["originalprimary"] = dm.isprimary;
                $log.get("personService#loadPhone", ["phone"]).debug("setting original phone data: {0}{1}{2}".format(dm.phonenum, dm.type, dm.isprimary));
            } else {
                handlePrimaryForCreation(dm, "phone_");
            }
        };

        function loadEmail() {
            const dm = crudContextHolderService.rootDataMap("#modal");
            if (dm.emailid) {
                //edition
                dm["#originalemailaddress"] = dm.emailaddress;
                dm["#originaltype"] = dm.type;
                dm["originalprimary"] = dm.isprimary;
                $log.get("personService#loadEmail", ["email"]).debug("setting original email data: {0}{1}{2}".format(dm.emailaddress, dm.type, dm.isprimary));
            } else {
                handlePrimaryForCreation(dm, "email_");
            }
        }


        function cancelEdition() {
            const schema = crudContextHolderService.currentSchema();
            if (schema.schemaId === 'myprofiledetail') {
                const redirected = historyService.redirectOneBack();
                if (!redirected) {
                    return redirectService.redirectToHome();
                }
                return $q.when();
            }
            return redirectService.goToApplication("Person", "list");
        };

        function afterChangeUsername(datamap) {
            if (!nullOrEmpty(datamap["#personid"])) {
                datamap["personid"] = datamap["#personid"];
            }
        };

        function validatePerson(schema, datamap) {
            if (!datamap["#password"]) {
                return true;
            }
            const errors = [];
            if (datamap["#password"] != datamap["#retypepassword"]) {
                errors.push("Passwords do not match");
            }

            return errors;
        }

        function submitPerson(schema, datamap) {
            var currentSchema = crudContextHolderService.currentSchema();
            const fields = datamap;

            applicationService.save().then(function (response) {

                if (currentSchema.schemaId === "myprofiledetail") {
                    const ro = response.resultObject;
                    let updatedUser = angular.copy(fields);
                    updatedUser.orgid = fields.locationorg;
                    updatedUser.siteid = fields.locationsite;
                    const primaryEmail = fields.email_.find(e=> e.isprimary);
                    const primaryPhone = fields.phone_.find(e=> e.isprimary);
                    if (primaryEmail) {
                        updatedUser.email = primaryEmail.emailaddress;
                    }
                    if (primaryPhone) {
                        updatedUser.phone = primaryPhone.phonenum;
                    }

                    updatedUser.maximoPersonId = fields.personid;
                    updatedUser = angular.extend(updatedUser, ro.fields);
                    updatedUser.firstName = fields.firstname;
                    updatedUser.lastName = fields.lastname;


                    contextService.loadUserContext(updatedUser);

                }
            });
        }

        function onSiteSelected(event) {
            const datamap = event.fields;
            if (!datamap["locationorg"]) {
                datamap["locationorg"] = datamap["site_.orgid"];
            }
        }

        function onOrganizationSelected(event) {
            const datamap = event.fields;
            if (datamap["locationorg"] !== datamap["site_.orgid"]) {
                datamap["locationsite"] = null;
            }
        }

        const service = {
            afterChangeUsername,
            validatePerson,
            cancelEdition,
            submitPerson,
            loadPhone,
            loadEmail,
            onSiteSelected,
            onOrganizationSelected
        };
        return service;

    }
})(angular);
