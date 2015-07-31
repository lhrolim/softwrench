
(function (angular) {
    "use strict";

    angular.module("softwrench").factory("crudContextHolderService", ["$log", "contextService", crudContextHolderService]);

    function crudContextHolderService($log, contextService) {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="$log"></param>
        /// <param name="contextService"></param>
        /// <returns type=""></returns>

        //#region Utils

    
        // ReSharper disable once InconsistentNaming
        var _crudContext = {

            currentApplicationName: null,
            currentApplication: null,
            currentTitle: null,

            currentListSchema: null,
            itemlist: null,
            filteredList: null,

            originalDetailItemDatamap: null,
            currentDetailItem: null,
            currentDetailSchema: null,
            currentNewDetailSchema: null,
            newItem: false,

            //composition
            composition: {
                currentTab: null,
                currentListSchema: null,
                itemlist: null,

                currentDetailItem: null,
                originalDetailItemDatamap: null,
                currentDetailSchema: null,


            },


            previousItem: null,
            nextItem: null,

        }

        //#endregion

        //#region Public methods

        function setPreviousAndNextItems(item) {
            if (!item) {
                return;
            }
            var itemlist = _crudContext.itemlist;
            var idx = itemlist.indexOf(item);
            if (idx == 0) {
                _crudContext.previousItem = null;
            } else {
                _crudContext.previousItem = itemlist[idx - 1];
            }
            if (idx >= itemlist.length - 2) {
                _crudContext.nextItem = null;
            } else {
                _crudContext.nextItem = itemlist[idx + 1];
            }
        }


        function restoreState() {
            if (!isRippleEmulator()) {
                return null; //this is used for F5 (refresh) upon development mode, so that we can return to the page we were before quickier
            }
            var savedCrudContext = contextService.getFromContext("crudcontext");
            if (savedCrudContext) {
                _crudContext = JSON.parse(savedCrudContext);
                if (_crudContext.itemlist) {
                    _crudContext.itemlist = [];
                }
                if (_crudContext.originalDetailItemDatamap) {
                    // the persistence entries do not get serialized correctly
                    _crudContext.originalDetailItemDatamap = angular.copy(_crudContext.originalDetailItemDatamap);
                    _crudContext.currentDetailItem.datamap = _crudContext.originalDetailItemDatamap;
                    setPreviousAndNextItems(savedCrudContext.currentDetailItem);
                }

            }
            $log.get("crudContextService#factory").debug("restoring state of crudcontext");
            return savedCrudContext;
        };

        function isList() {
            return _crudContext.currentDetailItem == null;
        };

        function getFilteredList() {
            return _crudContext.filteredList;
        };

        function currentTitle() {
            var tabTitle = this.tabTitle();
            if (tabTitle != null) {
                return _crudContext.currentTitle + " / " + tabTitle;
            }

            return _crudContext.currentTitle;
        };
        
        function currentApplicationName() {
            return _crudContext.currentApplicationName;
        }

        function currentListSchema() {
            return _crudContext.currentListSchema;
        }
        function currentDetailSchema() {
            if (_crudContext.newItem) {
                return _crudContext.currentNewDetailSchema ? _crudContext.currentNewDetailSchema : _crudContext.currentDetailSchema;
            }
            return _crudContext.currentDetailSchema;
        }

        function currentDetailItem() {
            return _crudContext.currentDetailItem;
        }

        function itemlist() {
            return _crudContext.itemlist;
        }

        function currentDetailItemDataMap() {
            if (_crudContext.composition.currentDetailItem) {
                return _crudContext.composition.currentDetailItem;
            }

            return _crudContext.currentDetailItem.datamap;
        }

        function leavingDetail() {
            _crudContext.composition = {};
            _crudContext.currentDetailItem = null;
        };

        function leavingCompositionDetail() {
            _crudContext.composition.currentDetailItem = null;
        };

        function isOnMainTab () {
            return _crudContext.composition.currentTab == null;
        };

        function resetTab() {
            _crudContext.composition.currentTab = null;
        };

        function tabTitle () {
            if (this.isOnMainTab()) {
                return null;
            }
            return _crudContext.composition.currentTab.label;
        };


        function compositionList () {
            return _crudContext.composition.itemlist;
        };

        function getCompositionListSchema() {
            return _crudContext.composition.currentListSchema;
        };

        function getCompositionDetailSchema() {
            return _crudContext.composition.currentDetailSchema;
        };

        function getCompositionDetailItem () {
            return _crudContext.composition.currentDetailItem;
        };

        function getCrudContext() {
            return _crudContext;
        }

        function hasDirtyChanges () {
            if (_crudContext.composition.currentDetailItem) {
                return _crudContext.composition.currentDetailItem && (!angular.equals(_crudContext.composition.originalDetailItemDatamap, _crudContext.composition.currentDetailItem));
            }

            return _crudContext.currentDetailItem && (!angular.equals(_crudContext.originalDetailItemDatamap, _crudContext.currentDetailItem.datamap));
        };

        //#endregion

        //#region Service Instance

        var service = {
            setPreviousAndNextItems: setPreviousAndNextItems,
            restoreState: restoreState,
            isList: isList,
            getFilteredList: getFilteredList,
            currentTitle: currentTitle,
            currentApplicationName: currentApplicationName,
            currentListSchema: currentListSchema,
            currentDetailSchema: currentDetailSchema,
            currentDetailItem: currentDetailItem,
            itemlist: itemlist,
            currentDetailItemDataMap: currentDetailItemDataMap,
            leavingDetail: leavingDetail,
            isOnMainTab: isOnMainTab,
            resetTab: resetTab,
            tabTitle: tabTitle,
            leavingCompositionDetail: leavingCompositionDetail,
            compositionList: compositionList,
            getCompositionListSchema: getCompositionListSchema,
            getCompositionDetailSchema: getCompositionDetailSchema,
            getCompositionDetailItem: getCompositionDetailItem,
            hasDirtyChanges:hasDirtyChanges,
            //below method to facilitate migration
            getCrudContext:getCrudContext
        };

        return service;

        //#endregion
    }

    //#region Service registration



    //#endregion

})(angular);




