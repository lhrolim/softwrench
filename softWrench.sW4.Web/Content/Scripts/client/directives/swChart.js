(function (angular, app) {
    "use strict";

    app.directive("swChart", ["contextService", "chartService", "$log", "redirectService", "searchService", "$timeout", "dispatcherService", function (contextService, chartService, $log, redirectService, searchService, $timeout, dispatcherService) {

        class chartClickParams {
            constructor(originalDto, fieldName, fieldValue) {
                /*original search dto that has been built upon the chart click, for loading its details */
                this.originalDto = originalDto;
                this.fieldName = fieldName;
                this.fieldValue = fieldValue;
            }
        }


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
                scope.vm = {};
                scope.vm.labeloptions = {};

                function goToFilteredGrid(application, fieldName, fieldValue, clickDtoProvider) {
                    const appName = application.equalsIc("sr") ? "servicerequest" : application; // open: search for 'not closed' --> !C('CLOSE')
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
                    const searchData = {
                        [fieldName]: searchDataValue
                    };
                    const searchOperator = {
                        [fieldName] : searchOperation
                    };
                    let searchDTO = searchService.buildSearchDTO(searchData, null, searchOperator);
                    if (clickDtoProvider) {
                        searchDTO = dispatcherService.invokeServiceByString(clickDtoProvider, [new chartClickParams(searchDTO, fieldName, fieldValue)]);
                    }
                    return redirectService.goToApplication(appName, "list", { 'SearchDTO': searchDTO });
                }

                //#region user insteraction
                function onChartClick(event) {
                    let configurationDictionary = scope.panel.configurationDictionary;
                    const application = configurationDictionary.application;
                    const fieldName = configurationDictionary.field;
                    const target = event.target;
                    var fieldLabel = angular.isString(target) || angular.isNumber(target) // clicked on legend
                                        ? target
                                        // clicked on chart point
                                        : target.argument;
                    // get actual value from label
                    const fieldValue = scope.data.find(function (e) { return e.entry.fieldLabel === fieldLabel; }).entry.fieldValue;
                    const log = $log.get("swChart#onChartClick", ["chart", "dashboard"]);
                    log.debug("clicked: [application: {0}, fieldName: {1}, fieldValue: {2}, fieldLabel: {3}]".format(application, fieldName, fieldValue, fieldLabel));

                    goToFilteredGrid(application, fieldName, fieldValue, configurationDictionary.clickDtoProvider);
                }
                //#endregion

                function init() {
                    //get the default options
                    const chartOptions = chartService.getChartOptions(scope.chartType, scope.options, scope.data); //launch the correct chart
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