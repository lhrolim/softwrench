(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('deltadental_.personService', function () {
    return {
        afterChangeAltDisplayname: function (datamap) {
            if (datamap.fields['altdisplayname'] === null) {
                datamap.fields['altperson'] = null;
                datamap.fields['altfirstname'] = null;
                datamap.fields['altlastname'] = null;
                datamap.fields['altphone'] = null;
                datamap.fields['altemail'] = null;
            } else {
                datamap.fields['altfirstname'] = datamap.fields['altperson_.firstname'];
                datamap.fields['altlastname'] = datamap.fields['altperson_.lastname'];
                datamap.fields['altphone'] = datamap.fields['altperson_.phone_.phonenum'];
                datamap.fields['altemail'] = datamap.fields['altperson_.email_.emailaddress'];
            }
        },
    };
});

})(angular);