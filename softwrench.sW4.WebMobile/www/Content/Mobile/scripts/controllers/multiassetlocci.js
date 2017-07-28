(function (softwrench) {
    "use strict";

    softwrench.controller('MultiAssetController', MultiAssetController);
    function MultiAssetController($log, $scope, $q, crudContextHolderService, offlineAssociationService, inlineCompositionService) {
        "ngInject";

        const crudContext = crudContextHolderService.getCrudContext();
        const multiAssetTab = crudContext.composition.currentTab;

        angular.forEach(multiAssetTab.displayables, function (d) {
            if (d.attribute === "multiassetlocci") {
                $scope.fieldMetadata = d;
            }
        });

        $scope.detailSchema = $scope.fieldMetadata.schema.schemas.detail;
        $scope.associationData = {};
        $scope.loaded = false;

        function load() {
            $scope.loaded = false;
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
            $q.all(promisses).then(() => $scope.loaded = true).catch((e) => console.log(e));
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

        $scope.$on(inlineCompositionService.compositionLoadedEventName(), () => {
            load();
        });

        $scope.toggleProcess = function(multiasset) {
            multiasset.progress = multiasset.progress ? 0 : 1;
            multiasset["#isDirty"] = "true";
        }
    }

    window.MultiAssetController = MultiAssetController;
})(softwrench);



