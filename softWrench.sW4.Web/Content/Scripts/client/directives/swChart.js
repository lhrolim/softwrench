(function (angular, app) {
    "use strict";

    app.directive("swChart", ["contextService", "chartService", function (contextService, chartService) {
        return {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/directives/swChart.html"),
            scope: {
                chartType: "=",
                data: "=",
                options: "="
            },
            link: function(scope, element, attrs) {
                scope.labeloptions = {};

                //get the default options
                var chartOptions = chartService.getChartOptions(scope.chartType, scope.options, scope.data);

                //launch the correct chart
                switch (scope.chartType) {
                    case "dxChart":
                    case "swRecordCountChart":
                    case "swRecordCountRotatedChart":
                    case "swRecordTrends":
                        $(element).dxChart(chartOptions);
                        break;
                    case "dxCircularGauge":
                    case "swCircularGauge":
                    case "swRecordCountGauge":
                        $(element).dxCircularGauge(chartOptions);
                        break;
                    case "swLabel":
                        scope.labeloptions = chartOptions;
                        break;
                    case "dxLinearGauge":
                    case "swLinearGauge":
                        $(element).dxLinearGauge(chartOptions);
                        break;
                    case "dxMap":
                    case "swRecordCountMap":
                        $(element).dxVectorMap(chartOptions);
                        break;
                    case "dxPie":
                    case "swRecordCountPie":
                        $(element).dxPieChart(chartOptions);
                        break;
                    case "dxSparkline":
                    case "swSparkline":
                        $(element).dxSparkline(chartOptions);
                        break;
                }
            }
        }
    }]);

})(angular, app);