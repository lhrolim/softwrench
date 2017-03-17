
(function (angular) {
    "use strict";

    angular.module("softwrench").factory("crudContextHolderService", ["$log","$state", "contextService", crudContextHolderService]);

    function crudContextHolderService($log,$state, contextService) {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="$log"></param>
        /// <param name="contextService"></param>
        /// <returns type=""></returns>

        //#region Utils
        const quickSearchValue = "_quicksearch";
        const defaultSortValue = "default";

        var initialContext = {
            currentApplicationName: null,
            currentApplication: null,
            currentTitle: null,

            currentListSchema: null,
            itemlist: null,

            originalDetailItemDatamap: null,
            currentDetailItem: null,
            currentProblems: null,
            currentDetailSchema: null,
            currentNewDetailSchema: null,
            newItem: false,

            // grid search
            gridSearch: {
                searchFields: {}, // the searchable fields as a object with attribute as key
                searchValues: {}, // all the search values as a object with attribute as key
                sortables: {}, // the sortableFields as a object with attribute as key
                sortableFields: [], // the sortableFields as a array
                sort: {}, // sort value
                count: 0
            },

            //composition
            composition: {
                currentTab: null,
                currentListSchema: null,
                itemlist: null,

                currentDetailItem: null,
                originalDetailItemDatamap: null,
                currentDetailSchema: null,


            },

            // indexes for searching and ordering
            indexes: null,

            previousItem: null,
            nextItem: null,
            wizardStateIndex: 0
        };

        // inits the search and sort data with defaults
        function defaultGridSearch(gridSearch) {
            gridSearch.searchFields[quickSearchValue] = {
                label: "Quick Search",
                value: quickSearchValue,
                type: "BaseMetadataFilter"
            }
            const defaultSort = {
                label: "Default",
                value: defaultSortValue
            }
            gridSearch.sortables[defaultSortValue] = defaultSort;
            gridSearch.sortableFields.push(defaultSort);
            gridSearch.sort = defaultSort;
        }

        defaultGridSearch(initialContext.gridSearch);

        // ReSharper disable once InconsistentNaming
        var _crudContext = angular.copy(initialContext);

        //#endregion

        //#region Public methods

        function setPreviousAndNextItems(item) {
            if (!item) {
                return;
            }
            const itemlist = _crudContext.itemlist;
            const idx = itemlist.indexOf(item);
            if (idx === 0) { // first on the list: has no previous
                _crudContext.previousItem = null;
            } else {
                _crudContext.previousItem = itemlist[idx - 1];
            }
            if (idx === itemlist.length - 1) { // last on the list: has no next
                _crudContext.nextItem = null;
            } else {
                _crudContext.nextItem = itemlist[idx + 1];
            }
        }


        function restoreState() {
            if (!isRippleEmulator()) {
                return null; //this is used for F5 (refresh) upon development mode, so that we can return to the page we were before quickier
            }
            const savedCrudContext = contextService.getFromContext("crudcontext");
            if (savedCrudContext) {
                _crudContext = JSON.parse(savedCrudContext);
                if (_crudContext.itemlist) {
                    _crudContext.itemlist = [];
                }
                if (_crudContext.originalDetailItemDatamap) {
                    // the persistence entries do not get serialized correctly
                    _crudContext.currentDetailItem = _crudContext.currentDetailItem || {};
                    _crudContext.originalDetailItemDatamap = angular.copy(_crudContext.originalDetailItemDatamap);
                    _crudContext.currentDetailItem.datamap = _crudContext.originalDetailItemDatamap;
                    setPreviousAndNextItems(savedCrudContext.currentDetailItem);
                }

            }
            $log.get("crudContextService#factory").debug("restoring state of crudcontext");
            return savedCrudContext;
        };

        function isList() {
            return $state.current.name === "main.crudlist";
        };

        function currentTitle() {
            const tabTitle = this.tabTitle();
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

        function currentSchema() {
            const listSchema = _crudContext.currentListSchema;
            const detailSchema = _crudContext.currentDetailSchema;
            if (!listSchema) {
                return detailSchema;
            } else if (!detailSchema) {
                return listSchema;
            }
            //both are defined, bigger devices
            return [listSchema, detailSchema];
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

        function currentProblems() {
            return _crudContext.currentProblems;
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

        function isOnMainTab() {
            return _crudContext.composition.currentTab == null;
        };

        function resetTab() {
            _crudContext.composition.currentTab = null;
        };

        function tabTitle() {
            if (this.isOnMainTab()) {
                return null;
            }
            return _crudContext.composition.currentTab.label;
        };

        function tabIcon() {
            if (this.isOnMainTab()) {
                return null;
            }
            return _crudContext.composition.currentTab.schema.schemas.list.properties['icon.composition.tab'];
        };

        function getActiveTab() {
            if (this.isOnMainTab()) {
                return null;
            }
            return _crudContext.composition.currentTab.id;
        }

        function compositionList() {
            return _crudContext.composition.itemlist;
        };

        function getCompositionListSchema() {
            return _crudContext.composition.currentListSchema;
        };

        function getCompositionDetailSchema() {
            return _crudContext.composition.currentDetailSchema;
        };

        function getCompositionDetailItem() {
            return _crudContext.composition.currentDetailItem;
        };

        function getGridSearchData() {
            return _crudContext.gridSearch;
        }

        // parse the indexes from the application props
        function parseIndexes(key, props) {
            const indexes = [];
            const indexesString = props[key];

            if (!indexesString) {
                return indexes;
            }
            angular.forEach(indexesString.split(","), index => {
                const trimmed = index.trim();
                if (trimmed) {
                    indexes.push(trimmed);
                }
            });
            return indexes;
        }

        function getIndexes() {
            if (_crudContext.indexes) {
                return _crudContext.indexes;
            }

            _crudContext.indexes = {};

            const app = _crudContext.currentApplication;
            if (!app || !app.data || !app.data.properties) {
                return _crudContext.indexes;
            }

            _crudContext.indexes.textIndexes = parseIndexes("list.offline.text.indexlist", app.data.properties);
            _crudContext.indexes.numericIndexes = parseIndexes("list.offline.numeric.indexlist", app.data.properties);
            _crudContext.indexes.dateIndexes = parseIndexes("list.offline.date.indexlist", app.data.properties);
            return _crudContext.indexes;
        }

        // clears the search and sort values
        function clearGridSearchValues() {
            angular.forEach(_crudContext.gridSearch.searchFields, (searchable, attribute) => {
                if (_crudContext.gridSearch.searchFields.hasOwnProperty(attribute)) {
                    _crudContext.gridSearch.searchValues[attribute] = {};
                }
            });
            _crudContext.gridSearch.sort = _crudContext.gridSearch.sortables[defaultSortValue];
        }

        // reset search and sort structure to default - not only values
        function clearGridSearch() {
            _crudContext.gridSearch = {
                searchFields: {},
                searchValues: {},
                sortables: {},
                sortableFields: [],
                sort: {}
            }
            defaultGridSearch(_crudContext.gridSearch);
        }

        function getQuickSearch() {
            if (!_crudContext.gridSearch.searchValues[quickSearchValue]) {
                _crudContext.gridSearch.searchValues[quickSearchValue] = {};
            }
            return _crudContext.gridSearch.searchValues[quickSearchValue];
        }

        function getCrudContext() {
            return _crudContext;
        }

        function reset() {
            _crudContext = angular.copy(initialContext);
            defaultGridSearch(_crudContext.gridSearch);
        }

        function hasDirtyChanges() {
            if (_crudContext.composition.currentDetailItem) {
                return _crudContext.composition.currentDetailItem && (!angular.equals(_crudContext.composition.originalDetailItemDatamap, _crudContext.composition.currentDetailItem));
            }

            return _crudContext.currentDetailItem && (!angular.equals(_crudContext.originalDetailItemDatamap, _crudContext.currentDetailItem.datamap));
        };

        //#endregion

        //#region Service Instance

        const service = {
            setPreviousAndNextItems,
            restoreState,
            isList,
            currentTitle,
            currentApplicationName,
            currentListSchema,
            currentDetailSchema,
            currentDetailItem,
            currentProblems,
            currentSchema,
            itemlist,
            currentDetailItemDataMap,
            leavingDetail,
            isOnMainTab,
            resetTab,
            tabTitle,
            tabIcon,
            leavingCompositionDetail,
            compositionList,
            getCompositionListSchema,
            getCompositionDetailSchema,
            getCompositionDetailItem,
            getGridSearchData,
            getIndexes,
            clearGridSearchValues,
            clearGridSearch,
            getQuickSearch,
            hasDirtyChanges,
            getActiveTab,
            reset,
            //below method to facilitate migration
            getCrudContext

        };

        return service;

        //#endregion
    }

    //#region Service registration



    //#endregion

})(angular);
