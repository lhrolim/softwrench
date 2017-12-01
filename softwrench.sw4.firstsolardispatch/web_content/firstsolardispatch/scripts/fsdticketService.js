(function (angular) {
    "use strict";

    let formatPhone;

    class fsdTicketService {
        constructor(crudContextHolderService) {
            this.crudContextHolderService = crudContextHolderService;

            formatPhone = function(phone) {
                if (!phone || phone.length !== 10) return phone;
                return `(${phone.substr(0, 3)}) ${phone.substr(3, 3)}-${phone.substr(6, 4)}`;
            }
        }

        siteSelected(event) {
            const fields = event.fields;
            if (!fields["site_"] || !fields["gfedid"]) {
                return;
            }
            const dm = this.crudContextHolderService.rootDataMap();

            if (fields["site_.singlelineaddress"]) {
                dm["siteaddress"] = fields["site_.singlelineaddress"];
            } else if (fields["site_.address"]) {
                const address = fields["site_.address"];
                const city = fields["site_.city"] ? ` ${fields["site_.city"]}` : "";
                const state = fields["site_.state"] ? ` ${fields["site_.state"]}` : "";
                const postalcode = fields["site_.postalcode"] ? ` ${fields["site_.postalcode"]}` : "";
                const separator = city || state || postalcode ? "," : "";
                dm["siteaddress"] = `${address}${separator}${city}${state}${postalcode}`;
            } else {
                dm["siteaddress"] = null;
            }

            dm["gpslatitude"] = fields["site_.gpslatitude"];
            dm["gpslongitude"] = fields["site_.gpslongitude"];
            dm["sitecontact"] = fields["site_.sitecontact"];
            dm["sitecontactphone"] = formatPhone(fields["site_.sitecontactphone"]);
            dm["maintenaceprovider"] = fields["site_.maintenaceprovider"];
            dm["supportphone"] = formatPhone(fields["site_.supportphone"]);
            dm["primarycontact"] = fields["site_.primarycontact"];
            dm["primarycontactphone"] = formatPhone(fields["site_.primarycontactphone"]);
            dm["escalationcontact"] = fields["site_.escalationcontact"];
            dm["escalationcontactphone"] = formatPhone(fields["site_.escalationcontactphone"]);
            dm["site_.siteid"] = fields["site_.siteid"];
            dm["site_.locationprefix"] = fields["site_.locationprefix"];
        }

        filterStatus(item) {
            const value = item.value;
            const dm = this.crudContextHolderService.rootDataMap();
            
            if (!dm["id"]) {
                return value === "DRAFT";
            }
            const currentStatus = dm["#originalstatus"];
            if (currentStatus === value) {
                return true;
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

            return false;

        }
    }

    fsdTicketService.$inject = ["crudContextHolderService"];

    angular.module("firstsolardispatch").clientfactory("fsdTicketService", fsdTicketService);

})(angular);