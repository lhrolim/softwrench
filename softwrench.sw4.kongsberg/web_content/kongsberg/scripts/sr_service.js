﻿kongsbergmod.clientfactory('srService', function (srService) {

    return {
        afterchangeowner: function (event) {
            srService.afterchangeowner(event);
        },

        afterchangeownergroup: function (event) {

            if (event.fields['ownergroup'] == null) {
                return;
            }
            if (event.fields['ownergroup'] == ' ') {
                event.fields['ownergroup'] = null;
                return;
            }
            if (event.fields['status'] == 'NEW') {
                event.fields['status'] = 'QUEUED';
                return;
            }
        },
    };

});