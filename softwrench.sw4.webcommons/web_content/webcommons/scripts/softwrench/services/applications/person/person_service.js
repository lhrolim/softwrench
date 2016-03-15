
(function () {
    'use strict';

    angular.module('maximo_applications').factory('personService', ['$rootScope', 'alertService', 'redirectService', 'applicationService', 'contextService', 'crudContextHolderService', 'dispatcherService', personService]);

    function personService($rootScope, alertService, redirectService, applicationService, contextService, crudContextHolderService, dispatcherService) {

        var service = {
            afterChangeUsername: afterChangeUsername,
            validatePerson: validatePerson,
            cancelEdition: cancelEdition,
            submitPerson: submitPerson
        };

        return service;

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
})();
