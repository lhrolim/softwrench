(function (angular, Spinner) {
    'use strict';

    const defaultOptions = {
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

    const smallOpts = {
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


    const extraSmallOpts = {
        lines: 13, // The number of lines to draw
        length: 4, // The length of each line
        width: 2, // The line thickness
        radius: 4, // The radius of the inner circle
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

 

    

    const defaultSpingParams= {
        savingDetail:false,
        compositionSpin:false
    }

    class spinService{

        constructor($rootScope) {
            this.$rootScope = $rootScope;
            this.ajaxspin = null;
            this.compositionSpin = null;
        }

        mergeOptions(options) {
            let merged;
            if (!!options.small) {
                merged = angular.copy(smallOpts);
            }else if (!!options.extraSmall) {
                merged = angular.copy(extraSmallOpts);
            } else {
                merged = angular.copy(defaultOptions);
            }
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
        startSpinner(target, options) {
            const merged = this.mergeOptions(options);
            return new Spinner(merged).spin(target);
        }

        start({savingDetail,compositionSpin}=defaultSpingParams) {
            if (this.$rootScope.showingspin) {
                //if already showing no action needed
                return;
            }
            const spinDivId = savingDetail ? 'detailspinner' : 'mainspinner';
            const optsToUse = savingDetail ? smallOpts : defaultOptions;
            const spinner = document.getElementById(spinDivId);
            this.$rootScope.showingspin = true;
            if (compositionSpin) {
                this.compositionspin = new Spinner(optsToUse).spin(spinner);
            } else {
                this.ajaxspin = new Spinner(optsToUse).spin(spinner);
            }
        }

        stop({compositionSpin}=defaultSpingParams) {

            const spinToUse = compositionSpin ? this.compositionspin : this.ajaxspin;
            if (spinToUse) {
                this.$rootScope.showingspin = false;
                spinToUse.stop();
            }
        }

    }

    spinService.$inject = ['$rootScope'];

    angular.module('sw_layout').service('spinService', spinService);

})(angular, Spinner);
