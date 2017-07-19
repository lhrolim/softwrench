(function (softwrench) {
    "use strict";

    softwrench.controller('MultiAssetController', MultiAssetController);
    function MultiAssetController($log, $scope, $q, crudContextHolderService, offlineAssociationService, inlineCompositionService) {
        "ngInject";

        angular.forEach(crudContextHolderService.currentDetailSchema().displayables, function (d) {
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
                promisses.push(loadAssociation(multiasset, "assetnum", "offlineasset_", "description", associationDataEntry));
                promisses.push(loadAssociation(multiasset, "location", "offlinelocation_", "description", associationDataEntry));
                $scope.associationData[multiasset["multiid"]] = associationDataEntry;
            });
            $q.all(promisses).then(() => $scope.loaded = true).catch((e) => console.log(e));
        }
        load();

        function loadAssociation(parentDatamap, valueField, association, labelField, associationDataEntry) {
            if (!associationDataEntry[association]) {
                associationDataEntry[association] = {};
            }
            const value = parentDatamap[valueField];
            associationDataEntry[association][value] = {};
            if (!value) {
                return $q.when(null);
            }
            return offlineAssociationService.filterPromise($scope.detailSchema, parentDatamap, association, `"${valueField}":"${value}"`, null, true).then((assocs) => {
                const label = assocs && assocs[0] ? assocs[0].datamap[labelField] : null;
                associationDataEntry[association][value] = label ? `${value} - ${label}` : value;
            });
        }

        $scope.$on(inlineCompositionService.compositionLoadedEventName(), () => {
            load();
        });

        $scope.options = {
            loop: false,
            effect: "slide",
            speed: 500
        }

        $scope.toggleProcess = function(multiasset) {
            multiasset.progress = multiasset.progress ? 0 : 1;
            multiasset["#isDirty"] = "true";
        }

        $scope.$on("$ionicSlides.sliderInitialized", function (event, data) {
            $scope.slider = data.slider;
        });

        $scope.$on("$ionicSlides.slideChangeEnd", function (event, data) {
            $scope.activeIndex = data.slider.activeIndex;
            $scope.previousIndex = data.slider.previousIndex;
        });
    }

    window.MultiAssetController = MultiAssetController;
})(softwrench);



