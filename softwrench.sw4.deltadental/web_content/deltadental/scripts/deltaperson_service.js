(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('deltadental_.personService', function () {
    return {
        afterChangeAltDisplayname: function (datamap) {
            if (datamap['altdisplayname'] === null) {
                datamap['altperson'] = null;
                datamap['altfirstname'] = null;
                datamap['altlastname'] = null;
                datamap['altphone'] = null;
                datamap['altemail'] = null;
            } else {
                datamap['altfirstname'] = datamap['altperson_.firstname'];
                datamap['altlastname'] = datamap['altperson_.lastname'];
                datamap['altphone'] = datamap['altperson_.phone_.phonenum'];
                datamap['altemail'] = datamap['altperson_.email_.emailaddress'];
            }
        },
    };
});

})(angular);