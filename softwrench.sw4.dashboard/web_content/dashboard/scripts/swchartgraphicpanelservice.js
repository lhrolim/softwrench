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
                    const specific = this[application];
                    return specific ? this.base.concat(specific) : this.base;
                },
                getNullValueLabel: function(field) {
                    const nullValueLabel = this.nullLabelLookupTable[field];
                    return !!nullValueLabel ? nullValueLabel : null;
                }
            }
        };

        function getChartData(panel) {
            var configuration = panel.configurationDictionary;
            const isServiceRequest = "sr".equalsIc(configuration.application);
            const params = {
                entity: isServiceRequest ? "SR" : configuration.application,
                application: isServiceRequest ? "servicerequest" : configuration.application,
                property: configuration.field,
                whereClauseMetadataId: `dashboard:${panel.alias}`,
                limit: (configuration.limit > 0 && !configuration.showothers && configuration.statusconfig !== "openclosed") ? configuration.limit : 0,
                nullValueLabel: config.fields.getNullValueLabel(configuration.field)
            };
            var method = "CountByProperty";

            // custom system only charts: require custom server-side action logic
            if (!!configuration.action) {
                params["action"] = configuration.action;
                method = "GetStatisticalData";
            }

            return restService.getPromise("Statistics", method, params)
                .then(response => {
                    const processed = processData(configuration, response.data);
                    return formatDataForChart(configuration.type, processed);
                });
        }

        function processData(configuration , data) {
            // sort by value descending
            var processed = data.sort((d1, d2) => d2.fieldCount - d1.fieldCount);

            // status -> open/close
            if (configuration.field === "status" && configuration.statusfieldconfig === "openclosed") {
                // closed status entry
                const closed = processed.find(d => d.fieldValue.equalsIc("close") || d.fieldValue.equalsIc("closed"));
                // sum of all except closed
                const openCount = processed
                    .filter(d => !d.fieldValue.equalsIc("close") && !d.fieldValue.equalsIc("closed"))
                    .reduce( (previous, current) => previous + current.fieldCount, 0);
                // new array containing only open/close
                processed = [
                    closed || { fieldValue: "CLOSED", fieldLabel: "CLOSED", fieldCount: 0 },
                    { fieldValue: "OPEN", fieldLabel: "OPEN", fieldCount: openCount }
                ];

            }
            // should overflow to 'others' -> top within limit + others
            else if (configuration.limit > 0 && processed.length > configuration.limit && configuration.showothers) {
                // <limit> highest counts
                const topresults = processed.slice(0, configuration.limit);
                // sum of the others's counts
                const othersCount = processed.slice(configuration.limit).reduce((previous, current) => previous + current.fieldCount, 0);
                // new array composed of top <limit> + 'others'
                processed = topresults.concat({ fieldValue: "OTHERS", fieldLabel: "OTHERS", fieldCount: othersCount });
            }

            return processed;
        }
        
        function formatDataForChart(type, data) {
            var chartData = {};

            switch (type) {
                case "dxChart":
                case "swRecordCountChart":
                case "swRecordCountRotatedChart":
                case "swRecordCountLineChart":
                case "dxPie":
                case "swRecordCountPie":
                    chartData = data.map(d => ({
                        argument: d.fieldLabel,
                        total: d.fieldCount,
                        entry: d
                    }));
                    break;
                case "dxLinearGauge":
                case "swLinearGauge":
                case "dxCircularGauge":
                case "swCircularGauge":
                case "swRecordCountGauge":
                    var total = 0;
                    data.forEach(d => {
                        total += d.fieldCount;
                        chartData[d.fieldLabel] = d.fieldCount;
                    });
                    chartData.total = total;
                    break;
                case "swSparkline":
                    chartData = data.map(d => ({
                        argument: d.fieldLabel,
                        value: d.fieldCount,
                        entry: d
                    }));
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
                .then(data => {
                    // create isolated scope for the dynamically compiled 'sw-chart' directive
                    const $parent = angular.element(element).scope();
                    const $scope = $rootScope.$new(true, $parent);
                    $scope.panel = panel;
                    $scope.data = data;
                    $scope.chartType = panel.configurationDictionary.type;
                    $scope.options = panel.configurationDictionary.options;
                    // compile the template with the isolated scope
                    const graphic = $compile(config.template)($scope);
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
            const application = event.fields.application;
            if (!application) return;
            const fields = config.fields.getFields(application);
            crudContextHolderService.updateEagerAssociationOptions("fields", fields);

        }

        /**
         * Renders the devexpress element of the graphic container.
         * 
         * @param DOMNode graphic sw-chart compiled directive
         */
        function onDashboardSelected(graphic) {
            const scope = angular.element(graphic.children()).scope();
            const chart = scope.chart;
            if (!chart) return;
            chart.render();
        }

        /**
         * Filters the application option according to the list of authorized 
         * applications of the current user.
         * 
         * @param AssociationOption option 
         * @returns boolean whether or not the options should be displayed (user has authorization) 
         */
        function filterSelectableApplications(option) {
            const applications = crudContextHolderService.fetchEagerAssociationOptions("applications", { schemaId: "#modal" });
            const appNames = applications.map(a => a.value);
            const filterableName = option.value === "sr" ? "servicerequest" : option.value;
            // option is part of authorized apps
            return !!appNames.find(a =>  a === filterableName);
        }

        //#endregion

        //#region Service Instance
        const service = {
            loadGraphic: loadGraphic,
            resizeGraphic: resizeGraphic,
            onProviderSelected: onProviderSelected,
            onBeforeAssociatePanel: onBeforeAssociatePanel,
            onApplicationSelected: onApplicationSelected,
            onDashboardSelected: onDashboardSelected,
            filterSelectableApplications: filterSelectableApplications
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
