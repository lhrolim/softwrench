var app = angular.module('sw_layout');

function mapDOM(element, json) {
    var treeObject = {};

    // If string convert to document Node
    if (typeof element === "string") {
        if (window.DOMParser) {
            parser = new DOMParser();
            docNode = parser.parseFromString(element, "text/xml");
        } else { // Microsoft strikes again
            docNode = new ActiveXObject("Microsoft.XMLDOM");
            docNode.async = false;
            docNode.loadXML(element);
        }
        element = docNode.firstChild;
    }

    //Recursively loop through DOM elements and assign properties to object
    function treeHTML(element, object) {
        object["type"] = element.nodeName;
        var nodeList = element.childNodes;
        if (nodeList != null) {
            if (nodeList.length) {
                object["content"] = [];
                for (var i = 0; i < nodeList.length; i++) {
                    if (nodeList[i].nodeType == 3) {
                        object["content"].push(nodeList[i].nodeValue);
                    } else {
                        object["content"].push({});
                        treeHTML(nodeList[i], object["content"][object["content"].length - 1]);
                    }
                }
            }
        }
        if (element.attributes != null) {
            if (element.attributes.length) {
                object["attributes"] = {};
                for (var i = 0; i < element.attributes.length; i++) {
                    object["attributes"][element.attributes[i].nodeName] = element.attributes[i].nodeValue;
                }
            }
        }
    }
    treeHTML(element, treeObject);

    return (json) ? JSON.stringify(treeObject) : treeObject;
}

app.directive('breadcrumb', function ($rootScope, $log, $compile, menuService) {
    var log = $log.getInstance('sw4.breadcrumb');
    //log.debug($rootScope);

    return {
        scope: {
            schema: '=',
            menu: '=',
            title: '='
        },
        link: function (scope, element, attr) {


            scope.$watch('title', function (newValue, oldValue) {
                log.debug(scope.menu);
                //log.debug(scope, scope.schema1);
                //build breadcrumb based on menu.xml

                var template;
                //var template = '<div class="part main" bc-menu>';
                //template += '<a data-toggle="dropdown" aria-expanded="false">';
                //template += '<i class="fa fa-bars"></i>';
                //template += '&ensp;<i class="fa fa-caret-down"></i>';
                //template += '</a>';



                //template += seperator;
                //template += '</div>';

                //log.debug('template', template, seperator);

                //if (scope.title) {

                //log.debug(menuItem);

                //var menuIcon;
                //log.debug(scope.title, currentItem, template != null);

                //if the current page is in the admin menu
                var currentItem = $('.admin-area .admin-menu .dropdown-menu a:contains("' + scope.title + '")');
                if (currentItem.hasOwnProperty(length)) {
                    //menuIcon = menuItem[0].firstChild;
                    //console.log(menuItem, menuIcon);

                    //
                    template = getBreadCrumbHTML(log, scope.menu, undefined, menuService);
                    template += seperator;

                    var mainItem = $('.admin-area .admin-menu > a');

                    if (mainItem.hasOwnProperty(length)) {
                        log.debug(mainItem, mainItem[0].firstChild);



                        template += '<span class="part">';
                        template += '<a>';
                        template += mainItem[0].firstElementChild.outerHTML;
                        template += '&ensp;';
                        template += 'Admin';
                        template += '&ensp;';
                        template += '</a>';
                        template += '</span>';
                    }
                    //TODO: parse admin menu
                    //TODO: build admin menu

                } else {
                    template = getBreadCrumbHTML(log, scope.menu, scope.title, menuService);
                }
                //log.debug(template);



                var mainMenu = $('.admin-area .admin-menu > .dropdown-menu');
                //var children = mainMenu[0].children;
                var leafs = getAdminLeafs(log, mainMenu[0].children);

                var adminMenu = {};
                adminMenu.leafs = leafs;

                log.debug(mainMenu, adminMenu);

                //if (children.hasOwnProperty(length)) {
                //    //for (index in children) {
                //    for (index = 1; index < children.length; index++) {
                //        //console.log(mainMenu[0].children[index].firstElementChild.firstElementChild);


                //        if (children[index].localName != undefined) {
                //            var iconClass = '';

                //            //log.debug(children[index], children[index].localName);
                //            if (children[index].localName == 'li') {
                //                var link = children[index].firstElementChild;
                //                var icon = link.firstElementChild;

                //                log.debug(link.innerText.trim());

                //                if (icon != null) {
                //                    if (icon.className != undefined) {

                //                        //for (idx in icon.classList) {
                //                        //    log.debug(icon.classList[idx]);
                //                        //}

                //                        for (i = 0; i < icon.classList.length; i++) {
                //                            //text += cars[i] + "<br>";
                //                            //log.debug(icon.classList[i]);

                //                            if (icon.classList[i] != 'fa-fw') {
                //                                iconClass += icon.classList[i] + ' ';
                //                            }
                //                        }

                //                       // ,,innerText

                //                        //iconClass
                //                        log.debug(iconClass);
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                // Test with an element.
                //var initElement = document.getElementsByTagName("html")[0];
                //log.debug(initElement);

                //var json = mapDOM(mainMenu, true);
                //log.debug(json);

                // Test with a string.
                //initElement = "<div><span>text</span>Text2</div>";
                //json = mapDOM(initElement, true);
                //console.log(json);



                if (template != null) {
                    var content = $compile(template)(scope);
                    element.html(content);
                }
            });
        },

    }
});


app.directive('bcMenuItem', function ($rootScope, $log, $compile, menuService) {
    var log = $log.getInstance('sw4.breadcrumb Menu Item');

    return {
        link: function (scope, element, attr) {
            $compile(element.contents())(scope);
        },
        controller: function ($scope, alertService, validationService) {

            $scope.goToApplication = function (title) {
                var leaf = findleafByTitle(log, $scope.menu.leafs, title);
                var msg = "Are you sure you want to leave the page?";
                if (validationService.getDirty()) {
                    alertService.confirmCancel(null, null, function () {
                        menuService.goToApplication(leaf, null);
                        $scope.$digest();
                    }, msg, function () { return; });
                }
                else {
                    menuService.goToApplication(leaf, null);
                }
            };

            $scope.doAction = function (title) {
                //update title when switching to dashboard
                $scope.$emit('sw_titlechanged', null);

                var leaf = findleafByTitle(log, $scope.menu.leafs, title);
                var msg = "Are you sure you want to leave the page?";
                if (validationService.getDirty()) {
                    alertService.confirmCancel(null, null, function () {
                        menuService.doAction(leaf, null);
                        $scope.$digest();
                    }, msg, function () { return; });
                }
                else {
                    menuService.doAction(leaf, null);
                }
            };
        },
    }
});


function getAdminLeafs(log, kids) {
    var leafs = null;
    log.debug(kids, kids.length);


    //for (m in kids) {
    //    log.debug('found', kids[m]);
    //}

    //for (index = 0; index < kids.length; index++) {
    //    log.debug('found', kids[index]);
    //}




    //if (kids != null && kids.length > 0) {
    //    //for (index in kids) {
        for (idx = 0; idx < kids.length; idx++) {
    //        //console.log(mainMenu[0].kids[idx].firstElementChild.firstElementChild);

            //if (kids[idx].localName != undefined) {
            //    log.debug(idx, kids[idx]);
            //}

            if (kids[idx].localName != undefined) {
                var iconClass = '';

                log.debug(idx, kids[idx].localName, kids[idx]);

    //            //log.debug(kids[idx], kids[idx].localName);
                switch (kids[idx].localName) {
                    case 'a':
                        break;
                    case 'li':
    //            //if (kids[idx].localName == 'li') {
    //                    var link = kids[idx].firstElementChild;
    //                    var icon = link.firstElementChild;
    //                    var title = link.innerText.trim();

    //                    if (icon != null) {
    //                        if (icon.className != undefined) {

    //                            //for (idx in icon.classList) {
    //                            //    log.debug(icon.classList[idx]);
    //                            //}

    //                            for (i = 0; i < icon.classList.length; i++) {
    //                                //text += cars[i] + "<br>";
    //                                //log.debug(icon.classList[i]);

    //                                if (icon.classList[i] != 'fa-fw') {
    //                                    iconClass += icon.classList[i] + ' ';
    //                                }
    //                            }

    //                            // ,,innerText

    //                            //iconClass
    //                            //log.debug(iconClass);
    //                        }
    //                    }

    //                    var newObject = {};
    //                    newObject.icon = iconClass.trim();
    //                    newObject.title = title;
    //                    newObject.type = '';

    //                    //var leafs;
    //                    //log.debug(kids[idx].children);
                        if (kids[idx].children != null && kids[idx].children.length > 0) {
                            log.debug('get kids', kids[idx].children.length, kids[idx].children);

    //                        //for (x = 0; x < children[idx].children.length; x++) {
    //                        //    log.debug(children[idx].children[x]);
    //                        //}

    //                        var childLeafs;
                            //childLeafs = getAdminLeafs(log, kids[idx].children);

    //                        if (childLeafs != null) {
    //                            newObject.leafs = childLeafs;
    //                        }
                        }

    //                    leafs.push(newObject);
                        break;

                    case 'ul':
                        break;

                }
            }
       }
    //}

    return leafs;
}

function getAdminLeafsArchive(log, children) {
    var leafs = [];
    log.debug(children, children.length);


    //for (m in children) {
    //    log.debug('found', children[m]);
    //}

    for (index = 0; index < children.length; index++) {
        log.debug('found', children[index]);
    }




    if (children != null && children.length > 0) {
        //for (index in children) {
        for (index = 0; index < children.length; index++) {
            //console.log(mainMenu[0].children[index].firstElementChild.firstElementChild);

            if (children[index].localName != undefined) {
                log.debug(index, children[index]);
            }

            if (children[index].localName != undefined) {
                var iconClass = '';

                log.debug(children[index].localName);

                //log.debug(children[index], children[index].localName);
                switch (children[index].localName) {
                    case 'a':
                        break;
                    case '~li':
                        //if (children[index].localName == 'li') {
                        var link = children[index].firstElementChild;
                        var icon = link.firstElementChild;
                        var title = link.innerText.trim();

                        if (icon != null) {
                            if (icon.className != undefined) {

                                //for (idx in icon.classList) {
                                //    log.debug(icon.classList[idx]);
                                //}

                                for (i = 0; i < icon.classList.length; i++) {
                                    //text += cars[i] + "<br>";
                                    //log.debug(icon.classList[i]);

                                    if (icon.classList[i] != 'fa-fw') {
                                        iconClass += icon.classList[i] + ' ';
                                    }
                                }

                                // ,,innerText

                                //iconClass
                                //log.debug(iconClass);
                            }
                        }

                        var newObject = {};
                        newObject.icon = iconClass.trim();
                        newObject.title = title;
                        newObject.type = '';

                        //var leafs;
                        //log.debug(children[index].children);
                        if (children[index].children != null && children[index].children.length > 0) {
                            log.debug('get children', children[index].children);

                            //for (x = 0; x < children[index].children.length; x++) {
                            //    log.debug(children[index].children[x]);
                            //}

                            var childLeafs;
                            childLeafs = getAdminLeafs(log, children[index].children);

                            if (childLeafs != null) {
                                newObject.leafs = childLeafs;
                            }
                        }

                        leafs.push(newObject);
                        break;


                    case 'li':
                        //if (children[index].localName == 'li') {
                        //var link = children[index].firstElementChild;
                        //var icon = link.firstElementChild;
                        //var title = link.innerText.trim();

                        //if (icon != null) {
                        //if (icon.className != undefined) {

                        //for (idx in icon.classList) {
                        //    log.debug(icon.classList[idx]);
                        //}

                        //for (i = 0; i < icon.classList.length; i++) {
                        //text += cars[i] + "<br>";
                        //log.debug(icon.classList[i]);

                        //if (icon.classList[i] != 'fa-fw') {
                        //iconClass += icon.classList[i] + ' ';
                        //}
                        //}

                        // ,,innerText

                        //iconClass
                        //log.debug(iconClass);
                        //}
                        //}

                        //var newObject = {};
                        //newObject.icon = iconClass.trim();
                        //newObject.title = title;
                        //newObject.type = '';

                        //var leafs;
                        //log.debug(children[index].children);
                        if (children[index].children != null && children[index].children.length >= 5) {
                            log.debug('get children', children[index].children);

                            //for (x = 0; x < children[index].children.length; x++) {
                            //    log.debug(children[index].children[x]);
                            //}

                            var childLeafs;
                            childLeafs = getAdminLeafs(log, children[index].children);

                            //if (childLeafs != null) {
                            //    newObject.leafs = childLeafs;
                            //}
                        }

                        //leafs.push(newObject);
                        break;


                    case 'ul':
                        break;

                }



            }
        }
    }

    return leafs;
}

function getBreadCrumbHTML(log, menu, current, menuService) {
    var path = '<div class="part main" bc-menu>';
    path += '<a data-toggle="dropdown" aria-expanded="false">';
    path += '<i class="fa fa-bars"></i>';
    path += '&ensp;<i class="fa fa-caret-down"></i>';
    path += '</a>';

    //add submenu
    path += getChildMenu(log, menu.leafs, null, menuService);
    path += '</div>';

    //append child parts
    if (current != undefined) {
        var foundPath = findCurrentPage(log, menu.leafs, current, null);

        if (foundPath) {
            path += seperator;
            path += foundPath;
        } else {
            return null;
        }
    }

    return path;
}

function findCurrentPage(log, leafs, current, parent) {
    var path = '';

    if (leafs != null) {
        for (var id in leafs) {
            var newPath = findCurrentPage(log, leafs[id].leafs, current, leafs[id]);
            var icon = '<i class="' + leafs[id].icon + '"></i>&ensp;'

            //if this is part of the breadcrumb
            if ((newPath != undefined && newPath != '') || (leafs[id].title == current)) {

                //get the child menu items
                var childMenu = getChildMenu(log, leafs, leafs[id]);

                //build the breadcrumb part and menu
                path += '<div class="part">';

                //if child menu found, add the dropdown toggle
                if (newPath) {
                    path += '<a data-toggle="dropdown" aria-expanded="false">';
                } else {
                    path += '<a ng-click="';

                    if (leafs[id].type == 'ActionMenuItemDefinition') {
                        path += 'doAction';
                    } else if (leafs[id].type == 'ApplicationMenuItemDefinition') {
                        path += 'goToApplication';
                    } else if (leaf.type == 'ExternalLinkMenuItemDefinition') {
                        path += '<a target="_blank" href="{0}"'.format(leaf.link);
                    }

                    path += '(\'' + leafs[id].title + '\')">';
                }

                //add the icon and title
                path += icon + leafs[id].title;

                if (newPath) {
                    path += '&ensp;<i class="fa fa-caret-down"></i>';
                }

                path += '</a>';

                //add the child menu items
                if (newPath) {
                    path += getChildMenu(log, leafs, leafs[id]);
                }

                path += '</div>';

                //if found add the next breadcrumb part
                if (newPath != undefined && newPath != '') {
                    path += seperator + newPath;
                }
            }
        }
    }

    return path;
}

function findleafByTitle(log, leafs, title) {
    var found = null;

    if (leafs != null) {
        for (var id in leafs) {
            var search = findleafByTitle(log, leafs[id].leafs, title);

            //if a child is the current item, pass it along
            if (search != null) {
                found = search;
            }

            //if this is the current item
            if (leafs[id].title == title) {
                found = leafs[id];
            }
        }
    }

    return found;
}

function getChildMenu(log, leafs, parent, menuService) {
    var path = '';
    var searchLeafs = null;

    //if no parent use the get the whole menu, else the child items from the parent
    if (parent == null) {
        searchLeafs = leafs
    } else {
        if (parent.leafs != null) {
            searchLeafs = parent.leafs;
        }
    }

    if (searchLeafs != null) {
        path += '<ul class="dropdown-menu" role="menu" bc-menu>';
        for (var id in searchLeafs) {
            var leaf = searchLeafs[id];
            if (leaf.title != null) {
                var childMenu = getChildMenu(log, leaf.leafs, leaf, menuService);

                //if child menu found, display as submenu
                if (childMenu) {
                    path += '<li class="dropdown-submenu"><a data-toggle="dropdown" aria-expanded="false">';
                } else {
                    path += '<li><a bc-menu-item ng-click="';

                    if (leaf.type == 'ActionMenuItemDefinition') {
                        path += 'doAction';
                    } else if (leaf.type == 'ApplicationMenuItemDefinition') {
                        path += 'goToApplication';
                    } else if (leaf.type == 'ExternalLinkMenuItemDefinition') {
                        if (!leaf.link.startsWith("http")) {
                            leaf.link = "http://" + leaf.link;
                        }
                        var externalLink = menuService.parseExternalLink(leaf);
                        path += '\" target="_blank" href="{0}"'.format(externalLink);
                    }

                    path += '(\'' + leaf.title + '\')">';
                }

                //build the menu item
                path += '<i class="' + leaf.icon + '"></i>&ensp;' + leaf.title.trim();
                path += '</a>';

                //add the child menu items
                if (childMenu) {
                    path += childMenu;
                }

                path += '</li>';
            }
        }
        path += '</ul>';
    }

    return path;
}

var seperator = '<span class="part seperator">/</span>';
