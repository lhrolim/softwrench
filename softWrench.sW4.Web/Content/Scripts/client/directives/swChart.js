(function (angular, app) {
    "use strict";

    app.directive("swChart", ["contextService", "chartService", "$log", "redirectService", "searchService", "$timeout", function (contextService, chartService, $log, redirectService, searchService, $timeout) {

        return {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/directives/swChart.html"),
            scope: {
                chartType: "=", // visualization type
                data: "=", // fetched data to display in chart form (formatted to fit devexpress's charts's expected form)
                options: "=", // layout options (see devexpress layout options)
                panel: "=" // DashboardGraphicPanel entity
            },
            link: function (scope, element, attrs) {
                scope.vm = { };
                scope.vm.labeloptions = {};

                function goToFilteredGrid(application, fieldName, fieldValue) {
                    var appName = application.equalsIc("sr") ? "servicerequest" : application;

                    // open: search for 'not closed' --> !C('CLOSE')
                    // others: search for 'is not equal to all other entries' --> !=(<comma_separated_other_entries_values>)
                    // regular: search for 'is equal to value' --> =(fieldValue)
                    var searchDataValue, searchOperation;
                    if (fieldValue.equalsIc("open")) {
                        searchDataValue = "CLOSE";
                        searchOperation = searchService.getSearchOperationBySymbol("!C");

                    } else if (fieldValue.equalsIc("others")) {
                        searchDataValue = scope.data
                                                .map(function (e) { return e.entry.fieldValue; })
                                                .filter(function (e) { return !e.equalsIc("others"); })
                                                .join(",");
                        searchOperation = searchService.getSearchOperationBySymbol("!=");

                    } else {
                        searchDataValue = fieldValue.equalsIc("null") ? "nullor:" : fieldValue;
                        searchOperation = searchService.getSearchOperationBySymbol("=");
                    }

                    var searchData = {};
                    searchData[fieldName] = searchDataValue;
                    var searchOperator = {};
                    searchOperator[fieldName] = searchOperation;

                    var searchDTO = searchService.buildSearchDTO(searchData, null, searchOperator);

                    return redirectService.goToApplication(appName, "list", { 'SearchDTO' : searchDTO  });
                }

                //#region user insteraction
                function onChartClick(event) {

                    var application = scope.panel.configurationDictionary.application;
                    var fieldName = scope.panel.configurationDictionary.field;
                    var target = event.target;
                    var fieldLabel = angular.isString(target) || angular.isNumber(target) // clicked on legend
                                        ? target
                                        // clicked on chart point
                                        : target.argument;
                    // get actual value from label
                    var fieldValue = scope.data.find(function(e) { return e.entry.fieldLabel === fieldLabel; }).entry.fieldValue;

                    var log = $log.get("swChart#onChartClick", ["chart", "dashboard"]);
                    log.debug("clicked: [application: {0}, fieldName: {1}, fieldValue: {2}, fieldLabel: {3}]".format(application, fieldName, fieldValue, fieldLabel));

                    goToFilteredGrid(application, fieldName, fieldValue);
                }
                //#endregion

                function init() {
                    //get the default options
                    var chartOptions = chartService.getChartOptions(scope.chartType, scope.options, scope.data);

                    //launch the correct chart
                    switch (scope.chartType) {
                        case "dxChart":
                        case "swRecordCountChart":
                        case "swRecordCountRotatedChart":
                        case "swRecordCountLineChart":
                        case "swRecordTrends":
                            scope.chart = $(".sw.chart", element).dxChart(chartOptions).dxChart("instance");
                            break;
                        case "dxCircularGauge":
                        case "swCircularGauge":
                        case "swRecordCountGauge":
                            scope.chart = $(".sw.chart", element).dxCircularGauge(chartOptions).dxCircularGauge("instance");
                            break;
                        case "swLabel":
                            scope.labeloptions = chartOptions;
                            break;
                        case "dxLinearGauge":
                        case "swLinearGauge":
                            scope.chart = $(".sw.chart", element).dxLinearGauge(chartOptions).dxLinearGauge("instance");
                            break;
                        case "dxMap":
                        case "swRecordCountMap":
                            scope.chart = $(".sw.chart", element).dxVectorMap(chartOptions).dxVectorMap("instance");
                            break;
                        case "dxPie":
                        case "swRecordCountPie":
                            scope.chart = $(".sw.chart", element).dxPieChart(chartOptions).dxPieChart("instance");
                            break;
                        case "dxSparkline":
                        case "swSparkline":
                            scope.chart = $(".sw.chart", element).dxSparkline(chartOptions).dxSparkline("instance");
                            break;
                    }

                    if (scope.chart) {
                        scope.chart
                            .on("pointClick", onChartClick)
                            .on("legendClick", onChartClick);
                    }
                }

                init();
            }
        }
    }]);

})(angular, app);