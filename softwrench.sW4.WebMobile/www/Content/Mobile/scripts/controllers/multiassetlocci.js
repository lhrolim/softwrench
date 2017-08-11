(function (softwrench) {
    "use strict";

    window.MultiAssetController = function ($log, $scope, $q, crudContextHolderService, offlineAssociationService, inlineCompositionService) {

        const multiAssetLog = $log.get("multiasset");

        const crudContext = crudContextHolderService.getCrudContext();
        const multiAssetTab = crudContext.composition.currentTab;

        angular.forEach(multiAssetTab.displayables, function (d) {
            if (d.relationship === "multiassetlocci_") {
                $scope.fieldMetadata = d;
            }
        });

        $scope.detailSchema = $scope.fieldMetadata.schema.schemas.detail;
        $scope.associationData = {};
        $scope.loaded = false;

        function setIonicPaneDivHeight(height) {
            const fullheight = (height + 13) + "px"; // 13 = margins, paddings and borders
            $("[state='main.cruddetail.tab']").css("height", fullheight);
        }

        function load() {
            multiAssetLog.debug("loading multiassets");
            $scope.loaded = false;
            setIonicPaneDivHeight(100); // 100 = loading height
            const dm = crudContextHolderService.currentDetailItemDataMap();
            $scope.multiassets = dm["multiassetlocci_"] || [];
            $scope.associationData = {};
            const promisses = [];
            angular.forEach($scope.multiassets, (multiasset) => {
                const associationDataEntry = {};
                promisses.push(loadAsset(multiasset, associationDataEntry));
                associationDataEntry["offlinelocation_"] = multiasset["location"];
                $scope.associationData[multiasset["multiid"]] = associationDataEntry;
            });
            $q.all(promisses).then(() => {
                if ($scope.multiassets && $scope.multiassets.length > 0) {
                    setIonicPaneDivHeight(75 * $scope.multiassets.length); // 75 = row height
                } else {
                    setIonicPaneDivHeight(16); // 16 = no records message height
                }
                $scope.loaded = true;
            }).catch((e) => console.log(e));
        }

        load();

        function loadAsset(parentDatamap, associationDataEntry) {
            const assetNum = parentDatamap["assetnum"];
            associationDataEntry["offlineasset_"] = assetNum;
            if (!assetNum) {
                return $q.when(null);
            }
            return offlineAssociationService.filterPromise($scope.detailSchema, parentDatamap, "offlineasset_", `"assetnum":"${assetNum}"`, null, true).then((assocs) => {
                associationDataEntry["assetdescription"] = assocs && assocs[0] ? assocs[0].datamap["description"] : null;
            });
        }

        $scope.$on(inlineCompositionService.compositionLoadedEventName(), (event, tabid) => load());

        $scope.toggleProcess = function (multiasset) {
            multiasset.progress = multiasset.progress ? 0 : 1;
            multiasset["#isDirty"] = "true";
        }
    }

    softwrench.controller("MultiAssetController", ["$log", "$scope", "$q", "crudContextHolderService", "offlineAssociationService", "inlineCompositionService", window.MultiAssetController]);
})(softwrench);