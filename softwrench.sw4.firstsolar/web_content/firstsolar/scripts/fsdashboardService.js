
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

            if (datamap["#buildcomplete"]) {
                return {
                    bgcolor: '#39b54a', // green,
                    forecolor: 'white'
                }
            }

            if (schema.schemaId === "workpackagebuilddash") {
                const today = moment().startOf("day");
                const reportDay = moment(datamap["reportdate"]).startOf("day");
                const diffInDays = today.diff(reportDay, "days");
                if (diffInDays >= 6 && diffInDays <= 25) {
                    return {
                        bgcolor: '#39b54a', // green,
                        forecolor: 'white'
                    }
                }

                return {
                    bgcolor: '#f2d935', // yellow,
                    forecolor: 'black'
                }
            }

            if (schema.schemaId === "workpackagebuild290dash") {
                return {
                    bgcolor: '#f65752', // red,
                    forecolor: 'white'
                }
            }
        }

        incomingDashClick(datamap, schema) {
            this.redirectService.goToApplicationView("_WorkPackage", "newdetail", "input", null, null, { wonum: datamap["wonum"], workorderid: datamap["workorderid"] });
        }

        buildDashClick(datamap, schema) {
            this.redirectService.goToApplicationView("_WorkPackage", "adetail", "input", null, { id: datamap["#id"] });
        }


    }


    fsDashboardService["$inject"] = ["redirectService"];

    angular.module("sw_layout").service("fsDashboardService", fsDashboardService);

})(angular, moment);