(function (angular, $) {
    "use strict";

    function swchartGraphicPanelService($q, $log, restService, crudContextHolderService, localStorageService, contextService) {
        //#region Utils
        var config = {
            defaultProvider: "swChart"
        }

        //TODO: replace with real data
        function getPlaceHolderData(type) {
            var data = {};

            switch (type) {
                case 'dxChart':
                case 'swRecordCountChart':
                case 'dxPie':
                case 'swRecordCountPie':
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
                case 'dxLinearGauge':
                case 'swLinearGauge':
                case 'dxCircularGauge':
                case 'swCircularGauge':
                case 'swRecordCountGauge':
                    data = {
                        "total": 14113,
                        "opened": 5631,
                        "closed": 8482
                    }
                    break;
                case 'swRecordTrends':
                case 'dxSparkline':
                case 'swSparkline':
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
                    ]
                    break;
                case 'swLabel':

                    break;
                case 'dxMap':
                case 'swRecordCountMap':
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
                    ]
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
         * @returns Promise resolved with the tableau Viz instantiated
         */
        function renderGraphic(element, panel, options) {
            var log = $log.getInstance('sw4.swchartGraphicPanelService.renderGraphic');
            var deferred = $q.defer();

            try {
                deferred.resolve({
                    chartType: panel.configurationDictionary.type,
                    data: getPlaceHolderData(panel.configurationDictionary.type), //TODO: replace with real data
                    options: panel.configurationDictionary.options
                });
            } catch (e) {
                deferred.reject(e);
            }
            return deferred.promise;
        }

        /**
         * Renders the requested graphic.
         * 
         * @param DOMNode element 
         * @param {} panel 
         * @param {} options 
         * @returns Promise resolved with tableau Viz instantiated 
         */
        function loadGraphic(element, panel, options) {
            return renderGraphic(element, panel, options);
        }

        function resizeGraphic(graphic, width, height) {
            //console.log('resizeGraphic', graphic, width, height);
            //return graphic.setFrameSize(width, height);
        }

        /**
         * Fills the datamps's 'configurationDictionary' property correctly 
         * from it's view properties.
         * 
         * @param {} datamap 
         */
        function onBeforeAssociatePanel(datamap) {
            var log = $log.getInstance('sw4.swchartGraphicPanelService.onBeforeAssociatePanel');

            datamap.configurationDictionary = {
                'application': datamap.application,
                'field': datamap.field,
                'type': datamap.type,
                'options': datamap.options
            };
        }

        /**
         * Authenticates to tableau server's REST api (if not already authenticated) 
         * and populates the associationOptions with the user's workbooks.
         * 
         * @param {} event 
         */
        function onProviderSelected(event) {
        }
        //#endregion

        //#region Service Instance
        var service = {
            renderGraphic: renderGraphic,
            loadGraphic: loadGraphic,
            resizeGraphic: resizeGraphic,
            onProviderSelected: onProviderSelected,
            //onWorkbookSelected: onWorkbookSelected,
            onBeforeAssociatePanel: onBeforeAssociatePanel
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular
        .module("sw_layout")
        .factory("swchartGraphicPanelService",
            ["$q", "$log", "restService", "crudContextHolderService", "localStorageService", "contextService", swchartGraphicPanelService]);
    //#endregion

})(angular, jQuery);
