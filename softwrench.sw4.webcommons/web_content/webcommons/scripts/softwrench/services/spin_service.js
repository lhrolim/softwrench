
(function (angular) {
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

    function spinService($rootScope) {

        var ajaxspin;

        var compositionspin;

        var service = {
            start: start,
            stop: stop,
        };

        return service;

        function start(parameters) {
            if ($rootScope.showingspin) {
                //if already showing no action needed
                return;
            }
            parameters = parameters || {};
            var savingDetail = parameters.savingDetail || false;
            var isComposition = parameters.compositionSpin || false;

            var spinDivId = savingDetail ? 'detailspinner' : 'mainspinner';
            var optsToUse = savingDetail ? smallOpts : defaultOptions;
            var spinner = document.getElementById(spinDivId);
            $rootScope.showingspin = true;
            if (isComposition) {
                compositionspin = new Spinner(optsToUse).spin(spinner);;
            } else {
                ajaxspin = new Spinner(optsToUse).spin(spinner);;
            }
        };

        function stop(parameters) {

            parameters = parameters || {};
            var isComposition = parameters.compositionSpin || false;

            var spinToUse = isComposition ? compositionspin : ajaxspin;

            if (spinToUse) {
                $rootScope.showingspin = false;
                spinToUse.stop();
            }
        }

    }
})(angular);
