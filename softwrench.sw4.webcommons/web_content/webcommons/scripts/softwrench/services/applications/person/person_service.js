
(function (angular) {
    'use strict';

    angular.module('maximo_applications').service('personService', ['$rootScope', "$log", 'alertService', 'redirectService', 'applicationService',
        'contextService', 'crudContextHolderService', 'historyService', 'restService', '$q', personService]);
    
    function personService($rootScope, $log, alertService, redirectService, applicationService, contextService, crudContextHolderService, historyService, restService, $q) {


        function handlePrimaryForCreation(compositionDatamap, type) {
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

        //afterchange
        function afterChangeUsername(event) {
            const datamap = event.fields;
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

            return applicationService.save().then(function (response) {

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
                return response;
            });
        }

        //afterchange
        function onSiteSelected(event) {
            const datamap = event.fields;
            if (!datamap["locationorg"]) {
                datamap["locationorg"] = datamap["site_.orgid"];
            }
        }

        //afterchange
        function onOrganizationSelected(event) {
            const datamap = event.fields;
            if (datamap["locationorg"] !== datamap["site_.orgid"]) {
                datamap["locationsite"] = null;
            }
        }


        const forcePasswordReset = () => {
            const dm = crudContextHolderService.rootDataMap();
            const username = dm["personid"];
            alertService.confirm(`are you sure you want to prompt user ${username} to change his password`).then(r => {

                restService.postPromise("UserSetupWebApi", "ForceResetPassword", { username }).then(r => {
                    alertService.success("User will be prompted to reset his password upon next login");
                });
            });
        }

        const unlock = () => {
            const dm = crudContextHolderService.rootDataMap();
            const username = dm["personid"];

            alertService.confirm(`are you sure you want to unlock user ${username}`).then(r => {
                restService.postPromise("UserSetupWebApi", "UnLock", { username }).then(r => {
                    alertService.success("User unlocked. User will be prompted to reset their password next time they login");
                    dm["locked"] = false;
                });
            });


        }

        const activate = () => {
            const dm = crudContextHolderService.rootDataMap();
            const username = dm["personid"];

            const userid = dm["#userid"];


            alertService.confirm(`are you sure you want to activate user ${username}`).then(r => {
                restService.postPromise("UserSetupWebApi", "Activate", { userid }).then(response => {
                    alertService.success("User Activated");
                    dm["isactive"] = true;
                    dm["activationlink"] = response.data;
                });
            });
        }

        
        const inactivate = () => {
            const dm = crudContextHolderService.rootDataMap();
            const username = dm["personid"];
            const userid = dm["#userid"];

            alertService.confirm(`are you sure you want to inactivate user ${username}`).then(r => {
                restService.postPromise("UserSetupWebApi", "Inactivate", { userid }).then(r => {
                    alertService.success("User Inactivated");
                    dm["isactive"] = false;
                });
            });
        }


        const service = {
            afterChangeUsername,
            activate,
            inactivate,
            validatePerson,
            cancelEdition,
            forcePasswordReset,
            submitPerson,
            loadPhone,
            loadEmail,
            onSiteSelected,
            onOrganizationSelected,
            unlock
        };
        return service;

    }
})(angular);
