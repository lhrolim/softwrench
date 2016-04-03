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
            link: function (scope, element, attrs) {
                scope.vm = { };
                scope.vm.labeloptions = {};

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
                //if (scope.chart) {
                //    scope.chart.on("pointClick", function(e) {
                //        console.log("!!! DEVEXPRESS POINTCLICK !!!");
                //        console.log(e);
                //    });
                //}
            }
        }
    }]);

})(angular, app);