var app = angular.module('sw_layout');

app.factory('menuService', function ($rootScope, redirectService, contextService, i18NService, $log) {

    "ngInject";

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

                if (leafs[i].type == "ApplicationMenuItemDefinition") {
                    leafUrl = redirectService.getApplicationUrl(leafs[i].application, leafs[i].schema, leafs[i].mode, i18NService.getI18nMenuLabel(leafs[i].title, false));
                } else if (leafs[i].type == "ActionMenuItemDefinition") {
                    leafUrl = redirectService.getActionUrl(leafs[i].title, leafs[i].controller, leafs[i].action, leafs[i].parameters);
                } else if (leafs[i].type == "MenuContainerDefinition") {
                    var leaf = searchMenuLeafByUrl(leafs[i].leafs, url);
                    if (leaf != null) {
                        return leaf;
                    }
                }

                if (leafUrl != null && decodeURI(leafUrl) == decodeURI(url)) {
                    return leafs[i];
                }
            }
        }
    };

    return {



        executeById: function (menuId) {
            var leafs = $rootScope.menu.leafs;
            var leaf = locateLeafById(leafs, menuId);
            if (leaf.type == "ApplicationMenuItemDefinition") {
                this.goToApplication(leaf, null);
            } else if (leaf.type == "ActionMenuItemDefinition") {
                this.doAction(leaf, null);
            }
        },


        doAction: function (leaf, target) {
            if (target != undefined) {
                this.setActiveLeaf(target);
            }
            contextService.insertIntoContext('currentgridarray', null);
            if (leaf.parameters && leaf.parameters.popupmode == "browser") {
                //HAP-813 --> since we´ll open that into a new browser window, let´s make sure we don´t change the main window module
                contextService.insertIntoContext("currentmodulenewwindow", leaf.module);
            } else {
                contextService.insertIntoContext("currentmodule", leaf.module);
                contextService.insertIntoContext('currentmetadata', null);
            }
            $log.getInstance('sw4.menu').info("current module: " + leaf.module);

            var title = ''; /* 'New ' +  leaf.parameters['application'] + " - " + leaf.title;*/
            redirectService.redirectToAction(title, leaf.controller, leaf.action, leaf.parameters, leaf.target);
        },

        goToApplication: function (leaf, target) {

            $rootScope.$broadcast('sw_checksuccessmessage', leaf);
            if (target != undefined) {
                this.setActiveLeaf(target);
            }
            $log.getInstance('sw4.menu').info("current module: " + leaf.module);
            
            contextService.insertIntoContext('currentgridarray', null);

            if (leaf.parameters && leaf.parameters.popupmode == "browser") {
                //HAP-813 --> since we´ll open that into a new browser window, let´s make sure we don´t change the main window module
                contextService.insertIntoContext("currentmodulenewwindow", leaf.module);
            } else {
                contextService.insertIntoContext("currentmodule", leaf.module);
                contextService.insertIntoContext('currentmetadata', null);
            }


            var parameters = {};

            if (leaf.parameters != null) {
                parameters.popupmode = leaf.parameters['popupmode'];
            }

            redirectService.goToApplicationView(leaf.application, leaf.schema, leaf.mode, this.getI18nMenuLabel(leaf, null), parameters);
        },


        adjustHeight: function (callbackFunction) {
            var menu = $("#applicationmenu");
            if (($rootScope.clientName != undefined && $rootScope.clientName == 'hapag') || menu.data('displacement') != 'vertical') {
                return;
            }
            var bodyHeight = $(".hapag-body").height();
            menu.children().first().css('min-height', bodyHeight + 4);
        },

        setActiveLeaf: function (leaf) {
            var menu = $("#applicationmenu");
            if (menu.data('displacement') == 'horizontal') {
                cleanSelectedLeaf();
                if ($(leaf).parents('#applicationmenu').length > 0) {
                    toggleSelectedLeaf(leaf);
                }
            }
        },

        getI18nMenuLabel: function (menuItem, tooltip) {

            if (menuItem.module != null) {
                return menuItem.title;
            }
            return i18NService.getI18nMenuLabel(menuItem, tooltip);
        },

        setActiveLeafByUrl: function (menu, url) {
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
    };

});


