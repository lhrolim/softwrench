(function (angular, $) {
    "use strict";

    function swchartGraphicPanelService($q, $log, restService, crudContextHolderService, localStorageService, contextService) {
        //#region Utils
        var config = {
            defaultProvider: "swChart"
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

            var data = [{ "argument": "CLOSED", "total": 5013 }, { "argument": "INPROG", "total": 764 }, { "argument": "NEW", "total": 2406 }, { "argument": "PENDING", "total": 1089 }, { "argument": "QUEUED", "total": 1372 }, { "argument": "RESOLVED", "total": 3469 }];

            try {
                log.debug(element, panel, options);

                //TODO: get panel options, get chart options, load data? pass api?
                deferred.resolve({
                    chartType: 'swRecordCountChart', //from panel options
                    data: data, //from panel options (temp)
                    //json: 'http://charts.designsbysm.me/api/field-count?field=status',
                    options: {} //from defaul and addon service
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
        //function onBeforeAssociatePanel(datamap) {
        //    datamap.configurationDictionary = {
        //        'workbook': datamap.workbook.name,
        //        'view': datamap.view.name
        //    };
        //}

        //#endregion

        //#region Service Instance
        var service = {
            renderGraphic: renderGraphic,
            loadGraphic: loadGraphic,
            resizeGraphic: resizeGraphic,
            //onProviderSelected: onProviderSelected,
            //onWorkbookSelected: onWorkbookSelected,
            //onBeforeAssociatePanel: onBeforeAssociatePanel
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
