(function (angular) {
    "use strict";

    function commandBarDelegate() {
        //#region Utils

        //#endregion

        //#region Public methods

        /**
         * Positions the commandBarElement in the FAB position.
         * 
         * @param {$(DOMNode)} commandBarElement 
         * @param {Number} scrollPosition optional current scroll handler's position.top
         */
        function positionFabCommandBar(commandBarElement, scrollPosition) {
            const toolbarPrimary = $(".bar-header.bar-positive:visible").outerHeight(true);
            const toolbarSecondary = $(".bar-subheader.bar-dark:visible").outerHeight(true);
            const headerTitle = $(".crud-details .crud-title:visible").outerHeight(true);
            const headerDescription = $(".crud-details .crud-description:visible").outerHeight(true);
            const componetHeights = toolbarPrimary + toolbarSecondary + headerTitle + headerDescription;
            const top = angular.isNumber(scrollPosition) && scrollPosition >= 0 ? scrollPosition : 0;
            const windowHeight = $(window).height();
            const offset = (windowHeight - componetHeights - 110) + top;

            $(commandBarElement).css("top", offset);
        }

        //#endregion

        //#region Service Instance
        const service = {
            positionFabCommandBar
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services").factory("commandBarDelegate", [commandBarDelegate]);

    //#endregion

})(angular);