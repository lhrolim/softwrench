!(function (angular) {
    "use strict";

    function drillDownService($q, swdbDAO, crudContextHolderService, securityService) {
        //#region Utils
        const dd = () => crudContextHolderService.getCrudContext().drillDown;

        const getCurrentLocation = function() {
            const drillDown = crudContextHolderService.getCrudContext().drillDown;
            return drillDown.selectedLocation ? drillDown.selectedLocation.datamap.location : null;
        }

        const buildLocationClause = function (start, end) {
            const currentLocation = getCurrentLocation();
            if (currentLocation) {
                return ` (${start}${currentLocation}${end}) `;
            }

            const siteid = securityService.currentFullUser().SiteId;
            const pm = `PM${siteid}`;
            return ` (${start}${siteid}${end} OR ${start}${pm}${end} OR ${start}140${end} OR ${start}180${end}) `;
        }

        const updatePaginationOptions = function (drillDown) {
            drillDown.page++;
            drillDown.moreItemsAvailable = false;
            return { pagesize: 20, pageNumber: drillDown.page }
        }

        const setMoreItemsAvailable = function(drillDown, list) {
            drillDown.moreItemsAvailable = list.length === 20;
        }

        const locationSearchWc = (drillDown, alias) => drillDown.locationQuery ? ` and ${alias ? `\`${alias}\`.` : ""}datamap like '%${drillDown.locationQuery}%'` : "";

        const locationsQuery = function () {
            const drillDown = dd();
            if (drillDown.locationQuery) {
                const locationClause = buildLocationClause("`root`.textindex04 like '%/", "/%'");
                return ` \`root\`.application = 'offlinelocation' and ${locationClause} ${locationSearchWc(drillDown, "root")} order by \`root\`.textindex01`;
            }
            const locationClause = buildLocationClause("`root`.datamap like '%\"parent\":\"", "\"%'");
            return ` \`root\`.application = 'offlinelocation' and ${locationClause} order by \`root\`.textindex01`;
        }

        const assetSearchWc = (drillDown) => drillDown.assetQuery ? ` and \`root\`.datamap like '%${drillDown.assetQuery}%'` : "";

        const assetQuery = function (order) {
            const drillDown = dd();
            const orderClause = order ? " order by `root`.textindex02 " : "";
            const locationClause1 = buildLocationClause("`root`.textindex01 = '", "'");
            const locationClause2 = buildLocationClause("textindex04 like '%/", "/%'");
            return ` \`root\`.application = 'offlineasset' ${assetSearchWc(drillDown)} and (${locationClause1} or \`root\`.textindex01 in (select textindex01 from AssociationData where application = 'offlinelocation' and ${locationClause2})) ${orderClause} `;
        }
        //#endregion

        const drillDownClear = function () {
            crudContextHolderService.drillDownClear();
        }

        const isOnDrillDown = function () {
            return crudContextHolderService.isOnDrillDown();
        }

        const getDrillDown = function () {
            return dd();
        }

        const updateDrillDownLocations = function () {
            const drillDown = dd();
            drillDown.page = 0;

            const promises = [];
            promises.push(swdbDAO.findByQuery("AssociationData", locationsQuery(), updatePaginationOptions(drillDown)));

            const locationClause1 = buildLocationClause("`root`.textindex04 like '%/", "/%'");
            promises.push(swdbDAO.countByQuery("AssociationData", ` \`root\`.application = 'offlinelocation' and ${locationClause1} ${locationSearchWc(drillDown, "root")}`));

            const locationClause2 = buildLocationClause("`root`.textindex01 = '", "'");
            const locationClause3 = buildLocationClause("textindex04 like '%/", "/%'");
            promises.push(swdbDAO.countByQuery("AssociationData", ` \`root\`.application = 'offlineasset' and (${locationClause2} or \`root\`.textindex01 in (select textindex01 from AssociationData where application = 'offlinelocation' and ${locationClause3}))`));

            return $q.all(promises).then((results) => {
                setMoreItemsAvailable(drillDown, results[0]);
                drillDown.locations = results[0];
                drillDown.locationsCount = results[1];
                drillDown.assetsCount = results[2];
                return drillDown.locations;
            });
        }

        const locationDrillDownClick = function (location) {
            const drillDown = dd();
            drillDown.locationHistory.push({
                location: drillDown.selectedLocation,
                query: drillDown.locationQuery
            });
            drillDown.selectedLocation = location;
            drillDown.locationQuery = undefined;
            return this.updateDrillDownLocations();
        }

        const drillDownBack = function () {
            const drillDown = dd();
            if (drillDown.assetView) {
                drillDown.assetView = false;
                return true;
            }

            if (drillDown.locationHistory.length === 0) {
                return false;
            }

            const locationHistoryEntry = drillDown.locationHistory.pop();
            drillDown.selectedLocation = locationHistoryEntry.location;
            drillDown.locationQuery = locationHistoryEntry.query;
            this.updateDrillDownLocations();
            return true;
        }

        const updateDrillDownAssets = function () {
            const drillDown = dd();
            drillDown.page = 0;

            const promises = [];
            promises.push(swdbDAO.findByQuery("AssociationData", assetQuery(true), updatePaginationOptions(drillDown)));
            promises.push(swdbDAO.countByQuery("AssociationData", assetQuery()));

            return $q.all(promises).then((results) => {
                setMoreItemsAvailable(drillDown, results[0]);
                drillDown.assets = results[0];
                drillDown.assetsCount = results[1];
                return drillDown.assets;
            });
        }

        const assetView = function () {
            const drillDown = dd();
            drillDown.assetView = true;
            drillDown.assetQuery = undefined;
            return this.updateDrillDownAssets();
        }

        const findAsset = function(assetNum) {
            return swdbDAO.findByQuery("AssociationData", ` \`root\`.application = 'offlineasset' and \`root\`.textindex02 = '${assetNum}'`).then((assets) => {
                return assets[0];
            });
        }

        const loadMore = function () {
            const drillDown = dd();
            if (!drillDown.assetView) {
                swdbDAO.findByQuery("AssociationData", locationsQuery(), updatePaginationOptions(drillDown)).then((newLocations) => {
                    setMoreItemsAvailable(drillDown, newLocations);
                    drillDown.locations = drillDown.locations.concat(newLocations);
                });
            } else {
                swdbDAO.findByQuery("AssociationData", assetQuery(true), updatePaginationOptions(drillDown)).then((newAssets) => {
                    setMoreItemsAvailable(drillDown, newAssets);
                    drillDown.assets = drillDown.assets.concat(newAssets);
                });
            }
        }

        const service = {
            drillDownClear,
            isOnDrillDown,
            getDrillDown,
            updateDrillDownLocations,
            locationDrillDownClick,
            drillDownBack,
            updateDrillDownAssets,
            assetView,
            findAsset,
            loadMore
        };

        return service;
    }

    mobileServices.factory("drillDownService", ["$q", "swdbDAO", "crudContextHolderService", "securityService", drillDownService]);
})(angular);