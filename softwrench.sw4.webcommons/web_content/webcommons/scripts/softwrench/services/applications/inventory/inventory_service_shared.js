(function (angular) {
    "use strict";

    function inventorySharedService() {

        //#region Public methods

        /**
         * TODO: remove when swoff 'invissue creation' is supported
         * @deprecated 
         * @param String application current application name
         * @param Schema schema current schema
         * @param Datamap datamap selected datamap
         * @returns String detail schema's id
         */
        function nextReadOnlyInvIssueDetailSchema(application, schema, datamap) {
            return datamap["issuetype"] === "ISSUE" ? "viewinvissuedetail" : "viewinvreturndetail";
        }

        /**
         * @param String application current application name
         * @param Schema schema current schema
         * @param Datamap datamap selected datamap
         * @returns String detail schema's id
         */
        function nextInvIssueDetailSchema(application, schema, datamap) {
            //Logic to determine whether the record is an ISSUE
            //and whether all of the issued items have been returned
            if (datamap["issuetype"] !== "ISSUE") {
                //if it´s not an issue redirecting to return screen
                return "viewinvreturndetail";
            }

            //Sets qtyreturned to 0 if null
            //Parses the qtyreturned if its in a strng format
            var qtyreturned = 0;
            if (angular.isString(["qtyreturned"])) {
                qtyreturned = parseInt(datamap["qtyreturned"]);
            } else if (datamap["qtyreturned"] != null) {
                qtyreturned = datamap["qtyreturned"];
            }

            //For an issue, the quantity will be a negative number, representing the # of items issued
            //The below if statement will add the positive quantityreturned to the negative quantity.
            //If the result is negative, then are still items to be returned
            if (qtyreturned + datamap["quantity"] >= 0) {
                //If all of the items have been returned, show the viewdetail page for 'ISSUE' records
                return "viewinvissuedetail";
            }

            if (qtyreturned + datamap["quantity"] !== -1) {
                //There are still items to be returned
                return application === "invreturn" ? "editinvreturndetail" : "editinvissuedetail";
            }

            return null;
        }

        //#endregion

        //#region Service Instance
        var service = {
            nextInvIssueDetailSchema: nextInvIssueDetailSchema,
            nextReadOnlyInvIssueDetailSchema: nextReadOnlyInvIssueDetailSchema
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("webcommons_services").factory("inventorySharedService", [inventorySharedService]);
    //#endregion

})(angular);