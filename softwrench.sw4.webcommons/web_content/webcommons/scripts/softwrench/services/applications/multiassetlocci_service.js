
(function (angular) {
    'use strict';

    function multiassetlocciService($rootScope, crudContextHolderService, associationService, modalService, searchService) {

        //afterchange
        function afterChangeAsset(parameters) {

            if (parameters.fields['assetnum'] != null) {
                parameters.fields['location'] = parameters.fields['asset_.location'];
            }
        };

        //afterchange
        function afterChangeLocation(parameters) {

            if (parameters.fields['location'] != parameters.fields['asset_.location']) {
                parameters.fields['assetnum'] = null;
            }
        };

        function beforeAssetModalOpen(datamap, fieldSchema, fieldMetadata, props) {
            if (datamap.location) {
                props.searchData = {
                    location: datamap.location
                }
                props.searchOperator = {
                    location: searchService.getSearchOperationById("EQ")
                }
            }

            const assetId = datamap["asset_.assetuid"];
            if (datamap.assetnum && assetId) {
                props.searchSort = {
                    field: `custom:(case assetnum when '${datamap.assetnum}' then 1 else 2 end)`,
                    order: "asc"
                }
                crudContextHolderService.addSelectionToBuffer(assetId, { }, "#modal");
            }
            return {};
        }

        function assetsSelected(currentRowDm, schema, selecteditems, fieldMetadata) {
            const selectedAssets = crudContextHolderService.getSelectionModel("#modal").selectionBuffer;

            const datamaps = [];

            const currentRowAssetId = currentRowDm["asset_.assetuid"] + "";


            // checks if the current asset still on selection
            // if not uses the first one on the current row
            const keepOriginal = currentRowAssetId && selectedAssets[currentRowAssetId] && selectedAssets[currentRowAssetId].assetnum;
            let currentRowUpdated = false;


            // update lazy assoc options
            Object.keys(selectedAssets).map((key, idx) => {
                const asset = selectedAssets[key];
                if (!asset.assetnum) {
                    return;
                }

                // checks if the asset should go to the curren row datamap
                // if so it is not added to the array that is used to create new rows
                const isTheCurrentRowAsset = (!keepOriginal && !currentRowUpdated) || (keepOriginal && key === currentRowAssetId);
                const datamap = isTheCurrentRowAsset ? currentRowDm : { location: asset.location };
                datamap.assetnum = asset.assetnum;
                datamap["asset_.assetuid"] = asset.assetuid;

                const option = {
                    value: asset.assetnum,
                    label: asset.description,
                    type: "MultiValueAssociationOption",
                    extrafields: asset
                }

                associationService.updateUnderlyingAssociationObject(fieldMetadata, new AssociationOptionDTO(option), { datamap, schema });
                if (!isTheCurrentRowAsset) {
                    datamaps.push(datamap);
                } else {
                    currentRowUpdated = true;
                }
            });

            if (datamaps.length > 0) {
                $rootScope.$broadcast(JavascriptEventConstants.CompositionBatchAddMultiple, "multiassetlocci_", datamaps);
            }
            modalService.hide();
        }


        const service = {
            afterChangeAsset,
            afterChangeLocation,
            beforeAssetModalOpen,
            assetsSelected
        };

        return service;


    }

    angular.module('sw_layout').service('multiassetlocciService', ["$rootScope", "crudContextHolderService", "associationService", "modalService", "searchService", multiassetlocciService]);

})(angular);
