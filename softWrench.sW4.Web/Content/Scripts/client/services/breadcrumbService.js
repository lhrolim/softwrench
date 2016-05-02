
(function (angular) {
    "use strict";

    function breadcrumbService($rootScope, $location, $log, i18NService, crudContextHolderService, historyService, contextService) {
        //#region Utils
        var convertAdminHtmlToLeafs = function (kids) {
            var leafs = [];

            for (var idx = 0; idx < kids.length; idx++) {
                if (kids[idx].localName != undefined) {
                    var iconClass = "";

                    switch (kids[idx].localName) {
                        case "li":
                            var link = kids[idx].firstElementChild;
                            var icon = link.firstElementChild;
                            var title = link.innerText.trim();

                            if (icon != null) {
                                if (icon.className != undefined) {
                                    for (var i = 0; i < icon.classList.length; i++) {
                                        if (icon.classList[i] !== "fa-fw") {
                                            iconClass += icon.classList[i] + " ";
                                        }
                                    }
                                }
                            }

                            if (kids[idx].className !== "user") {
                                var newObject = {
                                };
                                newObject.icon = iconClass.trim();
                                newObject.title = title;

                                if (link.attributes["ng-click"]) {
                                    var click = link.attributes["ng-click"].nodeValue;

                                    //rename the admin function, to avoid confilts
                                    click = click.replace("doAction", "$scope.adminDoAction");
                                    click = click.replace("$event", "null");
                                    click = click.replace("myProfile", "$scope.adminMyProfile");
                                    click = click.replace("loadApplication", "$scope.adminLoadApplication");
                                    click = click.replace("logout", "$scope.adminLogout");

                                    newObject.click = click;
                                    newObject.type = "AdminMenuItemDefinition";
                                }

                                if (kids[idx].children != null && kids[idx].children.length > 0) {
                                    var childLeafs = convertAdminHtmlToLeafs(kids[idx].children);

                                    if (childLeafs.length > 0) {
                                        newObject.leafs = childLeafs;
                                        newObject.type = "MenuContainerDefinition";
                                    }
                                }

                                leafs.push(newObject);
                            }

                            break;

                        case "ul":
                            if (kids[idx].children != null && kids[idx].children.length > 0) {
                                leafs = convertAdminHtmlToLeafs(kids[idx].children);
                            }

                            break;
                    }
                }
            }

            return leafs;
        }

        var getBreadcrumbTitle = function (title) {
            var string = title;

            if (title && title.indexOf("Detail") > -1) {
                var record = i18NService.getI18nRecordLabel(crudContextHolderService.currentSchema(), crudContextHolderService.rootDataMap());
                if (record) {
                    string = record;
                }
            }

            return string;
        }

        var getBreadcrumbIcon = function (title, isDetail) {
            var icon = "fa fa-circle-o";

            if (title.indexOf("Detail") > -1 || isDetail) {
                icon = "fa fa-file-text-o";
            }

            return icon;
        }

        var pageFoundInMenu = function (title) {
            var pageFound = false;
            var menu = getCurrentMenu(title);

            if (menu.explodedLeafs != null) {
                for (var id in menu.explodedLeafs) {
                    if (menu.explodedLeafs[id].hasOwnProperty("title")) {
                        if (menu.explodedLeafs[id].title === title) {
                            pageFound = true;
                        }
                    }
                }
            }

            return pageFound;
        }

        var buildUnknownMenuItemPage = function (title, url, isDetail) {
            var newPage = {};
            newPage.title = getBreadcrumbTitle(title);
            newPage.icon = getBreadcrumbIcon(title, isDetail);
            newPage.type = "UnknownMenuItemDefinition";
            if (url) {
                newPage.redirectURL = url;
            }
            return newPage;
        }

        // verifies if a page on history have to be changed to ellipsis
        var changeToEllipsis = function (historySize, index, selectedIndex) {
            // first, second and last page always shown
            if (index === 0 || index === 1 || index === historySize - 1) {
                return false;
            }

            // current item, before current and after current always shown
            if (index === selectedIndex || index === selectedIndex - 1 || index === selectedIndex + 1) {
                return false;
            }

            return true;
        }

        // calc the indexes of all pages that have to be changed to ellipsis
        var calcEllipsisIndexes = function (historySize, selectedIndex) {
            var ellipsisIndexes = [];
            // short list no need for ellipsis
            if (historySize < 7) {
                return ellipsisIndexes;
            }

            for (var i = 0; i < historySize; i++) {
                if (changeToEllipsis(historySize, i, selectedIndex)) {
                    ellipsisIndexes.push(i);
                }
            }

            var pagesShown = historySize - ellipsisIndexes.length;
            if (pagesShown < 6) {
                ellipsisIndexes.splice(0, 6 - pagesShown);
            }

            return ellipsisIndexes;
        }

        // add pages from history on breadcrumb page object
        // for now only pages that are not on menu could be on breadcrumb history
        // so it's safe to add all pages from history as a UnknownMenuItemDefinition
        var addPagesFromHistory = function (pages, currentIndexOnHistory) {
            var history = historyService.getBreadcrumbHistory();

            // do not show pages after the current
            // but keeps them on history (var history is changed but not updated on session)
            if (currentIndexOnHistory < history.length - 1) {
                var oneAfterCurrent = currentIndexOnHistory + 1;
                history.splice(oneAfterCurrent, history.length - oneAfterCurrent);
            }

            var lastPageWasEllipsis = false;
            var ellipsisIndexes = calcEllipsisIndexes(history.length, currentIndexOnHistory);

            history.forEach(function (historyEntry, index) {
                if (ellipsisIndexes.indexOf(index) >= 0) {
                    if (!lastPageWasEllipsis) {
                        pages.push({ type: "EllipsisItemDefinition" });
                    }
                    lastPageWasEllipsis = true;
                    return;
                }
                lastPageWasEllipsis = false;
                var page = buildUnknownMenuItemPage(historyEntry.title, historyEntry.url, true);
                page.historyIndex = index;
                pages.push(page);
            });
        }

        var findCurrentPages = function (leafs, currentTitle, applicationName, schemaTitle, indexOnHistory) {
            var pages = [];

            if (currentTitle == null) {
                //if the current title is null, use the first menu leaf as the current
                pages.push(leafs[0]);
                return pages;
            }

            for (var id in leafs) {
                if (!leafs[id].hasOwnProperty("title")) {
                    continue;
                }

                var childPages = findCurrentPages(leafs[id].leafs, currentTitle, applicationName, schemaTitle, indexOnHistory);

                //add page if current or decentant is the current page
                if (childPages.length > 0 || leafs[id].title === currentTitle) {
                    pages.push(leafs[id]);
                }

                //if decentants were found, add to the return
                if (childPages.length > 0) {
                    for (var x in childPages) {
                        if (childPages[x].hasOwnProperty("title") || childPages[x].type === "EllipsisItemDefinition") {
                            pages.push(childPages[x]);
                        }
                    }
                }

                if (!applicationName) {
                    continue;
                }

                //if the current leaf matches the current application
                var isParent = leafs[id].applicationContainer === applicationName;

                //if the lcurrent leaf is likely the parent
                var possibleParent = leafs[id].application === applicationName;
                possibleParent = possibleParent && (leafs[id].schema.toLowerCase().indexOf("list") > -1 || leafs[id].schema.toLowerCase().indexOf("grid") > -1);
                possibleParent = possibleParent && leafs[id].title !== schemaTitle;

                if ((isParent || possibleParent) && childPages.length === 0) {
                    //add to the breadcrumb
                    pages.push(leafs[id]);

                    // for now only pages that are not on menu could be on breadcrumb history
                    // so it's possible to iterate over history here on the deepest part of the recursion (no child pages)
                    if (indexOnHistory >= 0) {
                        addPagesFromHistory(pages, indexOnHistory);
                    }
                    else if (!pageFoundInMenu(currentTitle)) {
                        //add a breadcrumb item for the unknown page
                        pages.push(buildUnknownMenuItemPage(currentTitle));
                    }
                }
            }

            return pages;
        }

        var getCurrentMenu = function (title) {
            var currentItem = $('.admin-area .admin-menu .modern .dropdown-menu a:contains("' + title + '")');
            var menu = {};

            if (currentItem.hasOwnProperty(length)) {
                var mainMenu = $(".admin-area .admin-menu .modern > .dropdown-menu");
                var leafs = convertAdminHtmlToLeafs(mainMenu[0].children);
                menu.displacement = "admin";
                menu.leafs = leafs;
            } else {
                menu = $rootScope.menu;
            }

            return menu;
        };

        var getCurrentTitle = function (title) {
            var schema = crudContextHolderService.currentSchema();
            return schema ? schema.title : title;
        };

        var userInfo = function () {
            var user = contextService.getUserData();
            return user.firstName + " " + user.lastName;
        }
        //#endregion

        //#region Public methods

        function getBreadcrumbItems(title) {
            var log = $log.getInstance("breadcrumbService#getBreadcrumbItems");

            var currentTitle = getCurrentTitle(title);
            var currentMenu = getCurrentMenu(currentTitle);

            var indexOnHistory = historyService.indexOnBreadcrumbHistory();
            var history = historyService.getBreadcrumbHistory();
            if (indexOnHistory < 0) {
                // if it's not on history should erase the history in case the start of a new one is needed
                historyService.eraseBreadcrumbHistory();
            } else if (indexOnHistory === history.length - 1) {
                // updates the history title if the current page it's the last one on history
                // this is needed if the user goes back to another page on history and the last one 
                // still need to be shown
                historyService.updateBreadcrumbHistoryTitle(getBreadcrumbTitle(currentTitle));
            }

            var applicationName;
            var schemaTitle;

            if (indexOnHistory < 0) {
                // if not on history use the current schema info
                var schema = crudContextHolderService.currentSchema();
                applicationName = schema ? schema.applicationName : null;
                schemaTitle = schema ? schema.title : null;
            } else {
                // if on history gets the schema info for the first on history
                // this afects the application and grid pages on breadcrumb
                applicationName = history.length > 0 ? history[0].applicationName : null;
                schemaTitle = history.length > 0 ? history[0].schemaTitle : null;
            }

            var foundPages = findCurrentPages(currentMenu.leafs, currentTitle, applicationName, schemaTitle, indexOnHistory);

            //if no menu items where found, add a unknow item
            if (foundPages.length === 0 && (schemaTitle || currentTitle)) {
                foundPages.push(buildUnknownMenuItemPage(schemaTitle || currentTitle));
            }

            //add the settings menu
            if (currentMenu.displacement === "admin") {
                var settingsPage = {};
                settingsPage.icon = "fa fa-cog";
                settingsPage.title = 'Settings';
                settingsPage.leafs = currentMenu.leafs;
                settingsPage.type = "MenuContainerDefinition";
                foundPages.unshift(settingsPage);
            }

            //add the hamburger menu
            var hamburgerMenuPage = {};
            hamburgerMenuPage.icon = "fa fa-bars";
            hamburgerMenuPage.title = "Hamburger";
            hamburgerMenuPage.leafs = $rootScope.menu.leafs;
            hamburgerMenuPage.type = "MenuContainerDefinition";
            foundPages.unshift(hamburgerMenuPage);

            return foundPages;
        };

        //#endregion

        //#region Service Instance
        var service = {
            getBreadcrumbItems: getBreadcrumbItems
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("breadcrumbService", ["$rootScope", "$location", "$log", "i18NService", "crudContextHolderService", "historyService", "contextService", breadcrumbService]);

    //#endregion

})(angular);