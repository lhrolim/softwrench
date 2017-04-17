!(function (angular) {
    "use strict";

    function drillDownService($q, swdbDAO, crudContextHolderService, securityService) {
        //#region Utils
        const dd = () => crudContextHolderService.getCrudContext().drillDown;

        const getCurrentLocation = function() {
            const drillDown = crudContextHolderService.getCrudContext().drillDown;
            return drillDown.selectedLocation ? drillDown.selectedLocation.datamap.location : securityService.currentFullUser().SiteId;
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
            const currentLocation = getCurrentLocation();
            if (drillDown.locationQuery) {
                return ` \`root\`.application = 'offlinelocation' and \`root\`.textindex04 like '%/${currentLocation}/%' ${locationSearchWc(drillDown, "root")} order by \`root\`.textindex01`;
            }
            return ` \`root\`.application = 'offlinelocation' and \`root\`.datamap like '%"parent":"${currentLocation}"%' order by \`root\`.textindex01`;
        }

        const assetSearchWc = (drillDown) => drillDown.assetQuery ? ` and \`root\`.datamap like '%${drillDown.assetQuery}%'` : "";

        const assetQuery = function (order) {
            const drillDown = dd();
            const currentLocation = getCurrentLocation();
            const orderClause = order ? " order by `root`.textindex02 " : "";
            return ` \`root\`.application = 'offlineasset' ${assetSearchWc(drillDown)} and (\`root\`.textindex01 = '${currentLocation}' or \`root\`.textindex01 in (select textindex01 from AssociationData where application = 'offlinelocation' and textindex04 like '%/${currentLocation}/%')) ${orderClause} `;
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
            const currentLocation = getCurrentLocation();

            const promises = [];
            promises.push(swdbDAO.findByQuery("AssociationData", locationsQuery(), updatePaginationOptions(drillDown)));
            promises.push(swdbDAO.countByQuery("AssociationData", ` \`root\`.application = 'offlinelocation' and \`root\`.textindex04 like '%/${currentLocation}/%' ${locationSearchWc(drillDown, "root")}`));
            promises.push(swdbDAO.countByQuery("AssociationData", ` \`root\`.application = 'offlineasset' and (\`root\`.textindex01 = '${currentLocation}' or \`root\`.textindex01 in (select textindex01 from AssociationData where application = 'offlinelocation' and textindex04 like '%/${currentLocation}/%'))`));

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