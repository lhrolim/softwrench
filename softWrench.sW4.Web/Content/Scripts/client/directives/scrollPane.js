(function (angular, app) {
    "use strict";

    app.directive('scrollPane', function (contextService, $log, $timeout) {
        return {
            restrict: 'E',
            replace: false,
            scope: {
                availablefn: '&'
            },

            link: function (scope, element, attrs) {
                var log = $log.getInstance('sw4.scrollPane');
                var scrollPaneData = null;

                function getContentHeight(scrollElement, available) {
                    //look for .jspPane to ensure we get the total content height
                    var contents = $('.jspPane', scrollElement).height();
                    if (contents == null) {
                        contents = scrollElement.height();
                    }

                    log.debug(contents, available);

                    //if the height was not set (no content), exit
                    if (contents == 0) {
                        return null;
                    }

                    //set pane height to smallest, contents or available area
                    if (contents > available) {
                        return available;
                    } else {
                        return contents;
                    }
                }

                function initScrollPane(pane, scroll) {
                    //create the scrollPane or reset it
                    if (pane == null) {
                        pane = scroll.jScrollPane().data('jsp');
                    } else {
                        pane.reinitialise();
                    }

                    return pane;
                }

                function setSrcollHeight() {
                    var scrollElement = $('.scroll', element);
                    var contentHeight = getContentHeight(scrollElement, scope.availablefn()());

                    if (contentHeight != null) {
                        scrollElement.height(contentHeight);
                        scrollPaneData = initScrollPane(scrollPaneData, scrollElement);
                    }

                    //log.debug(scrollPaneData);
                }

                var lazyLayout = window.debounce(setSrcollHeight, 200);
                $(window).resize(lazyLayout);
            }
        };
    });

})(angular, app);