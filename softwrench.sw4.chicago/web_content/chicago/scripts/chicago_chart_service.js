(function (angular) {
    "use strict";

    function chicagoChartService(searchService,formatService,fileService) {
        //#region Utils

        //#endregion

        //#region Public methods

        function dailyopenedTicketsClicked(chartClickParams) {
            const originalDaySelected = chartClickParams.fieldValue;

            const beginDate = new Date();
            beginDate.setDate(originalDaySelected);
            const beginDateString = formatService.formatDate(beginDate, "dd/MM/yyyy");

            const searchData = {
                ["creationdate"]: beginDateString,
                ["creationdate_end"]: beginDateString,
            };
            const searchOperator = {
                ["creationdate"]: searchService.getSearchOperationById("BTW")
            };
            return searchService.buildSearchDTO(searchData, null, searchOperator);
        }

        function dailyTicketCount() {
            fileService.download(url("/ChicagoExcel/GetDepartmentCount"));
        }

        //#endregion

        //#region Service Instance
        const service = {
            dailyopenedTicketsClicked,
            dailyTicketCount
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module('chicago')
      .clientfactory('chicagoChartService', ["searchService", "formatService","fileService", chicagoChartService]);

    //#endregion

})(angular);