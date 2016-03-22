(function (angular, app) {
    'use strict';

    app.directive('swChart', function (contextService, chartService, $log, $http) {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/Templates/directives/swChart.html'),
            scope: {
                chartType: '=',
                data: '=',
                options: '='
            },
            link: function(scope, element, attrs) {
                var log = $log.getInstance('sw4.swChart.directive');

                scope.$watch('graphic', function (newValue, oldValue) {
                    //get the default options
                    var chartOptions = chartService.getChartOptions(scope.chartType, scope.options, scope.data);

                    //launch the correct chart
                    switch (scope.chartType) {
                        case 'dxChart':
                        case 'swRecordCountChart':
                        case 'swRecordTrends':
                            $(element).dxChart(chartOptions);
                            break;
                        case 'dxCircularGauge':
                        case 'swCircularGauge':
                        case 'swRecordCountGauge':
                            $(element).dxCircularGauge(chartOptions);
                            break;
                        case 'swLabel':
                            scope.labeloptions = chartOptions;
                            break;
                        case 'dxLinearGauge':
                        case 'swLinearGauge':
                            $(element).dxLinearGauge(chartOptions);
                            break;
                        case 'dxMap':
                        case 'swRecordCountMap':
                            $(element).dxVectorMap(chartOptions);
                            break;
                        case 'dxPie':
                        case 'swRecordCountPie':
                            $(element).dxPieChart(chartOptions);
                            break;
                        case 'dxSparkline':
                        case 'swSparkline':
                            $(element).dxSparkline(chartOptions);
                            break;
                    }
                });
            }
        }
    });

    app.directive('swChartLabel', function (contextService, $log) {
        return {
            scope: {
                data: '='
            },
            //templateUrl: templateFolder + 'sw-chart-label.html',
            templateUrl: contextService.getResourceUrl('/Content/Templates/directives/swChartLabel.html'),
            link: function (scope, element, attrs) {
                var log = $log.getInstance('swChartLabel.directive');

                scope.$watch('data', function (newValue, oldValue) {
                    if (newValue != undefined) {
                        scope.title = newValue.title.text;
                        scope.value = newValue.value;
                        scope.units = newValue.units.text;
                        scope.background = newValue.background;
                        scope.color = newValue.color;
                    }
                });
            }
        };
    });
})(angular, app);