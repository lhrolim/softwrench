
(function () {
    'use strict';

    angular.module('maximo_applications').factory('personService', ['alertService','redirectService', 'applicationService', 'contextService', personService]);

    function personService(alertService, redirectService, applicationService, contextService) {

        var service = {
            afterChangeUsername: afterChangeUsername,
            validatePerson: validatePerson,
            cancelEdition: cancelEdition,
            submitPerson: submitPerson
        };

        return service;

        function cancelEdition() {
            redirectService.redirectToHome();
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
            applicationService.submitData(
                function(response) {
                    var ro = response.resultObject;
                    contextService.loadUserContext(ro);
                },
                function (response) {
                    var res = response;
                },
                {
                    isComposition: false,
                    refresh: true
                });
        }

    }
})();
