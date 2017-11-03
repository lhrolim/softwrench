(function (angular) {
    "use strict";


    class fsdTicketService {
        constructor(crudContextHolderService) {
            this.crudContextHolderService = crudContextHolderService;
        }

        siteSelected(event) {
            const fields = event.fields;
            if (!fields["site_"] || !fields["gfedid"]) {
                return;
            }
            const dm = this.crudContextHolderService.rootDataMap();
            const address = fields["site_.address"];
            const city = fields["site_.city"] ? ` ${fields["site_.city"]}` : "";
            const state = fields["site_.state"] ? ` ${fields["site_.state"]}` : "";
            const postalcode = fields["site_.postalcode"] ? ` ${fields["site_.postalcode"]}` : "";
            const separator = city || state || postalcode ? "," : "";
            dm["siteaddress"] = `${address}${separator}${city}${state}${postalcode}`;
            dm["gpslatitude"] = fields["site_.gpslatitude"];
            dm["gpslongitude"] = fields["site_.gpslongitude"];
            dm["sitecontact"] = fields["site_.sitecontact"];
            dm["sitecontactphone"] = fields["site_.sitecontactphone"];
            dm["supportphone"] = fields["site_.supportphone"];
            dm["primarycontact"] = fields["site_.primarycontact"];
            dm["primarycontactphone"] = fields["site_.primarycontactphone"];
            dm["escalationcontact"] = fields["site_.escalationcontact"];
            dm["escalationcontactphone"] = fields["site_.escalationcontactphone"];
            dm["site_.siteid"] = fields["site_.siteid"];
            dm["site_.locationprefix"] = fields["site_.locationprefix"];
        }

        filterStatus(item) {
            const value = item.value;
            const dm = this.crudContextHolderService.rootDataMap();
            
            if (!dm["id"]) {
                return value === "DISPATCHED";
            }
            const currentStatus = dm["#originalstatus"];
            if (currentStatus === value) {
                return true;
            }

            if (currentStatus === "REJECTED") {
                return false;
            }
            if (currentStatus === "ACCEPTED") {
                return value === "ARRIVED";
            }
            if (currentStatus === "ARRIVED") {
                return value === "RESOLVED";
            }

            if (currentStatus === "DISPATCHED") {
                return value.equalsAny("ACCEPTED", "REJECTED");
            }

            return value === "DISPATCHED";

        }
    }

    fsdTicketService.$inject = ["crudContextHolderService"];

    angular.module("firstsolardispatch").clientfactory("fsdTicketService", fsdTicketService);

})(angular);