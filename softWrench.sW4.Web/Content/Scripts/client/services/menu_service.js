(function (angular) {
    'use strict';

    angular.module('sw_layout').service('menuService', ['$rootScope', 'redirectService', 'contextService', 'i18NService', 'securityService', 'checkpointService', '$log', 'userService', 'gridPreferenceService', menuService]);

    function menuService($rootScope, redirectService, contextService, i18NService, securityService, checkpointService, $log, userService, gridPreferenceService) {

        var cleanSelectedLeaf = function () {
            const menu = $("#applicationmenu");
            $("button", menu).removeClass("selected");
            $("a", menu).removeClass("selected");
        }

        var toggleSelectedLeaf = function (leaf) {

            // look for parent container of the new active menu item and its button
            const parentMenuContainer = $(leaf).parents('.dropdown-container').last();
            const menuContainerToggle = $('button', parentMenuContainer).first();
            if (menuContainerToggle.length == 0) {
                $(leaf).addClass("selected");
            } else {
                menuContainerToggle.addClass("selected");
            }
        }

        var locateLeafById = function (leafs, id) {

            if (id == null) {
                throw new Error("id cannot be null");
            }

            if (leafs == null) {
                return null;
            }

            for (let i = 0; i < leafs.length; i++) {
                const leaf = leafs[i];
                if (id == leaf.id) {
                    return leaf;
                }
                if (leaf.type == "MenuContainerDefinition") {
                    const result = locateLeafById(leaf.leafs, id);
                    if (result != null) {
                        return result;
                    }
                }
            }
            return null;
        };

        var searchMenuLeafByUrl = function (leafs, url) {

            var leafUrl;

            if (leafs != null) {
                for (let i = 0; i < leafs.length; i++) {
                    const menuitem = leafs[i];
                    if (menuitem.type == "ApplicationMenuItemDefinition") {
                        leafUrl = redirectService.getApplicationUrl(menuitem.application, menuitem.schema, menuitem.mode, i18NService.getI18nMenuLabel(menuitem.title, false));
                    } else if (menuitem.type == "ActionMenuItemDefinition") {
                        leafUrl = redirectService.getActionUrl(menuitem.controller, menuitem.action, menuitem.parameters);
                    } else if (menuitem.type == "MenuContainerDefinition") {
                        const leaf = searchMenuLeafByUrl(menuitem.leafs, url);
                        if (leaf != null) {
                            return leaf;
                        }
                    }

                    if (leafUrl != null && decodeURI(leafUrl) === decodeURI(url)) {
                        return menuitem;
                    }
                }
            }
        };

        const service = {
            executeById,
            doAction,
            goToApplication,
            adjustHeight,
            setActiveLeaf,
            getI18nMenuLabel,
            getI18nMenuIcon,
            setActiveLeafByUrl,
            parseExternalLink

        };
        return service;


        function parseExternalLink(leaf) {
            const parameters = leaf.parameters;
            if (!leaf.link.startsWith("http")) {
                leaf.link = "http://" + leaf.link;
            }


            if (parameters == null) {
                return leaf.link;
            }

            var link = leaf.link;
            if (!link.endsWith("?")) {
                link = link + "?";
            }

            for (let parameter in parameters) {
                if (!parameters.hasOwnProperty(parameter)) {
                    continue;
                }
                let value = parameters[parameter];
                //let´s give it a chance for user properties to be set
                value = userService.readProperty(value);
                link += parameter + "=" + value;
                link += "&";
            }


            return link.substr(0, link.length - 1);
        }

        function executeById(menuId) {
            const leafs = $rootScope.menu.leafs;
            const leaf = locateLeafById(leafs, menuId);
            if (leaf.type == "ApplicationMenuItemDefinition") {
                this.goToApplication(leaf, null);
            } else if (leaf.type == "ActionMenuItemDefinition") {
                this.doAction(leaf, null);
            }
        };

        function doAction(leaf, target) {
            if (!securityService.validateRoleWithErrorMessage(leaf.role)) {
                return;
            }

            if (target != undefined) {
                this.setActiveLeaf(target);
            }
            contextService.insertIntoContext('currentmetadata', null);
            contextService.insertIntoContext('currentgridarray', null);
            contextService.insertIntoContext("currentmodule", leaf.module);
            $log.getInstance('sw4.menu').info("current module: " + leaf.module);
            checkpointService.clearCheckpoints();
            redirectService.redirectToAction(leaf.title, leaf.controller, leaf.action, leaf.parameters, leaf.target);
        };

        function goToApplication(leaf, target, parameters) {
            if (!securityService.validateRoleWithErrorMessage(leaf.role)) {
                return;
            }


            if (target != undefined) {
                this.setActiveLeaf(target);
            }
            $log.getInstance('sw4.menu').info("current module: " + leaf.module);
            contextService.insertIntoContext('currentmetadata', null);
            contextService.insertIntoContext('currentgridarray', null);
            contextService.insertIntoContext("currentmodule", leaf.module);

            parameters = parameters ? parameters : {};

            if (leaf.parameters != null) {
                parameters.popupmode = leaf.parameters['popupmode'];
            }
            checkpointService.clearCheckpoints();
            $rootScope.$broadcast('sw.redirect', leaf);

            const previousFilter = gridPreferenceService.getPreviousFilterDto(leaf.application, leaf.schema);
            if (previousFilter) {
                const previousDTO = previousFilter.searchDTO;
                return redirectService.redirectWithData(leaf.application, leaf.schema, previousDTO.searchData, { searchDTO: previousDTO }).then(data => {
                    $rootScope.$broadcast(JavascriptEventConstants.GRID_SETFILTER, previousFilter);
                });
            }
            return redirectService.goToApplicationView(leaf.application, leaf.schema, leaf.mode, this.getI18nMenuLabel(leaf, null), parameters);

        };


        function adjustHeight(callbackFunction) {
            const menu = $("#applicationmenu");
            if (($rootScope.clientName != undefined && $rootScope.clientName == 'hapag') || menu.data('displacement') != 'vertical') {
                return;
            }
            const bodyHeight = $(".hapag-body").height();
            menu.children().first().css('min-height', bodyHeight + 4);
        };

        function setActiveLeaf(leaf) {
            const menu = $("#applicationmenu");
            if (menu.data('displacement') === 'horizontal') {
                cleanSelectedLeaf();
                if ($(leaf).parents('#applicationmenu').length > 0) {
                    toggleSelectedLeaf(leaf);
                }
            }
        };


        function getI18nMenuLabel(menuItem, tooltip) {

            if (menuItem.module != null) {
                return menuItem.title;
            }
            return i18NService.getI18nMenuLabel(menuItem, tooltip);
        };


        function getI18nMenuIcon(menuItem) {

            if (menuItem.module != null) {
                return menuItem.icon;
            }
            return i18NService.getI18nMenuIcon(menuItem);
        };


        function setActiveLeafByUrl(menu, url) {
            if (menu.displacement == 'horizontal') {
                const leaf = searchMenuLeafByUrl(menu.leafs, url);
                if (leaf != null) {
                    const leafId = leaf.type + '_' + leaf.id;
                    const menuItem = $('#' + leafId);
                    if (menuItem.length > 0) {
                        cleanSelectedLeaf();
                        toggleSelectedLeaf(menuItem);
                    }
                }
            }
        }

    }
})(angular);





