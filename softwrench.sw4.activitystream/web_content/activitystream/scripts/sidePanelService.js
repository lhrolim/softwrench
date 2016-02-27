
(function (angular) {
    "use strict";

    function sidePanelService() {
        //#region Utils
        var openedPanel = null;

        // the sum of all handles and spaces between (including the first space)
        var sidePanelHandlesWidth = 0;

        var spaceBetweenHandles = 10;

        var sidePanelsContext = {};
        var sidePanelContextTemplate = {
            opened: false,
            hidden: false,
            icon: null,
            title: null,
            top: 0,
            handleWidth: 40,
            toggleCallback: null
        }

        // resize screen components to consider the side panel expanded
        function handleResize() {
            var activityWidth = 0;

            //if pane is open get width
            if (openedPanel != null) {
                activityWidth = $("#" + openedPanel).width();
            }

            //update widths
            $(".site-header").width($(".site-header").css("width", "calc(100% - " + activityWidth + "px)"));

            if ($(".site-header").css("position") === "fixed") {
                $(".affix-pagination").width($(".affix-pagination").css("width", "calc(100% - " + activityWidth + "px)"));
            } else {
                $(".affix-pagination").width($(".affix-pagination").css("width", "100%"));
            }

            $(".listgrid-thead").width($(".listgrid-thead").css("width", "calc(100% - " + activityWidth + "px)"));
            $(".content").width($(".content").css("width", "calc(100% - " + activityWidth + "px)"));
        }

        var handler = window.debounce(handleResize, 300);
        angular.element(window).on("resize", handler);

        // calc the next panel handle top value
        function calcNextTop(handleWidth) {
            var top = spaceBetweenHandles;
            for (var otherPanelid in sidePanelsContext) {
                if (!sidePanelsContext.hasOwnProperty(otherPanelid) || sidePanelsContext[otherPanelid].hidden) {
                    continue;
                }
                top += sidePanelsContext[otherPanelid].handleWidth + spaceBetweenHandles;
            }
            sidePanelHandlesWidth = top + handleWidth;
            top += Math.floor(handleWidth / 2);
            return top;
        }

        // recalcs all panel handles top value
        // used when a handle is hidden or shown
        function recalcAllTops() {
            var currentTop = 0;
            for (var otherPanelid in sidePanelsContext) {
                if (!sidePanelsContext.hasOwnProperty(otherPanelid) || sidePanelsContext[otherPanelid].hidden) {
                    continue;
                }
                currentTop += spaceBetweenHandles;
                var ctx = sidePanelsContext[otherPanelid];
                ctx.top = currentTop + Math.floor(ctx.handleWidth / 2);
                currentTop += ctx.handleWidth;
            }
            sidePanelHandlesWidth = currentTop;
        }
        //#endregion

        //#region Public methods

        function isAnyPanelOpened() {
            return openedPanel != null;
        }

        function registerPanel(panelid, handleWidth) {
            var sidePanelContext = angular.copy(sidePanelContextTemplate);

            var numberWidth = parseInt(handleWidth);
            if (!isNaN(numberWidth)) {
                sidePanelContext.handleWidth = numberWidth;
            }
            sidePanelContext.top = calcNextTop(sidePanelContext.handleWidth);
            sidePanelsContext[panelid] = sidePanelContext;
        }

        function getContext(panelid) {
            return sidePanelsContext[panelid];
        }

        function setIcon(panelid, icon) {
            getContext(panelid).icon = icon;
        }

        function setTitle(panelid, title) {
            getContext(panelid).title = title;
        }

        function setHandleWidth(panelid, handleWidth) {
            var numberWidth = parseInt(handleWidth);
            if (isNaN(numberWidth)) {
                return;
            }
            getContext(panelid).handleWidth = numberWidth;
            recalcAllTops();
        }

        function toggle(panelid) {
            var ctx = getContext(panelid);

            // the panel with the given id was open
            // side panels have to colapse
            if (openedPanel === panelid) {
                openedPanel = null;
                ctx.opened = false;
            }

            // no panel was opened
            // expands the panel with the given id
            else if (openedPanel === null) {
                openedPanel = panelid;
                ctx.opened = true;
            }
            // another panel was opened
            // give focus to the panel with the given id
            else {
                ctx.opened = true;
                getContext(openedPanel).opened = false;
                openedPanel = panelid;
            }

            //resize/position elements
            $(window).trigger('resize');

            // callback
            if (ctx.toggleCallback) {
                ctx.toggleCallback(ctx);
            }
        }

        // hide panel completely from view (not colapse/expand)
        function hide(panelid) {
            // colapses if expanded
            if (openedPanel === panelid) {
                toggle(panelid);
            }

            getContext(panelid).hidden = true;
            recalcAllTops();
        }

        function show(panelid) {
            getContext(panelid).hidden = false;
            recalcAllTops();
        }

        function getNumberOfVisiblePanels() {
            var number = 0;
            for (var otherPanelid in sidePanelsContext) {
                if (!sidePanelsContext.hasOwnProperty(otherPanelid)) {
                    continue;
                }
                var ctx = sidePanelsContext[otherPanelid];
                if (!ctx.hidden) {
                    number++;
                }
            }
            return number;
        }

        function getTotalHandlesWidth() {
            return sidePanelHandlesWidth;
        }
        //#endregion

        //#region Service Instance
        var service = {
            isAnyPanelOpened: isAnyPanelOpened,
            registerPanel: registerPanel,
            setIcon: setIcon,
            setTitle: setTitle,
            setHandleWidth: setHandleWidth,
            getContext: getContext,
            toggle: toggle,
            hide: hide,
            show: show,
            getNumberOfVisiblePanels: getNumberOfVisiblePanels,
            getTotalHandlesWidth: getTotalHandlesWidth
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("sidePanelService", [sidePanelService]);

    //#endregion

})(angular);