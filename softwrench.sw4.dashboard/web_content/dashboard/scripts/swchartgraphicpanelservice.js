(function (angular, $) {
    "use strict";

    function swchartGraphicPanelService($rootScope, $compile, restService, crudContextHolderService) {
        //#region Utils

        var config = {
            // swchart's template -> sw-chart directive
            template: "<sw-chart " +
                        "data-chart-type=\"chartType\" " +
                        "data-data=\"data\" " +
                        "data-options=\"options\" />",
            fields: {
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
                }
            }
        };

        function getChartData(configuration) {
            configuration.limit = parseInt(configuration.limit);
            configuration.showothers = (configuration.showothers === "True" || configuration.showothers === "true");

            var params = {
                entity: configuration.application,
                property: configuration.field,
                limit: (configuration.limit > 0 && !configuration.showothers && configuration.statusconfig !== "openclosed") ? configuration.limit : 0
            }

            return restService.getPromise("Statistics", "CountByProperty", params)
                .then(function (response) {
                    return formatDataForChart(configuration, response.data);
                });
        }

        function formatDataForChart(configuration, data) {
            var processed = [];
            angular.forEach(data, function (value, key) {
                processed.push({ field: key, value: value });
            });
            // sort by value descending
            processed = processed.sort(function (d1, d2) {
                return d2.value - d1.value;
            });

            // status -> open/close
            if (configuration.field === "status" && configuration.statusfieldconfig === "openclosed") {
                // closed status entry
                var closed = processed.find(function (d) {
                    return d.field.equalsIc("close");
                });
                // sum of all except closed
                var openCount = processed.filter(function (d) {
                    return !d.field.equalsIc("close");
                })
                .reduce(function (previous, current) {
                    return previous + current.value;
                }, 0);
                // total
                var totalCount = closed.value + openCount;
                // new array containing only open/close
                processed = [
                    { field: "closed", value: closed.value },
                    { field: "open", value: openCount },
                    { field: "total", value: totalCount }
                ];

            }
                // should overflow to 'others' -> top within limit + others
            else if (configuration.limit > 0 && processed.length > configuration.limit && configuration.showothers) {
                // 6 highest counts
                var topresults = processed.slice(0, configuration.limit);
                // sum of the others's counts
                var othersCount = processed.slice(configuration.limit)
                    .reduce(function (previous, current) {
                        return previous + current.value;
                    }, 0);
                // new array composed of top 6 + 'others'
                processed = topresults.concat({ field: "others", value: othersCount });
            }

            return handleData(configuration.type, processed);
        }
        
        function handleData(type, data) {
            var chartData = {};

            switch (type) {
                case "dxChart":
                case "swRecordCountChart":
                    chartData = data.map(function (d) {
                        return { argument: d.field, total: d.value }
                    });
                    break;
                case "dxPie":
                case "swRecordCountPie":
                    chartData = data
                        .filter(function(d) { // pie charts shouldn't display 'total' as a member
                            return d.field !== "total";
                        })
                        .map(function (d) {
                            return { argument: d.field, total: d.value }
                        });
                    break;
                case "dxLinearGauge":
                case "swLinearGauge":
                case "dxCircularGauge":
                case "swCircularGauge":
                case "swRecordCountGauge":
                    data.forEach(function(d) {
                        chartData[d.field] = d.value;
                    });
                    break;
                case "swSparkline":
                    chartData = data.map(function (d) {
                        return { argument: d.field, value: d.value }
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
            return getChartData(panel.configurationDictionary)
                .then(function (data) {
                    // create isolated scope for the dynamically compiled 'sw-chart' directive
                    var $parent = angular.element(element).scope();
                    var $scope = $rootScope.$new(true, $parent);
                    $scope.chartType = panel.configurationDictionary.type;
                    $scope.data = data;
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
        //#endregion

        //#region Service Instance
        var service = {
            loadGraphic: loadGraphic,
            resizeGraphic: resizeGraphic,
            onProviderSelected: onProviderSelected,
            onBeforeAssociatePanel: onBeforeAssociatePanel,
            onApplicationSelected: onApplicationSelected
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
