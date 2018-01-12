(function (angular) {
    "use strict";

    let formatPhone;

    class fsdTicketService {
        constructor(crudContextHolderService, restService, configurationService, applicationService, alertService, userService) {
            this.crudContextHolderService = crudContextHolderService;
            this.restService = restService;
            this.configurationService = configurationService;
            this.applicationService = applicationService;
            this.alertService = alertService;
            this.userService = userService;

            formatPhone = function (phone) {
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
            dm["supportemail"] = fields["site_.supportemail"];
            dm["primarycontact"] = fields["site_.primarycontact"];
            dm["primarycontactphone"] = formatPhone(fields["site_.primarycontactphone"]);
            dm["primarycontactemail"] = fields["site_.primarycontactemail"];
            dm["escalationcontact"] = fields["site_.escalationcontact"];
            dm["escalationcontactphone"] = formatPhone(fields["site_.escalationcontactphone"]);
            dm["escalationcontactemail"] = fields["site_.escalationcontactemail"];


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

        shouldShowCreateCommand() {
            const dm = this.crudContextHolderService.rootDataMap();
            return dm["status"] === "ACCEPTED" && this.userService.isSysAdmin();
        }

        doCreateWorkOrder(serverurl,dm, hashsignature) {
            const parameters = {
                _customurl: serverurl,
            };

            const jsonOb = {
                json: dm,
                messageToSign: dm.id,
                hashSignature: hashsignature
            }


            return this.restService.post("FSDBackend", "CreateWorkorders", parameters, jsonOb).catch(e => {
                this.alertService.alert(
                    "Could not connect to the server. Please check your network connection or contact support");
            });
        }

        createWorkOrder() {
            const serverurl = this.configurationService.getConfigurationValue("/FirstSolarDispatch/fsendpointurl");
            if (!serverurl) {
                this.alertService("Please fill in /FirstSolarDispatch/fsendpointurl at the configuration application");
                return false;
            }
            const dm = this.crudContextHolderService.rootDataMap();
            

            return this.restService.get("Security", "GenerateHashedKey", { message: dm.id }).then(result => {
                return this.doCreateWorkOrder(serverurl, dm, result.data);
            });
            

        }



    }




    fsdTicketService.$inject = ["crudContextHolderService", "restService", "configurationService", "applicationService", "alertService", "userService"];

    angular.module("sw_layout").service("fsdTicketService", fsdTicketService);

})(angular);