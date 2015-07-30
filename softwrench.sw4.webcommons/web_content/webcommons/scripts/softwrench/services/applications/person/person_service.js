
(function () {
    'use strict';

    angular.module('maximo_applications').factory('personService', ['alertService', personService]);

    function personService(alertService) {

        var service = {
            afterChangeUsername: afterChangeUsername,
            validatePerson: validatePerson,
            cancelPersonDetail: cancelPersonDetail
        };

        return service;

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
        };

    }
})();
