
(function (angular) {
    "use strict";

    function sidePanelService(fixHeaderService, userPreferencesService) {
        //#region Utils
        var openedPanel = null;
        var expandedPanelPreferenceKey = "expandedSidePanel";

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
                $(".toolbar-primary").width($(".toolbar-primary").css("width", "calc(100% - " + activityWidth + "px)"));
            } else {
                $(".toolbar-primary").width($(".toolbar-primary").css("width", "100%"));
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

        // register the side panel - creates context and all
        // the next registers need this to know the panel handle top value
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

        function isOpened(panelid) {
            return openedPanel === panelid;
        }

        function toggle(panelid, mantainPreference) {
            var ctx = getContext(panelid);

            // the panel with the given id was open
            // side panels have to colapse
            if (isOpened(panelid)) {
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
            fixHeaderService.callWindowResize();
            $(window).trigger('resize');

            var newState = isOpened(panelid);
            if (!mantainPreference) {
                userPreferencesService.setPreference(expandedPanelPreferenceKey, openedPanel);
            }

            // callback
            if (ctx.toggleCallback) {
                ctx.toggleCallback(newState);
            }

            return newState;
        }

        function getExpandedPanelFromPreference() {
            return userPreferencesService.getPreference(expandedPanelPreferenceKey);
        }

        // hides panel completely from view (not collapse/expand)
        function hide(panelid, mantainPreference) {
            // colapses if expanded
            if (isOpened(panelid)) {
                toggle(panelid, mantainPreference);
            }

            getContext(panelid).hidden = true;
            recalcAllTops();
        }

        // unhides panel (not colapse/expand)
        function show(panelid) {
            getContext(panelid).hidden = false;
            recalcAllTops();
        }

        // used on other screen parts to know if handles are covering important stuff
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

        // used on other screen parts to know if handles are covering important stuff
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
            isOpened : isOpened,
            toggle: toggle,
            hide: hide,
            show: show,
            getNumberOfVisiblePanels: getNumberOfVisiblePanels,
            getTotalHandlesWidth: getTotalHandlesWidth,
            getExpandedPanelFromPreference: getExpandedPanelFromPreference
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("sidePanelService", ["fixHeaderService", "userPreferencesService", sidePanelService]);

    //#endregion

})(angular);