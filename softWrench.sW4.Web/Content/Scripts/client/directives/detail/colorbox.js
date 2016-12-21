(function (angular, app) {
    "use strict";

    var defaultSettings = {
        close: '<i class="fa fa-times"></i>&nbsp;Close',
        previous: '<i class="fa fa-chevron-left"></i>&nbsp;Previous',
        next: '<i class="fa fa-chevron-right"></i>&nbsp;Next',
        maxHeight: '96%',
        maxWidth: '96%',
        opacity: 0.96
    }

    app.directive('colorboxRichtext', function () {
        return {

            link: function (scope, element, attrs) {
                $(element).addClass('colorbox');

                //handle output fields
                $(element).on('click', function (event) {

                    if (event.target.localName === 'img') {
                        var richtextSettings = {
                            html: event.target.outerHTML
                        };
                        var settings = $.extend({}, defaultSettings, richtextSettings);

                        //call the colorbox directly without an element trigger
                        $.colorbox(settings);
                    }

                });

                //handle input fields
                scope.$on('ta-element-select', function (event, target) {

                    if ($(target)[0].localName === 'img') {

                        var richtextSettings = {
                            html: $(target)[0].outerHTML
                        };
                        var settings = $.extend({}, defaultSettings, richtextSettings);

                        //call the colorbox directly without an element trigger
                        $.colorbox(settings);
                    }

                });
            }
        };
    });

})(angular, app);