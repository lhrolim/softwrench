var app = angular.module('sw_layout');


(function () {
    'use strict';

    angular.module('sw_layout').factory('menuService', ['$rootScope', 'redirectService', 'contextService', 'i18NService', 'securityService', 'checkpointService', '$log','userService', menuService]);

    function menuService($rootScope, redirectService, contextService, i18NService, securityService, checkpointService, $log, userService) {

        var cleanSelectedLeaf = function () {
            var menu = $("#applicationmenu");

            $("button", menu).removeClass("selected");
            $("a", menu).removeClass("selected");
        }

        var toggleSelectedLeaf = function (leaf) {

            // look for parent container of the new active menu item and its button
            var parentMenuContainer = $(leaf).parents('.dropdown-container').last();
            var menuContainerToggle = $('button', parentMenuContainer).first();

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

            for (var i = 0; i < leafs.length; i++) {
                var leaf = leafs[i];
                if (id == leaf.id) {
                    return leaf;
                }
                if (leaf.type == "MenuContainerDefinition") {
                    var result = locateLeafById(leaf.leafs, id);
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
                for (var i = 0; i < leafs.length; i++) {
                    var menuitem = leafs[i];
                    if (menuitem.type == "ApplicationMenuItemDefinition") {
                        leafUrl = redirectService.getApplicationUrl(menuitem.application, menuitem.schema, menuitem.mode, i18NService.getI18nMenuLabel(menuitem.title, false));
                    } else if (menuitem.type == "ActionMenuItemDefinition") {
                        leafUrl = redirectService.getActionUrl(menuitem.controller, menuitem.action, menuitem.parameters);
                    } else if (menuitem.type == "MenuContainerDefinition") {
                        var leaf = searchMenuLeafByUrl(menuitem.leafs, url);
                        if (leaf != null) {
                            return leaf;
                        }
                    }

                    if (leafUrl != null && decodeURI(leafUrl) == decodeURI(url)) {
                        return menuitem;
                    }
                }
            }
        };


        var service = {
            executeById: executeById,
            doAction: doAction,
            goToApplication: goToApplication,
            adjustHeight: adjustHeight,
            setActiveLeaf: setActiveLeaf,
            getI18nMenuLabel: getI18nMenuLabel,
            getI18nMenuIcon: getI18nMenuIcon,
            setActiveLeafByUrl: setActiveLeafByUrl,
            parseExternalLink:parseExternalLink

        };

        return service;


        function parseExternalLink(leaf) {
            var parameters = leaf.parameters;
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

            for (var parameter in parameters) {
                if (!parameters.hasOwnProperty(parameter)) {
                    continue;
                }
                var value = parameters[parameter];
                //let´s give it a chance for user properties to be set
                value =userService.readProperty(value);
                link += parameter + "=" + value;
                link += "&";
            }


            return link.substr(0,link.length-1);
        }

        function executeById(menuId) {

            var leafs = $rootScope.menu.leafs;
            var leaf = locateLeafById(leafs, menuId);
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

        function goToApplication(leaf, target) {
            if (!securityService.validateRoleWithErrorMessage(leaf.role)) {
                return;
            }

            //TODO: is this needed?
            //$rootScope.$broadcast('sw_checksuccessmessage', leaf);

            if (target != undefined) {
                this.setActiveLeaf(target);
            }
            $log.getInstance('sw4.menu').info("current module: " + leaf.module);
            contextService.insertIntoContext('currentmetadata', null);
            contextService.insertIntoContext('currentgridarray', null);
            contextService.insertIntoContext("currentmodule", leaf.module);

            var parameters = {};

            if (leaf.parameters != null) {
                parameters.popupmode = leaf.parameters['popupmode'];
            }
            checkpointService.clearCheckpoints();
            redirectService.goToApplicationView(leaf.application, leaf.schema, leaf.mode, this.getI18nMenuLabel(leaf, null), parameters);
        };


        function adjustHeight(callbackFunction) {
            var menu = $("#applicationmenu");
            if (($rootScope.clientName != undefined && $rootScope.clientName == 'hapag') || menu.data('displacement') != 'vertical') {
                return;
            }
            var bodyHeight = $(".hapag-body").height();
            menu.children().first().css('min-height', bodyHeight + 4);
        };

        function setActiveLeaf(leaf) {
            var menu = $("#applicationmenu");
            if (menu.data('displacement') == 'horizontal') {
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
                var leaf = searchMenuLeafByUrl(menu.leafs, url);

                if (leaf != null) {
                    var leafId = leaf.type + '_' + leaf.id;
                    var menuItem = $('#' + leafId);

                    if (menuItem.length > 0) {
                        cleanSelectedLeaf();
                        toggleSelectedLeaf(menuItem);
                    }
                }
            }
        }

    }
})();





