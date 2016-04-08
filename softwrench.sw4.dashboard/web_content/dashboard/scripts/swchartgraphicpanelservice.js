(function (angular, $) {
    "use strict";

    function swchartGraphicPanelService($rootScope, $compile, restService, crudContextHolderService) {
        //#region Utils

        var config = {
            // swchart's template -> sw-chart directive
            template: "<sw-chart "                          +
                        "data-chart-type=\"chartType\" "    +
                        "data-data=\"data\" "               +
                        "data-options=\"options\" "         +
                        "data-panel=\"panel\" "             +
                        "/>",
            fields: {
                nullLabelLookupTable: {
                    'owner': "UNASSINGED",
                    'reportedby': "UNASSINGED",
                    'wopriority': "NO PRIORITY"
                },
                base: [
                    { label: "Owner", value: "owner" },
                    { label: "Reporter", value: "reportedby" },
                    { label: "Status", value: "status" }
                ],
                workorder: [
                    { label: "Priority", value: "wopriority" },
                    { label: "Work Type", value: "worktype" }
                ],
                getFields: function (application) {
                    var specific = this[application];
                    return specific ? this.base.concat(specific) : this.base;
                },
                getNullValueLabel: function(field) {
                    var nullValueLabel = this.nullLabelLookupTable[field];
                    return !!nullValueLabel ? nullValueLabel : null;
                }
            }
        };

        function getChartData(panel) {
            var configuration = panel.configurationDictionary;
            //configuration.limit = parseInt(configuration.limit);
            //configuration.showothers = (configuration.showothers === "True" || configuration.showothers === "true");

            var params = {
                entity: configuration.application,
                application: configuration.application.equalsIc("sr") ? "servicerequest" : configuration.application,
                property: configuration.field,
                whereClauseMetadataId: "dashboard:" + panel.alias,
                limit: (configuration.limit > 0 && !configuration.showothers && configuration.statusconfig !== "openclosed") ? configuration.limit : 0,
                nullValueLabel: config.fields.getNullValueLabel(configuration.field)
            }

            return restService.getPromise("Statistics", "CountByProperty", params)
                .then(function (response) {
                    return formatDataForChart(configuration, response.data);
                });
        }

        function formatDataForChart(configuration , data) {

            // sort by value descending
            data = data.sort(function (d1, d2) {
                return d2.fieldCount - d1.fieldCount;
            });

            // status -> open/close
            if (configuration.field === "status" && configuration.statusfieldconfig === "openclosed") {
                // closed status entry
                var closed = data.find(function (d) {
                    return d.fieldValue.equalsIc("close") || d.fieldValue.equalsIc("closed");
                });
                // sum of all except closed
                var openCount = data.filter(function (d) {
                    return !d.fieldValue.equalsIc("close") && !d.fieldValue.equalsIc("closed");
                })
                .reduce(function (previous, current) {
                    return previous + current.fieldCount;
                }, 0);
                // new array containing only open/close
                data = [
                    closed,
                    { fieldValue: "OPEN", fieldLabel: "OPEN", fieldCount: openCount }
                ];

            }
                // should overflow to 'others' -> top within limit + others
            else if (configuration.limit > 0 && data.length > configuration.limit && configuration.showothers) {
                // <limit> highest counts
                var topresults = data.slice(0, configuration.limit);
                // sum of the others's counts
                var othersCount = data.slice(configuration.limit)
                    .reduce(function (previous, current) {
                        return previous + current.fieldCount;
                    }, 0);
                // new array composed of top <limit> + 'others'
                data = topresults.concat({ fieldValue: "OTHERS", fieldLabel: "OTHERS", fieldCount: othersCount });
            }

            return handleData(configuration.type, data);
        }
        
        function handleData(type, data) {
            var chartData = {};

            switch (type) {
                case "dxChart":
                case "swRecordCountChart":
                case "swRecordCountRotatedChart":
                case "swRecordCountLineChart":
                case "dxPie":
                case "swRecordCountPie":
                    chartData = data.map(function (d) {
                        return {
                            argument: d.fieldLabel,
                            total: d.fieldCount,
                            entry: d
                        }
                    });
                    break;
                case "dxLinearGauge":
                case "swLinearGauge":
                case "dxCircularGauge":
                case "swCircularGauge":
                case "swRecordCountGauge":
                    var total = 0;
                    data.forEach(function (d) {
                        total += d.fieldCount;
                        chartData[d.fieldLabel] = d.fieldCount;
                    });
                    chartData.total = total;
                    break;
                case "swSparkline":
                    chartData = data.map(function (d) {
                        return {
                            argument: d.fieldLabel,
                            value: d.fieldCount,
                            entry: d
                        }
                    });
                    break;
                //case "swRecordTrends":
                //case "dxSparkline":
                //case "swSparkline":
                //    chartData = [
                //        {
                //            "date": "02/12/16",
                //            "reported": 7,
                //            "completed": 5,
                //            "changed": 2
                //        },
                //    ];
                //    break;
                case "swLabel":
                    break;
                //case "dxMap":
                //case "swRecordCountMap":
                //    chartData = [
                //        {
                //            "coordinates": [
                //                -97.1431,
                //                32.844
                //            ],
                //            "percentages": [
                //                66,
                //                34
                //            ],
                //            "text": "BEDFORD",
                //            "total": 895
                //        }
                //    ];
                //    break;
            }
            return chartData;
        }

        /**
         * Renders the requested graphic.
         * 
         * @param DOMNode element 
         * @param {} panel 
         * @param {} options 
         * @returns Promise resolved with the DOMNode representing the graphic: compiled 'sw-chart' directive
         */
        function loadGraphic(element, panel, options) {
            return getChartData(panel)
                .then(function (data) {
                    // create isolated scope for the dynamically compiled 'sw-chart' directive
                    var $parent = angular.element(element).scope();
                    var $scope = $rootScope.$new(true, $parent);
                    $scope.panel = panel;
                    $scope.data = data;
                    $scope.chartType = panel.configurationDictionary.type;
                    $scope.options = panel.configurationDictionary.options;
                    // compile the template with the isolated scope
                    var graphic = $compile(config.template)($scope);
                    $(element).append(graphic);
                    // Promise resolved with the graphic element
                    return graphic;
                });
        }

        function resizeGraphic(graphic, width, height) {
            // implementing the 'interface' -> graphic resizing is handled by the graphic itself
        }

        /**
         * Fills the datamps's 'configurationDictionary' property correctly 
         * from it's view properties.
         * 
         * @param {} datamap 
         */
        function onBeforeAssociatePanel(datamap) {
            datamap.configurationDictionary = {
                'application': datamap.application,
                'field': datamap.field,
                'type': datamap.type,
                'statusfieldconfig': datamap.statusfieldconfig,
                'limit': datamap.limit,
                'showothers': datamap.showothers
            };
        }

        function onProviderSelected(event) {
            // implementing interface
        }

        function onApplicationSelected(event) {
            var application = event.fields.application;
            if (!application) return;
            var fields = config.fields.getFields(application);
            crudContextHolderService.updateEagerAssociationOptions("fields", fields);

        }

        /**
         * Renders the devexpress element of the graphic container.
         * 
         * @param DOMNode graphic sw-chart compiled directive
         */
        function onDashboardSelected(graphic) {
            var scope = angular.element(graphic.children()).scope();
            var chart = scope.chart;
            if (!chart) return;
            chart.render();
        }

        //#endregion

        //#region Service Instance
        var service = {
            loadGraphic: loadGraphic,
            resizeGraphic: resizeGraphic,
            onProviderSelected: onProviderSelected,
            onBeforeAssociatePanel: onBeforeAssociatePanel,
            onApplicationSelected: onApplicationSelected,
            onDashboardSelected: onDashboardSelected
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular
        .module("sw_layout")
        .factory("swchartGraphicPanelService",
            ["$rootScope", "$compile", "restService", "crudContextHolderService", swchartGraphicPanelService]);
    //#endregion

})(angular, jQuery);
