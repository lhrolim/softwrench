﻿
(function (angular, moment) {
    "use strict";

    class fsDashboardService {

        constructor(redirectService) {
            this.redirectService = redirectService;
        }

        getColorScheme(column, datamap, schema) {
            if (column != null) {
                return null;
            }

            if (schema.schemaId === "workpackagebuild290dash" && !datamap["#buildcomplete"]) {
                return {
                    forecolor: "#f65752" // red
                }
            }
        }

        incomingDashClick(datamap, schema) {
            this.redirectService.goToApplicationView("_WorkPackage", "newdetail", "input", null, null, { wonum: datamap["wonum"], workorderid: datamap["workorderid"] });
        }

        buildDashClick(datamap, schema) {
            this.redirectService.goToApplicationView("_WorkPackage", "adetail", "input", null, { id: datamap["#id"] });
        }

        techDashClick(datamap, schema) {
            this.redirectService.goToApplicationView("workorder", "detail", "input", null, { id: datamap["workorder_.workorderid"] });
        }

        techwoDashClick(datamap, schema) {
            this.redirectService.goToApplicationView("workorder", "detail", "input", null, { id: datamap["workorderid"] });
        }


    }


    fsDashboardService["$inject"] = ["redirectService"];

    angular.module("sw_layout").service("fsDashboardService", fsDashboardService);

})(angular, moment);