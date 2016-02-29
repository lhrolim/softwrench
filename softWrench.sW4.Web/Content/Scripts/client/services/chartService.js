(function (angular) {
    'use strict';

    angular.module('sw_layout').service('chartService', ['$log', chartService]);

    function chartService($log) {
        var log = $log.getInstance('sw4.chartService');
//        log.debug('running');

        //service mehtods
        function calcScale(total) {
            var scale = parseInt('1' + Array(total.toString().length).join('0'));
            return Math.ceil(total / scale) * scale;
        }

        function formatPercent(string) {
            return string.replace(/\s+/g, '');
        }

        function sumTotal(data, property) {
            var total = 0;

            data.forEach(function (item) {
                total += item[property];
            });

            return total;
        }

        //default options methods
        function getGaugeDefaults(data) {
            var total = data.total;
            return {
                geometry: {
                    orientation: 'vertical'
                },
                rangeContainer: {
                    backgroundColor: '#d9d9d9'
                },
                scale: {
                    startValue: 0,
                    endValue: calcScale(total),
                    tickInterval: calcScale(total) / 4
                },
                title: {
                    text: 'Total: ' + total.toLocaleString()
                },
                value: total,
                valueIndicator: {
                    color: '#f05b41'
                }
            };
        }

        function getGeneralDefaults() {
            return {
                legend: {
                    horizontalAlignment: 'center',
                    itemTextPosition: 'bottom',
                    verticalAlignment: 'bottom',
                    visible: false
                },
                loadingIndicator: {
                    show: true
                },
                title: {
                    font: {
                        size: 18
                    },
                    horizontalAlignment: 'center',
                    position: 'bottom-center',
                    verticalAlignment: 'bottom'
                }
            };
        }

        function getLabelDefaults(data) {
            return {
                background: '#f65752',
                color: '#fff',
                title: {
                    text: 'Title'
                },
                value: data.total.toLocaleString(),
                units: {
                    text: 'Units'
                }
            };
        }

        function getMapDefaults() {
            return {
                background: {
                    borderColor: 'transparent'
                },
                center: [-98.583333, 38.333333],
                controlBar: {
                    enabled: false
                },
                layers: [{
                    data: DevExpress.viz.map.sources.usa,
                    hoverEnabled: false
                }],
                loadingIndicator: {
                    show: false
                },
                touchEnabled: false,
                wheelEnabled: false,
                zoomFactor: 11
            };
        }

        function getRecordCountChartDefaults(data) {
            var total = sumTotal(data, 'total');
            return {
                dataSource: data,
                series: {
                    argumentField: 'argument',
                    color: '#4488f2',
                    label: {
                        visible: true,
                        format: 'fixedPoint'
                    },
                    type: 'bar',
                    valueField: 'total'
                },
                tooltip: {
                    enabled: true,
                    customizeTooltip: function (arg) {
                        var percentage = (arg.value / total) * 100;
                        return {
                            text: Math.round(percentage) + '%'
                        }
                    }
                }
            };
        }

        function getRecordCountGaugeDefaults(data) {
            return {
                rangeContainer: {
                    ranges: [
                        {
                            startValue: 0,
                            endValue: data.opened,
                            color: '#39b54a'
                        },
                        {
                            startValue: data.opened,
                            endValue: data.opened + data.closed,
                            color: '#f65752'
                        }
                    ],
                    width: 20
                },
                scale: {
                    tick: {
                        length: 20
                    }
                },
                title: {
                    text: '<b>Total: ' + data.total.toLocaleString() + '</b>',
                    subtitle: 'Open: ' + data.opened.toLocaleString() + '<br />Closed: ' + data.closed.toLocaleString()
                },
                valueIndicator: {
                    offset: 30
                }
            };
        }

        function getRecordCountMapDefaults(data) {
            //return {
            //    case 'swRecordCountMap':
            var names = ['Open', 'Closed'];
            var markers = {
                type: 'FeatureCollection',
                features: $.map(data, function (point) {
                    return {
                        type: 'Feature',
                        geometry: {
                            type: 'Point',
                            coordinates: point.coordinates
                        },
                        properties: {
                            text: point.text,
                            value: point.total,
                            values: point.percentages
                        }
                    };
                })
            };

            return {
                layers: [{}, {
                    name: 'bubbles',
                    data: markers,
                    elementType: 'bubble',
                    dataField: 'value',
                    minSize: 20,
                    maxSize: 60,
                    sizeGroups: [0, 1000, 3000, 5000, 10000]
                }],
                tooltip: {
                    enabled: true,
                    customizeTooltip: function (arg) {
                        var name = arg.attribute('text'),
                            total = arg.attribute('value');

                        if (name) {
                            var node = $('<div>')
                                .append('<h5><b>' + name + ':</b> ' + total.toLocaleString() + '</h5>')
                                .append('<div id="dxtt-chart" style="width: 150px; height: 125px;"></div>');

                            return {
                                html: node.html()
                            };
                        }
                    }
                },
                onTooltipShown: function (e) {
                    var container = $("#dxtt-chart");
                    var data = [
                        {
                            "status": names[0],
                            "total": e.target.attribute('values')[0]
                        },
                        {
                            "status": names[1],
                            "total": e.target.attribute('values')[1]
                        }
                    ];

                    var dxOptions = {
                        animation: {
                            enabled: false
                        },
                        commonSeriesSettings: {
                            label: {
                                visible: true,
                                format: 'fixedPoint',
                                customizeText: function (args) {
                                    return args.value + "%";
                                }
                            },
                            valueAxis: {
                                max: 100,
                                min: 0
                            }
                        },
                        dataSource: data,
                        legend: {
                            visible: false
                        },
                        series: {
                            argumentField: 'status',
                            valueField: 'total',
                            type: 'bar',
                            color: '#e59323',
                            label: {
                                visible: true,
                                format: 'fixedPoint'
                            }
                        },
                        valueAxis: {
                            label: { visible: false }
                        }
                    };
                    container.dxChart(dxOptions);
                }
            };
        }

        function getRecordCountPieDefaults(data) {
            return {
                //case 'swRecordCountPie':
                //chartDefaults = {
                dataSource: data,
                legend: {
                    visible: true
                },
                resolveLabelOverlapping: 'shift',
                series: {
                    argumentField: 'argument',
                    label: {
                        visible: false,
                        connector: {
                            visible: true
                        }
                    },
                    smallValuesGrouping: {
                        groupName: 'OTHERS',
                        mode: 'topN',
                        topCount: 6
                    },
                    valueField: 'total'
                },
                tooltip: {
                    enabled: true,
                    customizeTooltip: function (arg) {
                        return {
                            text: arg.originalValue.toLocaleString()
                        }
                    }
                }
            };
        }

        function getRecordTrendsDefaults(data) {
            var defaults = {
                dataSource: data,
                argumentAxis: {
                    discreteAxisDivisionMode: 'crossLabels',
                    grid: {
                        visible: true
                    },
                    valueMarginsEnabled: false
                },
                commonSeriesSettings: {
                    argumentField: 'date',
                    type: 'spline',
                    label: {
                        visible: true,
                        format: 'fixedPoint',
                        precision: 0
                    }
                },
                legend: {
                    visible: true
                },
                tooltip: {
                    enabled: true,
                    customizeTooltip: function (arg) {
                        return {
                            text: arg.originalValue.toLocaleString()
                        }
                    }
                }
            };

            var safetyOptions = {};

            //safety overrides for time spans longer than a week
            if (data.length > 7) {
                safetyOptions = {
                    commonSeriesSettings: {
                        label: {
                            visible: false
                        },
                        line: {
                            point: {
                                visible: false
                            }
                        },
                        spline: {
                            point: {
                                visible: false
                            }
                        }
                    }
                };

                $.extend(true, defaults, safetyOptions);
            }

            //safety overrides for time spans longer than a month
            if (data.length > 32) {
                safetyOptions = {
                    argumentAxis: {
                        grid: {
                            visible: false
                        }
                    },
                    tooltip: {
                        enabled: false
                    }
                };

                $.extend(true, defaults, safetyOptions);
            }

            return defaults;
        }

        function getSparklineDefaults(data) {
            return {
                //case 'swSparkline':
                //chartDefaults = {
                dataSource: data,
                argumentField: 'date',
                ignoreEmptyPoints: true,
                valueField: 'reported',
                type: 'splinearea',
                showMinMax: true,
                tooltip: {
                    customizeTooltip: function (sparkline) {
                        return {
                            html: '<strong>Min:</strong> ' + sparkline.minValue + '<br /><strong>Max:</strong> ' + sparkline.maxValue
                        };
                    }
                }
            };
        }

        //addon methods (DexExtreme object format)
        function addPieLabelAndCountLabels() {
            return {
                series: {
                    label: {
                        customizeText: function (arg) {
                            return arg.argumentText + ': ' + arg.originalValue.toLocaleString();
                        },
                        visible: true
                    }
                }
            };
        }

        function addPieLabelAndPercentageLabels() {
            return {
                series: {
                    label: {
                        customizeText: function (arg) {
                            return arg.argument + ': ' + chartServiceDefaults.formatPercent(arg.percentText);
                        },
                        visible: true
                    }
                }
            };
        }

        function addPiePercentageTooltips() {
            return {
                tooltip: {
                    customizeTooltip: function (arg) {
                        console.log(arg);
                        return {
                            text: chartServiceDefaults.formatPercent(arg.percentText)
                        }
                    },
                    enabled: true
                }
            };
        }

        function addRecordTotalTitle(data, property) {
            //get the default values
            property = property != undefined ? property : 'total';
            var total = 0;

            //if the data (object) has a total property, sum the array
            if (data[property]) {
                total = data[property];
            } else {
                total = chartServiceDefaults.sumTotal(data, property);
            }

            return {
                title: {
                    text: 'Total: ' + total.toLocaleString()
                }
            };
        }

        //public methods
        return {
            formatPercent: function (string) {
                return formatPercent(string);
            },
            sumTotal: function (data, property) {
                return sumTotal(data, property);
            },
            getAddonOptions: function (chartOptions, data) {
                var addonOptions = {};

                if (chartOptions != undefined && chartOptions.swChartsAddons != undefined) {
                    var swChartOptions = chartOptions.swChartsAddons;

                    //loop thru the addon property object
                    for (var key in swChartOptions) {
                        var addon = {};

                        // skip loop if the property is from prototype
                        if (!swChartOptions.hasOwnProperty(key)) continue;

                        //call the correct addon method
                        switch (key) {
                            case 'addPiePercentageTooltips':
                                addon = addPiePercentageTooltips();
                                break;
                            case 'addPieLabelAndCountLabels':
                                addon = addPieLabelAndCountLabels();
                                break;
                            case 'addPieLabelAndPercentageLabels':
                                addon = addPieLabelAndPercentageLabels();
                                break;
                            case 'addRecordTotalTitle':
                                addon = addRecordTotalTitle(data, swChartOptions.addRecordTotalTitle.property);
                                break;
                        }

                        $.extend(true, addonOptions, addon);
                    }
                }

                return addonOptions;
            },
            getChartOptions: function (chartType, specificOptions, data) {
                var log = $log.getInstance('sw4.chartService.getChartOptions');

                //build the option objects
                var chartDefaults = this.getDefaultOptions(chartType, data);
                var defaultAddons = this.getAddonOptions(chartDefaults, data);
                var specificAddons = this.getAddonOptions(specificOptions, data);

                //combine the default, specific, and addon options
                return $.extend(true, {}, chartDefaults, specificOptions, defaultAddons, specificAddons);
            },
            getDefaultOptions: function (chartType, data) {
                var log = $log.getInstance('sw4.chartService.getDefaultOptions');
                var chartDefaults = {};

                //get default chart options
                switch (chartType) {
                    case 'swCircularGauge':
                    case 'swLinearGauge':
                        chartDefaults = getGaugeDefaults(data);
                        break;
                    case 'swLabel':
                        chartDefaults = getLabelDefaults(data);
                        break;
                    case 'swRecordCountChart':
                        chartDefaults = getRecordCountChartDefaults(data);
                        break;
                    case 'swRecordCountGauge':
                        chartDefaults = $.extend(true, {}, getGaugeDefaults(data), getRecordCountGaugeDefaults(data));
                        break;
                    case 'swRecordCountMap':
                        chartDefaults = $.extend(true, {}, getMapDefaults(), getRecordCountMapDefaults(data));
                        break;
                    case 'swRecordCountPie':
                        chartDefaults = getRecordCountPieDefaults(data);
                        break;
                    case 'swRecordTrends':
                        chartDefaults = getRecordTrendsDefaults(data);
                        break;
                    case 'swSparkline':
                        chartDefaults = getSparklineDefaults(data);
                        break;
                }

                //combine the general and chart defaults
                return $.extend(true, {}, getGeneralDefaults(), chartDefaults);
            }
        }
    }
})(angular);





//var app = angular.module('App');
//app.service('chartService', ['$log', 'chartServiceAddons', 'chartServiceDefaults', function ($log, chartServiceAddons, chartServiceDefaults) {
//    var log = $log.getInstance('chartService');
//    log.debug('running');

//    //public methods
//    return {
//        getChartOptions: function (chartType, specificOptions, data) {
//            var log = $log.getInstance('chartService.getChartOptions');

//            //build the option objects
//            var chartDefaults = chartServiceDefaults.getDefaultOptions(chartType, data);
//            var defaultAddons = chartServiceAddons.getAddonOptions(chartDefaults, data);
//            var specificAddons = chartServiceAddons.getAddonOptions(specificOptions, data);

//            //combine the default, specific, and addon options
//            return $.extend(true, {}, chartDefaults, specificOptions, defaultAddons, specificAddons);
//        }
//    }
//}]);
