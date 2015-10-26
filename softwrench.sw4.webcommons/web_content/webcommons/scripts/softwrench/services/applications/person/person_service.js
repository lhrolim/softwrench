﻿
(function () {
    'use strict';

    angular.module('maximo_applications').factory('personService', ['alertService','redirectService', personService]);

    function personService(alertService, redirectService) {

        var service = {
            afterChangeUsername: afterChangeUsername,
            validatePerson: validatePerson,
            cancelEdition:cancelEdition
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

    }
})();