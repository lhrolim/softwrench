(function (angular, app) {
    "use strict";

    app.directive('scrollPane', function (contextService, $log, $timeout, $window) {
        return {
            restrict: 'E',
            replace: false,
            scope: {
                availableFn: '&',
                preventWindowScroll: '=',
                useAvailableHeight: '='
            },

            link: function (scope, element, attrs) {
                scope.$name = "scrollPane";

                var log = $log.getInstance('sw4.scrollPane', ["layout"]);
                log.debug("init scrollpane");

                var scrollPaneData = null;
                var scrollElement = $('.scroll', element);

                //TODO: performance: improve
                scope.$watch(
                    function () {
                        log.trace('checking scroll pane');
                        var t0 = performance.now();
                        const scrollParent = $(element[0].offsetParent).is(':visible');

                        //if scroll pane exists and parent is visible
                        if (!scrollPaneData && !scrollParent) {
                            return;
                        }
                        const length = element[0].innerHTML.length;

                        log.debug('checking scroll pane finish, took:(ms) ' + (performance.now() - t0));
                        return length;
                    },
                    function (newValue, oldValue) {
                        if (newValue !== oldValue) {
                            log.trace('content changed, resize scroll pane');

                            //allow the parent to update before resize
                            lazyLayout();
                        }
                    }
                );

                function getContentHeight(scrollElement, available) {
                    //if set use the avaialbe height as the pane size
                    if (scope.useAvailableHeight) {
                        return available;
                    }

                    //look for .jspPane to ensure we get the total content height
                    var contents = $('.jspPane', scrollElement).height();
                    if (contents == null) {
                        contents = scrollElement.height();
                    }

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
                    //create the scrollPane
                    if (pane == null) {
                        pane = scroll.jScrollPane().data('jsp');
                    }

                    //add a delay to resize the scroll panes (to account for opening the sidePanels)
                    $timeout(function () {
                        pane.reinitialise();
                    }, 200, false);

                    return pane;
                }

                function setScrollHeight() {
                    const contentHeight = getContentHeight(scrollElement, scope.availableFn()());
                    if (contentHeight != null) {
                        scrollElement.height(contentHeight);
                        scrollPaneData = initScrollPane(scrollPaneData, scrollElement);
                    }

                    //log.debug(scrollPaneData);
                }

                function stopWindowScroll(prevent) {
                    //prevent window scrolling after reaching end of navigation pane if enabled
                    if (prevent) {
                        scrollElement.on('mousewheel', function (e) {
                            const delta = e.originalEvent.wheelDelta;
                            this.scrollTop += (delta < 0 ? 1 : -1) * 30;
                            e.preventDefault();
                        });
                    }
                }
                stopWindowScroll(scope.preventWindowScroll);

                var lazyLayout = window.debounce(setScrollHeight, 100);
                $(window).resize(lazyLayout);

            }
        };
    });

})(angular, app);