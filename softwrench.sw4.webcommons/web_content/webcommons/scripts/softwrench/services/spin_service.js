(function (angular, Spinner) {
    'use strict';

    var defaultOptions = {
        lines: 13, // The number of lines to draw
        length: 20, // The length of each line
        width: 10, // The line thickness
        radius: 30, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#000', // #rgb or #rrggbb or array of colors
        speed: 1, // Rounds per second
        trail: 60, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: false, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: 'auto', // Top position relative to parent in px
        left: 'auto', // Left position relative to parent in px,
        opacity: 1 / 4
    };

    var smallOpts = {
        lines: 13, // The number of lines to draw
        length: 10, // The length of each line
        width: 5, // The line thickness
        radius: 15, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#000', // #rgb or #rrggbb or array of colors
        speed: 1, // Rounds per second
        trail: 60, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: false, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: 'auto', // Top position relative to parent in px
        left: 'auto', // Left position relative to parent in px,
        opacity: 1 / 4
    };

    angular.module('sw_layout').factory('spinService', ['$rootScope', spinService]);

    const defaultSpingParams= {
        savingDetail:false,
        compositionSpin:false
    }

    function spinService($rootScope) {

        let ajaxspin;
        let compositionspin;

        const service = {
            start: start,
            stop: stop,
            startSpinner: startSpinner
        };

        return service;

        function mergeOptions(options) {
            var merged = angular.copy(!!options.small ? smallOpts : defaultOptions);
            angular.forEach(merged, function(val, key) {
                if (!!options[key]) merged[key] = options[key];
            });
            return merged;
        }

        /**
         * Starts a spinner animation within the given target element.
         * 
         * @param DOMNode target 
         * @param boolean small 
         * @returns Spinner the spinner that was intantiated 
         */
        function startSpinner(target, options) {
            const merged = mergeOptions(options);
            return new Spinner(merged).spin(target);
        }

      

        function start({savingDetail,compositionSpin}=defaultSpingParams) {
            if ($rootScope.showingspin) {
                //if already showing no action needed
                return;
            }
            const spinDivId = savingDetail ? 'detailspinner' : 'mainspinner';
            const optsToUse = savingDetail ? smallOpts : defaultOptions;
            const spinner = document.getElementById(spinDivId);
            $rootScope.showingspin = true;
            if (compositionSpin) {
                compositionspin = new Spinner(optsToUse).spin(spinner);
            } else {
                ajaxspin = new Spinner(optsToUse).spin(spinner);
            }
        }

        function stop({compositionSpin}=defaultSpingParams) {

            const spinToUse = compositionSpin ? compositionspin : ajaxspin;
            if (spinToUse) {
                $rootScope.showingspin = false;
                spinToUse.stop();
            }
        }

    }
})(angular, Spinner);
