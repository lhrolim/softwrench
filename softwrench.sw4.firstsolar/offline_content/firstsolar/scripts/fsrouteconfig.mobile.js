(function (angular) {
    "use strict";
    
    angular.module("softwrench").config(["$stateProvider", function($stateProvider) {

        $stateProvider
            .state("main.firstsolar", { // entry-point state for FS's custom states
                'abstract': true,
                url: "/firstsolar"
            })
            .state("main.firstsolar.userprofile", {
                url: "/myprofile",
                cache: false, // can't cache otherwise the updated profile is not fetched
                views: {
                    "main@main": {
                        templateUrl: "Content/Customers/templates/firstsolar_offline/templates/userprofile.html",
                        controller: "fsUserProfile"
                    }
                }
            });

    }]);

})(angular);