(function (angular) {
    "use strict";

angular.module('ion-autocomplete', []).directive('ionAutocomplete', [
    '$ionicTemplateLoader', '$ionicBackdrop', '$ionicScrollDelegate', '$rootScope', '$document', '$q', '$parse', '$ionicPlatform','$log' ,
    function ($ionicTemplateLoader, $ionicBackdrop, $ionicScrollDelegate, $rootScope, $document, $q, $parse, $ionicPlatform, $log) {
        return {
            require: '?ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel',
                placeholder: '@',
                cancelLabel: '@',
                selectItemsLabel: '@',
                selectedItemsLabel: '@',
                templateUrl: '@',
                templateData: '=',
                itemsMethod: '&',
                itemsMethodValueKey: '@',
                itemValueKey: '@',
                itemViewValueKey: '@',
                multipleSelect: '@',
                itemsClickedMethod: '&',
                itemsRemovedMethod: '&',
                componentId: '@',
                modelToItemMethod: '&',
                loadingIcon: '@',
                hasUseWhereClause: '=',
                useWhereClauseLabel: '@'
            },
            link: function (scope, element, attrs, ngModel) {

                // do nothing if the model is not set
                if (!ngModel) return;

                // set the default values of the passed in attributes
                scope.placeholder = !scope.placeholder ? 'Search values...' : scope.placeholder;
                scope.cancelLabel = !scope.cancelLabel ? scope.multipleSelect === "true" ? 'Done' : 'Cancel' : scope.cancelLabel;
                scope.selectItemsLabel = !scope.selectItemsLabel ? 'Select an item...' : scope.selectItemsLabel;
                scope.selectedItemsLabel = !scope.selectedItemsLabel ? 'Selected items:' : scope.selectedItemsLabel;
                scope.templateUrl = !scope.templateUrl ? '' : scope.templateUrl;
                scope.loadingIcon = !scope.loadingIcon ? '' : scope.loadingIcon;
                scope.useWhereClauseLabel = scope.useWhereClauseLabel || "Show Less";
                scope.useWhereClause = true;

                // loading flag if the items-method is a function
                scope.showLoadingIcon = false;

                // the items, selected items and the query for the list
                scope.items = [];
                scope.selectedItems = [];
                scope.searchQuery = undefined;

                // returns the value of an item
                scope.getItemValue = function (item, key) {

                    // if it's an array, go through all items and add the values to a new array and return it
                    if (angular.isArray(item)) {
                        var items = [];
                        angular.forEach(item, function (itemValue) {
                            if (key && angular.isObject(item)) {
                                items.push($parse(key)(itemValue));
                            } else {
                                items.push(itemValue);
                            }
                        });
                        return items;
                    } else {
                        if (key && angular.isObject(item)) {
                            return $parse(key)(item);
                        }
                    }
                    return item;
                };

                // render the view value of the model
                ngModel.$render = function () {
                    element.val(scope.getItemValue(ngModel.$viewValue, scope.itemViewValueKey));
                };

                // set the view value of the model
                ngModel.$formatters.push(function (modelValue) {
                    var viewValue = scope.getItemValue(modelValue, scope.itemViewValueKey);
                    if (!viewValue) return "";
                    viewValue = replaceAll(viewValue, "undefined|null", "");
                    return viewValue.startsWith(" -") || viewValue.endsWith("- ") ? replaceAll(viewValue, " - | -|- ", "") : viewValue;
                });

                // set the model value of the model
                ngModel.$parsers.push(function (viewValue) {
                    return scope.getItemValue(viewValue, scope.itemValueKey);
                });

                // the search container template
                const searchContainerTemplate = [
                    '<div class="ion-autocomplete-container modal">',
                    '<div class="bar bar-header item-input-inset ion-autocomplete-topbar">',
                    '<label class="item-input-wrapper">',
                    '<i class="placeholder-icon fa fa-search"></i>',
                    '<input type="search" class="ion-autocomplete-search" ng-model="searchQuery" ng-model-options="{ debounce: 500 }" placeholder="{{placeholder}}"/>',
                    '</label>',
                    '<div class="ion-autocomplete-loading-icon" ng-if="showLoadingIcon && loadingIcon"><ion-spinner icon="{{loadingIcon}}"></ion-spinner></div>',
                    '<button class="ion-autocomplete-cancel button"><i class="fa fa-times-circle"></i>&ensp;{{cancelLabel}}</button>',
                    '</div>',
                    '<div class="bar bar-header item-input-inset ion-autocomplete-botbar" >',
                    '<div>{{useWhereClauseLabel}}</div>',
                    '<ion-toggle toggle-class="toggle-dark" ng-model="useWhereClause" ng-show="hasUseWhereClause"></ion-toggle>',
                    '<button class="ion-autocomplete-clear button"><i class="fa fa-eraser"></i>&ensp;Clear</button>',
                    '</div>',
                    '<ion-content class="has-header has-header" has-bouncing="false">',
                    '<ion-list>',
                    '<ion-item class="item-divider" ng-show="selectedItems.length > 0">{{selectedItemsLabel}}</ion-item>',
                    '<ion-item ng-repeat="selectedItem in selectedItems track by $index" type="item-text-wrap" class="item-icon-left item-icon-right">',
                    '<i class="fa fa-check"></i>',
                    '{{getItemValue(selectedItem, itemViewValueKey)}}',
                    '<i class="fa fa-trash" style="cursor:pointer" ng-click="removeItem($index)"></i>',
                    '</ion-item>',
                    '<ion-item class="item-divider" ng-show="items.length > 0">{{selectItemsLabel}}</ion-item>',
                    '<ion-item class="item-divider" ng-show="!items || items.length === 0">No results were found.</ion-item>',
                    '<ion-item collection-repeat="item in items" item-height="55" item-width="100%" type="item-text-wrap" ng-click="selectItem(item)" class="item item-text-wrap">',
                    '{{getItemValue(item, itemViewValueKey)}}',
                    '</ion-item>',
                    '</ion-list>',
                    '<ion-infinite-scroll ng-if="moreItemsAvailable" on-infinite="loadMore()" distance="10%"></ion-infinite-scroll>',
                    '</ion-content>',
                    '</div>'
                ].join('');

                scope.templateCompileStarted = false;

                const compileTemplate = function () {
                    if (scope.templateCompileStarted) {
                        return;
                    }
                    scope.templateCompileStarted = true;

                    // compile the popup template
                    $ionicTemplateLoader.compile({
                        templateUrl: scope.templateUrl,
                        template: searchContainerTemplate,
                        scope: scope,
                        appendTo: $document[0].body
                    }).then(function (compiledTemplate) {

                        // get the compiled search field
                        var searchInputElement = angular.element(compiledTemplate.element.find('input'));

                        // function which selects the item, hides the search container and the ionic backdrop if it is not a multiple select autocomplete
                        compiledTemplate.scope.selectItem = function (item) {

                            // clear the items and the search query
                            compiledTemplate.scope.items = [];
                            compiledTemplate.scope.searchQuery = undefined;

                            // if multiple select is on store the selected items
                            if (compiledTemplate.scope.multipleSelect === "true") {

                                if (!isKeyValueInObjectArray(compiledTemplate.scope.selectedItems,
                                        compiledTemplate.scope.itemValueKey, scope.getItemValue(item, scope.itemValueKey))) {
                                    // create a new array to update the model. See https://github.com/angular-ui/ui-select/issues/191#issuecomment-55471732
                                    compiledTemplate.scope.selectedItems = compiledTemplate.scope.selectedItems.concat([item]);
                                }

                                // set the view value and render it
                                ngModel.$setViewValue(compiledTemplate.scope.selectedItems);
                                ngModel.$render();
                            } else {
                                // set the view value and render it
                                ngModel.$setViewValue(item);
                                ngModel.$render();

                                // hide the container and the ionic backdrop
                                hideSearchContainer();
                            }

                            // call items clicked callback
                            if (angular.isFunction(compiledTemplate.scope.itemsClickedMethod)) {
                                compiledTemplate.scope.itemsClickedMethod({
                                    callback: {
                                        item: item,
                                        componentId: compiledTemplate.scope.componentId,
                                        selectedItems: compiledTemplate.scope.selectedItems.slice()

                                    }
                                });
                            }
                        };

                        // function which removes the item from the selected items.
                        compiledTemplate.scope.removeItem = function (index) {
                            // remove the item from the selected items and create a copy of the array to update the model.
                            // See https://github.com/angular-ui/ui-select/issues/191#issuecomment-55471732
                            var removed = compiledTemplate.scope.selectedItems.splice(index, 1)[0];
                            compiledTemplate.scope.selectedItems = compiledTemplate.scope.selectedItems.slice();

                            // set the view value and render it
                            ngModel.$setViewValue(compiledTemplate.scope.selectedItems);
                            ngModel.$render();

                            // call items clicked callback
                            if (angular.isFunction(compiledTemplate.scope.itemsRemovedMethod)) {
                                compiledTemplate.scope.itemsRemovedMethod({
                                    callback: {
                                        item: removed,
                                        selectedItems: compiledTemplate.scope.selectedItems.slice(),
                                        componentId: compiledTemplate.scope.componentId
                                    }
                                });
                            }
                        };

                        // object to store search container state
                        var searchContainer = {
                            showing: false
                        };

                        function doQuery(query, pageNumber) {
                            if (searchContainer && !searchContainer.showing) {
                                return;
                            }

                            pageNumber = pageNumber || 1;
                            compiledTemplate.scope.lastPage = pageNumber;

                            // right away return if the query is undefined to not call the items method for nothing
                            if (query === undefined) {
                                return;
                            }

                            // if the search query is empty, clear the items
                            if (query == '' && pageNumber === 1) {
                                compiledTemplate.scope.items = [];
                            }

                            if (angular.isFunction(compiledTemplate.scope.itemsMethod)) {

                                // show the loading icon
                                compiledTemplate.scope.showLoadingIcon = true;

                                const queryObject = {
                                    query: query,
                                    useWhereClause: compiledTemplate.scope.useWhereClause,
                                    pageNumber: pageNumber
                                };

                                // if the component id is set, then add it to the query object
                                if (compiledTemplate.scope.componentId) {
                                    queryObject["componentId"] = compiledTemplate.scope.componentId;
                                }

                                // convert the given function to a $q promise to support promises too
                                var promise = $q.when(compiledTemplate.scope.itemsMethod(queryObject));

                                promise.then(function (promiseData) {

                                    // if the given promise data object has a data property use this for the further processing as the
                                    // standard httpPromises from the $http functions store the response data in a data property
                                    if (promiseData && promiseData.data) {
                                        promiseData = promiseData.data;
                                    }

                                    // set the items which are returned by the items method
                                    const newItems = compiledTemplate.scope.getItemValue(promiseData, compiledTemplate.scope.itemsMethodValueKey);
                                    if (pageNumber === 1) {
                                        compiledTemplate.scope.items = newItems;
                                    } else {
                                        compiledTemplate.scope.$broadcast('scroll.infiniteScrollComplete');
                                        compiledTemplate.scope.items = compiledTemplate.scope.items.concat(newItems);
                                    }
                                    compiledTemplate.scope.moreItemsAvailable = newItems.length > 0;

                                    // force the collection repeat to redraw itself as there were issues when the first items were added
                                    $ionicScrollDelegate.resize();

                                    // hide the loading icon
                                    compiledTemplate.scope.showLoadingIcon = false;
                                }, function (error) {
                                    // hide the loading icon
                                    compiledTemplate.scope.showLoadingIcon = false;

                                    // reject the error because we do not handle the error here
                                    return $q.reject(error);
                                });
                            }
                        }

                        compiledTemplate.scope.loadMore = function () {
                            const query = compiledTemplate.scope.searchQuery === undefined ? "" : compiledTemplate.scope.searchQuery;
                            doQuery(query, compiledTemplate.scope.lastPage + 1);
                        }

                        // watcher on the search field model to update the list according to the input
                        compiledTemplate.scope.$watch('searchQuery', function (query) {
                            doQuery(query);
                        });

                        compiledTemplate.scope.$watch('useWhereClause', function () {
                            const query = compiledTemplate.scope.searchQuery === undefined ? "" : compiledTemplate.scope.searchQuery;
                            doQuery(query);
                        });

                        var displaySearchContainer = function () {
                            // container already showing: do nothing
                            if (searchContainer.showing) {
                                return;
                            }
                            $ionicBackdrop.retain();
                            compiledTemplate.element.css('display', 'block');
                            scope.$deregisterBackButton = $ionicPlatform.registerBackButtonAction(function () {
                                hideSearchContainer();
                            }, 300);
                            // mark container as showing
                            searchContainer.showing = true;
                        };

                        var hideSearchContainer = function () {
                            compiledTemplate.element.css('display', 'none');
                            $ionicBackdrop.release();
                            scope.$deregisterBackButton && scope.$deregisterBackButton();
                            // mark container as not showing anymore
                            searchContainer.showing = false;
                        };

                        // object to store if the user moved the finger to prevent opening the modal
                        var scrolling = {
                            moved: false,
                            startX: 0,
                            startY: 0
                        };

                        // store the start coordinates of the touch start event
                        var onTouchStart = function (e) {
                            $log.get("ionautocomplete#ontouchstart").trace("ontouchstart handler");
                            scrolling.moved = false;
                            // Use originalEvent when available, fix compatibility with jQuery
                            if (typeof (e.originalEvent) !== 'undefined') {
                                e = e.originalEvent;
                            }
                            scrolling.startX = e.touches[0].clientX;
                            scrolling.startY = e.touches[0].clientY;
                        };

                        // check if the finger moves more than 10px and set the moved flag to true
                        var onTouchMove = function (e) {
                            $log.get("ionautocomplete#ontouchmove").trace("ontouchmove handler");
                            // Use originalEvent when available, fix compatibility with jQuery
                            if (typeof (e.originalEvent) !== 'undefined') {
                                e = e.originalEvent;
                            }
                            if (Math.abs(e.touches[0].clientX - scrolling.startX) > 10 ||
                                Math.abs(e.touches[0].clientY - scrolling.startY) > 10) {
                                scrolling.moved = true;
                            }
                        };

                        // click handler on the input field to show the search container
                        const onClick = ionic.debounce(function (event) {
                            $log.get("ionautocomplete#onclick").trace("onclick handler");
                            // only open the dialog if was not touched at the beginning of a legitimate scroll event
                            if (scrolling.moved || ionic.scroll.isScrolling) {
                                return;
                            }

                            if (event) {
                                // prevent the default event and the propagation
                                event.preventDefault();
                                event.stopPropagation();
                            }

                            // show the ionic backdrop and the search container
                            displaySearchContainer();

                            doQuery("");

                            // focus on the search input field
                            if (searchInputElement.length > 0) {
                                searchInputElement[0].focus();
                                setTimeout(function () {
                                    searchInputElement[0].focus();
                                }, 0);
                            }

                            // force the collection repeat to redraw itself as there were issues when the first items were added
                            $ionicScrollDelegate.resize();
                        }, 0);

                        var isKeyValueInObjectArray = function (objectArray, key, value) {
                            for (var i = 0; i < objectArray.length; i++) {
                                if (scope.getItemValue(objectArray[i], key) === value) {
                                    return true;
                                }
                            }
                            return false;
                        };

                        // function to call the model to item method and select the item
                        var resolveAndSelectModelItem = function (modelValue) {
                            // convert the given function to a $q promise to support promises too
                            var promise = $q.when(compiledTemplate.scope.modelToItemMethod({ modelValue: modelValue }));

                            promise.then(function (promiseData) {
                                // select the item which are returned by the model to item method
                                compiledTemplate.scope.selectItem(promiseData);
                            }, function (error) {
                                // reject the error because we do not handle the error here
                                return $q.reject(error);
                            });
                        };

                        // bind the handlers to the click and touch events of the input field
                        element.bind('touchstart', onTouchStart);
                        element.bind('touchmove', onTouchMove);
                        element.bind('click focus', onClick);

                        // cancel handler for the cancel button which clears the search input field model and hides the
                        // search container and the ionic backdrop
                        $(compiledTemplate.element).find(".ion-autocomplete-cancel").bind('click', function (event) {
                            compiledTemplate.scope.searchQuery = undefined;
                            hideSearchContainer();
                        });

                        $(compiledTemplate.element).find(".ion-autocomplete-clear").bind('click', function (event) {
                            compiledTemplate.scope.items = [];
                            ngModel.$setViewValue(null);
                            ngModel.$render();
                            compiledTemplate.scope.searchQuery = undefined;
                            hideSearchContainer();
                        });

                        // prepopulate view and selected items if model is already set
                        if (compiledTemplate.scope.model && angular.isFunction(compiledTemplate.scope.modelToItemMethod)) {
                            if (compiledTemplate.scope.multipleSelect === "true" && angular.isArray(compiledTemplate.scope.model)) {
                                angular.forEach(compiledTemplate.scope.model, function (modelValue) {
                                    resolveAndSelectModelItem(modelValue);
                                });
                            } //else {
                            //resolveAndSelectModelItem(compiledTemplate.scope.model);
                            //}
                        }

                        // opens the modal on template compiled
                        onClick();
                    });
                }

                element.bind("touchend click focus", compileTemplate);
            }
        };
    }
]).directive('ionAutocomplete', function () {
    return {
        require: '?ngModel',
        restrict: 'E',
        template: '<input ion-autocomplete type="text" class="ion-autocomplete item-text-wrap" autocomplete="off" />',
        replace: true
    }
});

})(angular);
