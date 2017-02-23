var app = angular.module('sw_layout');

app.factory('srservice', function ($http, alertService, fieldService, $rootScope,contextService) {

    "ngInject";

    return {
        //This service is to add the new field ACTION in Service Request details for resolved tickets
     

        afterITCAssetChange: function (event) {
            // Clean User-Personal Asset
            if (event.fields['assetnum'] != null) {
                event.fields['assetnum'] = '$null$ignorewatch';
            }
            //event.fields['assetnum'] = null;

        },

        afterUserAssetChange: function (event) {
            // Clean ITC-Responsible Asset
            if (event.fields['itcassetnum'] != null) {
                event.fields['itcassetnum'] = '$null$ignorewatch';
            }
            //event.fields['itcassetnum'] = null;
        },

        /// <summary>
        /// if the afected person is different from the current user, then we should disable the User-Personal Asset combobox, since it would make no sense anymore
        /// </summary>
        /// <param name="event"></param>
        afterChangeaffectedperson: function (event) {
            var userData =contextService.getUserData();
            // Clean ITC-Responsible Asset
            if (userData.maximoPersonId != event.fields['affectedperson']) {
                $rootScope.$broadcast('sw_block_association', "asset_");
                //block
            } else {
                $rootScope.$broadcast('sw_unblock_association', "asset_");
            }
        },

        afterPhoneAffectedDeviceChange: function (event) {
            // Clear Phone Location and Affected Phone case Affected Device is not 'Cisco-IP-Phone'
            if (event.fields['affectedDevice'] != 'Cisco-IP-Phone') {
                event.fields['phonepluspcustomer'] = '$ignorewatch';
                event.fields['assetnum'] = '$ignorewatch';
            }            
        }
    }; 
});