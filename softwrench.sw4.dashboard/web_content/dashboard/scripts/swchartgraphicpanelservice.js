(function (angular, $) {
    "use strict";

    function swchartGraphicPanelService($q, $rootScope, $compile) {
        //#region Utils

        var config = {
            // swchart's template -> sw-chart directive
            template: 
                "<sw-chart"                                         +
                    "data-chart-type=\"data.graphic.chartType\""    +
                    "data-data=\"data.graphic.data\""               +
                    "data-options=\"data.graphic.options\">"        +
                "</sw-chart>"
        };

        //TODO: replace with real data
        function getPlaceHolderData(type) {
            var data = {};

            switch (type) {
                case "dxChart":
                case "swRecordCountChart":
                case "dxPie":
                case "swRecordCountPie":
                    data = [
                    {
                        "argument": "CLOSED",
                        "total": 5013
                    }, {
                        "argument": "INPROG",
                        "total": 764
                    }, {
                        "argument": "NEW",
                        "total": 2406
                    }, {
                        "argument": "PENDING",
                        "total": 1089
                    }, {
                        "argument": "QUEUED",
                        "total": 1372
                    }, {
                        "argument": "RESOLVED",
                        "total": 3469
                    }];
                    break;
                case "dxLinearGauge":
                case "swLinearGauge":
                case "dxCircularGauge":
                case "swCircularGauge":
                case "swRecordCountGauge":
                    data = {
                        "total": 14113,
                        "opened": 5631,
                        "closed": 8482
                    }
                    break;
                case "swRecordTrends":
                case "dxSparkline":
                case "swSparkline":
                    data = [
                        {
                            "date": "02/12/16",
                            "reported": 7,
                            "completed": 5,
                            "changed": 2
                        },
                        {
                            "date": "02/13/16",
                            "reported": 5,
                            "completed": 0,
                            "changed": 0
                        },
                        {
                            "date": "02/14/16",
                            "reported": 0,
                            "completed": 0,
                            "changed": 0
                        },
                        {
                            "date": "02/15/16",
                            "reported": 26,
                            "completed": 14,
                            "changed": 9
                        },
                        {
                            "date": "02/16/16",
                            "reported": 31,
                            "completed": 17,
                            "changed": 14
                        },
                        {
                            "date": "02/17/16",
                            "reported": 24,
                            "completed": 9,
                            "changed": 14
                        },
                        {
                            "date": "02/18/16",
                            "reported": 19,
                            "completed": 15,
                            "changed": 9
                        }
                    ];
                    break;
                case "swLabel":
                    break;
                case "dxMap":
                case "swRecordCountMap":
                    data = [
                        {
                            "coordinates": [
                                -97.1431,
                                32.844
                            ],
                            "percentages": [
                                66,
                                34
                            ],
                            "text": "BEDFORD",
                            "total": 895
                        },
                        {
                            "coordinates": [
                                -71.0589,
                                42.3601
                            ],
                            "percentages": [
                                38,
                                62
                            ],
                            "text": "BOSTON",
                            "total": 3308
                        },
                        {
                            "coordinates": [
                                -104.99,
                                39.7392
                            ],
                            "percentages": [
                                59,
                                41
                            ],
                            "text": "DENVER",
                            "total": 1430
                        },
                        {
                            "coordinates": [
                                -112.074,
                                33.4484
                            ],
                            "percentages": [
                                30,
                                70
                            ],
                            "text": "PHOENIX",
                            "total": 6637
                        },
                        {
                            "coordinates": [
                                -122.332,
                                47.6062
                            ],
                            "percentages": [
                                53,
                                47
                            ],
                            "text": "SEATTLE",
                            "total": 1843
                        }
                    ];
                    break;
            }
            return data;
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
            // create isolated scope for the dynamically compiled 'sw-chart' directive
            var $parent = angular.element(element).scope();
            var $scope = $rootScope.$new(true, $parent);
            $scope.chartType = panel.configurationDictionary.type;
            $scope.data = getPlaceHolderData(panel.configurationDictionary.type); //TODO: replace with real data
            $scope.options = panel.configurationDictionary.options;
            // compile the template with the isolated scope
            var graphic = $compile(config.template)($scope);
            element.append(graphic);
            // Promise resolved with the graphic element
            return $q.when(graphic);
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
                'options': datamap.options
            };
        }
        
        function onProviderSelected(event) {
            // implementing interface
            // TODO: enable chart creation -> set the options in the context
        }
        //#endregion

        //#region Service Instance
        var service = {
            loadGraphic: loadGraphic,
            resizeGraphic: resizeGraphic,
            onProviderSelected: onProviderSelected,
            onBeforeAssociatePanel: onBeforeAssociatePanel
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular
        .module("sw_layout")
        .factory("swchartGraphicPanelService",
            ["$q", "$rootScope", "$compile", swchartGraphicPanelService]);
    //#endregion

})(angular, jQuery);
