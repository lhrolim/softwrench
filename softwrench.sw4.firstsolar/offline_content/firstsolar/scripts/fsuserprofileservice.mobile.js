(function (angular, _) {
    "use strict";

    function fsUserProfileService(routeService, securityService, associationConstants, $q) {
        //#region Utils
        const config = {
            facilities: {
                available: "sync.availablefacilities",
                selected: "sync.facilities"
            }
        };
        //#endregion

        //#region Public methods

        /**
         * Redirects the user to first solar's userprofile screen.
         * 
         * @param {Object} menuleaf clicked menu item
         * @returns {Promise<$state>} 
         */
        function goToMyProfile(menuleaf) {
            return routeService.go("main.firstsolar.userprofile");
        }

        /**
         * Builds a viewmodel-friendly object from the curent user.
         * 
         * @returns {Object} 
         */
        function getUserProfileViewModel() {
            const user = securityService.currentFullUser();

            const userVm = {
                personid: user["PersonId"],
                username: user["UserName"],
                siteid: user["SiteId"],
                orgid: user["OrgId"]
            };

            if (!user.properties || !user.properties[config.facilities.available] || !user.properties[config.facilities.selected]) {
                return userVm;
            }

            const availableFacilities = user.properties[config.facilities.available].map(f => f.toUpperCase());
            const selectedFacilities = user.properties[config.facilities.selected].map(f => f.toUpperCase());

            const facilityOptions = _.chain(availableFacilities)
                .map(f => new associationConstants.Option(f).setChecked(_.contains(selectedFacilities, f)))
                .sortBy("value")
                .value();

            userVm.facilities = {
                options: facilityOptions,
                selected: selectedFacilities.join(";")
            }
            
            return userVm;
        }

        /**
         * Updates the selected facilities for the user.
         * 
         * @param {Object} userViewModel 
         * @returns {Promise<Void>} 
         */
        function saveUserProfile(userViewModel) {
            const selectedFacilities = userViewModel.facilities.options.filter(f => f.checked).map(f => f.value);
            const properties = { [config.facilities.selected]: selectedFacilities };
            return $q.when(securityService.updateCurrentUserProperties(properties, true));
        }

        //#endregion

        //#region Service Instance
        const service = {
            goToMyProfile,
            getUserProfileViewModel,
            saveUserProfile
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services").factory("fsUserProfileService", ["routeService", "securityService", "associationConstants", "$q", fsUserProfileService]);

    //#endregion

})(angular, _);